using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain;
using CigralBackend.Domain.Enums;
using CigralBackend.Domain.Exceptions;
using CigralBackend.Infraestructure.Database.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services
{
    /// <summary>
    /// Correcciones de soporte sobre remitos ya emitidos: líneas (lote, serie, vencimiento, cantidad,
    /// quitar, agregar) y cliente/proveedor. Con <c>AjustarStock</c>, el stock pasa del efecto viejo de la
    /// línea al nuevo en el depósito del remito; sin él, solo cambia el documento.
    /// </summary>
    public class CorreccionRemitoService : ICorreccionRemitoService
    {
        private readonly IRepository _repository;
        private readonly IExistenciaService _existencias;
        private readonly IHttpContextAccessor _http;

        public CorreccionRemitoService(IRepository repository, IExistenciaService existencias, IHttpContextAccessor http)
        {
            _repository = repository;
            _existencias = existencias;
            _http = http;
        }

        public Task<RemitoResponseGet> CorregirLinea(bool esIngreso, int remitoId, int detalleId, CorreccionLineaRequest r)
            => AplicarLinea(esIngreso, remitoId, detalleId, null, r);

        public Task<RemitoResponseGet> AgregarLinea(bool esIngreso, int remitoId, AgregarLineaRequest r)
            => AplicarLinea(esIngreso, remitoId, null, r.ProductoId, r);

        private async Task<RemitoResponseGet> AplicarLinea(bool esIngreso, int remitoId, int? detalleId, int? productoNuevoId, CorreccionLineaRequest r)
        {
            var motivo = CorreccionSoporte.Motivo(r.TicketId, r.Motivo);
            var (numero, depositoId) = await Cabecera(esIngreso, remitoId);

            DetalleRemito? detalle = null;
            if (detalleId.HasValue)
            {
                detalle = await _repository.GetById<DetalleRemito>(detalleId.Value);
                if (detalle == null || (esIngreso ? detalle.RemitoIngresoId : detalle.RemitoEgresoId) != remitoId)
                    throw new NotFoundException("Línea del remito", detalleId.Value);
            }

            var productoId = detalle?.ProductoId ?? productoNuevoId!.Value;
            var producto = await _repository.GetById<Producto>(productoId)
                ?? throw new NotFoundException(nameof(Producto), productoId);
            var ticket = await CorreccionSoporte.Ticket(_repository, r.TicketId);

            var quitar = detalle != null && r.Cantidad == 0;
            var loteNuevo = string.IsNullOrWhiteSpace(r.CodigoLote) ? null : r.CodigoLote.Trim().ToUpper();
            var serieNueva = string.IsNullOrWhiteSpace(r.NumeroSerie) ? null : r.NumeroSerie.Trim();
            var vencNuevo = r.FechaVencimiento?.Date;
            var loteAnterior = detalle?.LoteId is int idLote ? (await _repository.GetById<Lote>(idLote))?.CodigoLote?.ToUpper() : null;

            if (detalle == null && r.Cantidad <= 0)
                throw new DomainException(DomainErrorCode.CantidadInvalida, "La cantidad debe ser mayor a 0.");
            if (!quitar && serieNueva == null && loteNuevo == null && string.IsNullOrEmpty(producto.CodigoGenerico))
                throw new DomainException(DomainErrorCode.SerieYCodigoLoteNoEspecificados,
                    "La línea tiene que tener número de serie, código de lote o el producto un código genérico.");
            if (producto.EsUnitario && r.Cantidad > 1)
                throw new DomainException(DomainErrorCode.ProductoUnitarioCantidadInvalida, "Un producto unitario va de a 1 unidad por línea.");

            var cambiaLote = detalle != null && loteNuevo != loteAnterior;
            var cambiaSerie = detalle != null && serieNueva != detalle.NumeroSerie;
            var cambiaVenc = detalle != null && vencNuevo != detalle.FechaVencimiento?.Date;
            var cambiaCantidad = detalle != null && r.Cantidad != detalle.Cantidad;
            if (detalle != null && !quitar && !cambiaLote && !cambiaSerie && !cambiaVenc && !cambiaCantidad)
                throw new DomainException(DomainErrorCode.CorreccionSinCambios, "No hay cambios para guardar.");

            if (quitar)
            {
                var lineas = await _repository.GetFiltered<DetalleRemito>(
                    d => esIngreso ? d.RemitoIngresoId == remitoId : d.RemitoEgresoId == remitoId, 1, 2);
                if (lineas.TotalCount <= 1)
                    throw new DomainException(DomainErrorCode.RemitoSinDetalles, "No se puede quitar la única línea del remito.");
            }

            // Valores anteriores de la línea, para el comentario (el detalle se modifica más abajo).
            var serieAnterior = detalle?.NumeroSerie;
            var vencAnterior = detalle?.FechaVencimiento;
            var cantidadAnterior = detalle?.Cantidad ?? 0;

            using var transaction = await _repository.BeginTransaction();

            Lote? lote = null;
            if (!quitar && loteNuevo != null)
                lote = await _repository.First<Lote>(l => l.CodigoLote == loteNuevo)
                    ?? await _repository.Add(new Lote { CodigoLote = loteNuevo });

            var observaciones = CorreccionSoporte.Observaciones(ticket, motivo, $"Remito {numero}");
            if (r.AjustarStock)
                await AjustarStock(esIngreso, remitoId, depositoId, productoId, detalle, quitar, lote, serieNueva, vencNuevo, r.Cantidad, cambiaVenc, loteAnterior, observaciones);

            // El documento
            if (quitar)
            {
                await _repository.Delete(detalle!);
            }
            else if (detalle == null)
            {
                await _repository.Add(new DetalleRemito
                {
                    RemitoIngresoId = esIngreso ? remitoId : null,
                    RemitoEgresoId = esIngreso ? null : remitoId,
                    ProductoId = productoId,
                    LoteId = lote?.Id,
                    NumeroSerie = serieNueva,
                    FechaVencimiento = r.FechaVencimiento,
                    Cantidad = r.Cantidad,
                });
            }
            else
            {
                detalle.LoteId = lote?.Id;
                detalle.NumeroSerie = serieNueva;
                detalle.FechaVencimiento = r.FechaVencimiento;
                detalle.Cantidad = r.Cantidad;
                await _repository.Update(detalle);
            }

            if (ticket != null)
            {
                var v = CorreccionSoporte.Valor;
                string texto;
                if (detalle == null)
                    texto = $"Remito {numero}: se agregó {producto.Nombre} (lote {v(loteNuevo)}, serie {v(serieNueva)}, vence {CorreccionSoporte.Fecha(vencNuevo)}, {r.Cantidad} u.).";
                else if (quitar)
                    texto = $"Remito {numero}: se quitó la línea de {producto.Nombre} (lote {v(loteAnterior)}, {cantidadAnterior} u.).";
                else
                {
                    var cambios = new List<string>();
                    if (cambiaLote) cambios.Add($"lote {v(loteAnterior)} → {v(loteNuevo)}");
                    if (cambiaSerie) cambios.Add($"serie {v(serieAnterior)} → {v(serieNueva)}");
                    if (cambiaVenc) cambios.Add($"vencimiento {CorreccionSoporte.Fecha(vencAnterior)} → {CorreccionSoporte.Fecha(vencNuevo)}");
                    if (cambiaCantidad) cambios.Add($"cantidad {cantidadAnterior} → {r.Cantidad}");
                    texto = $"Remito {numero}, línea de {producto.Nombre} corregida: {string.Join(", ", cambios)}.";
                }
                texto += r.AjustarStock ? "" : " (solo el documento; el stock no se tocó)";
                await CorreccionSoporte.Comentar(_repository, _http, ticket, texto, motivo);
            }

            await transaction.CommitAsync();
            return await Obtener(esIngreso, remitoId);
        }

        /// <summary>
        /// Lleva el stock del efecto viejo de la línea al nuevo. Un ingreso sumó en el depósito del remito
        /// (se resta de esa existencia exacta); un egreso descontó (se devuelve al depósito del remito y
        /// lo nuevo se descuenta como en cualquier egreso).
        /// </summary>
        private async Task AjustarStock(bool esIngreso, int remitoId, int depositoId, int productoId, DetalleRemito? detalle, bool quitar,
            Lote? lote, string? serieNueva, DateTime? vencNuevo, int cantidadNueva, bool cambiaVenc, string? loteAnterior, string observaciones)
        {
            int? remIngreso = esIngreso ? remitoId : null;
            int? remEgreso = esIngreso ? null : remitoId;
            var mismaClave = detalle != null && !quitar && detalle.LoteId == lote?.Id && detalle.NumeroSerie == serieNueva;

            // En detalle, los valores todavía son los viejos (el documento se actualiza después).
            if (mismaClave)
            {
                var diferencia = cantidadNueva - detalle!.Cantidad;
                if (esIngreso)
                    await Revertir(() => _existencias.AjustarExistenciaExacta(depositoId, productoId, lote?.Id, serieNueva, vencNuevo,
                        diferencia, observaciones, remIngreso, remEgreso, actualizarVencimiento: cambiaVenc), loteAnterior);
                else if (diferencia > 0)
                    await Descontar(depositoId, productoId, lote?.CodigoLote, serieNueva, vencNuevo, diferencia, observaciones);
                else if (diferencia < 0)
                    await _existencias.AjustarExistenciaExacta(depositoId, productoId, lote?.Id, serieNueva, vencNuevo,
                        -diferencia, observaciones, remIngreso, remEgreso);
                return;
            }

            if (detalle != null)
            {
                if (esIngreso)
                    await Revertir(() => _existencias.AjustarExistenciaExacta(depositoId, productoId, detalle.LoteId, detalle.NumeroSerie,
                        detalle.FechaVencimiento, -detalle.Cantidad, observaciones, remIngreso, remEgreso), loteAnterior);
                else
                    await _existencias.AjustarExistenciaExacta(depositoId, productoId, detalle.LoteId, detalle.NumeroSerie,
                        detalle.FechaVencimiento, detalle.Cantidad, observaciones, remIngreso, remEgreso);
            }

            if (!quitar)
            {
                if (esIngreso)
                    await _existencias.AjustarExistenciaExacta(depositoId, productoId, lote?.Id, serieNueva, vencNuevo,
                        cantidadNueva, observaciones, remIngreso, remEgreso, actualizarVencimiento: vencNuevo != null);
                else
                    await Descontar(depositoId, productoId, lote?.CodigoLote, serieNueva, vencNuevo, cantidadNueva, observaciones);
            }
        }

        /// <summary>Si lo que entró con el ingreso ya no está, explica qué hacer en vez del mensaje genérico.</summary>
        private static async Task Revertir(Func<Task> accion, string? loteAnterior)
        {
            try
            {
                await accion();
            }
            catch (DomainException ex) when (ex.Code == DomainErrorCode.CorreccionStockYaMovido)
            {
                throw new DomainException(DomainErrorCode.CorreccionStockYaMovido,
                    $"Del lote {CorreccionSoporte.Valor(loteAnterior)} ya salió mercadería de este ingreso: {ex.Message} " +
                    "Podés corregir solo el documento, o corregir también los remitos de egreso.");
            }
        }

        private async Task Descontar(int depositoId, int productoId, string? codigoLote, string? serie, DateTime? venc, int cantidad, string observaciones)
        {
            // Igual que un egreso: descuenta del producto/lote/serie esté en el depósito que esté y respeta consignaciones.
            try
            {
                await _existencias.DisminuirStock(
                    new ExistenciaModelRequest(depositoId, productoId, serie, codigoLote, venc, cantidad, null),
                    remitoEgresoId: null,
                    observaciones: observaciones);
            }
            catch (NotFoundException)
            {
                throw new DomainException(DomainErrorCode.StockInsuficiente,
                    $"No hay stock del lote {CorreccionSoporte.Valor(codigoLote)}" +
                    (serie != null ? $", serie {serie}," : "") + " para descontar en este egreso.");
            }
        }

        public async Task<RemitoResponseGet> CorregirEntidad(bool esIngreso, int remitoId, CorreccionEntidadRequest r)
        {
            var motivo = CorreccionSoporte.Motivo(r.TicketId, r.Motivo);
            var ticket = await CorreccionSoporte.Ticket(_repository, r.TicketId);
            string anterior, nuevo, numero;

            if (esIngreso)
            {
                var remito = await _repository.GetById<RemitoIngreso>(remitoId) ?? throw new NotFoundException(nameof(RemitoIngreso), remitoId);
                if (remito.ProveedorId == r.EntidadId)
                    throw new DomainException(DomainErrorCode.CorreccionSinCambios, "Ya es el proveedor del remito.");
                var proveedor = await _repository.GetById<Proveedor>(r.EntidadId) ?? throw new NotFoundException(nameof(Proveedor), r.EntidadId);
                anterior = (await _repository.GetById<Proveedor>(remito.ProveedorId))?.RazonSocial ?? $"#{remito.ProveedorId}";
                nuevo = proveedor.RazonSocial;
                numero = remito.NumeroRemito ?? remito.Id.ToString();
                remito.ProveedorId = r.EntidadId;
                await _repository.Update(remito);
            }
            else
            {
                var remito = await _repository.GetById<RemitoEgreso>(remitoId) ?? throw new NotFoundException(nameof(RemitoEgreso), remitoId);
                if (remito.ClienteId == r.EntidadId)
                    throw new DomainException(DomainErrorCode.CorreccionSinCambios, "Ya es el cliente del remito.");
                var cliente = await _repository.GetById<Cliente>(r.EntidadId) ?? throw new NotFoundException(nameof(Cliente), r.EntidadId);
                anterior = (await _repository.GetById<Cliente>(remito.ClienteId))?.RazonSocial ?? $"#{remito.ClienteId}";
                nuevo = cliente.RazonSocial;
                numero = remito.NumeroRemito ?? remito.Id.ToString();
                remito.ClienteId = r.EntidadId;
                await _repository.Update(remito);
            }

            if (ticket != null)
                await CorreccionSoporte.Comentar(_repository, _http, ticket,
                    $"Remito {numero}: {(esIngreso ? "proveedor" : "cliente")} {anterior} → {nuevo}.", motivo);

            return await Obtener(esIngreso, remitoId);
        }

        private async Task<(string Numero, int DepositoId)> Cabecera(bool esIngreso, int remitoId)
        {
            if (esIngreso)
            {
                var r = await _repository.GetById<RemitoIngreso>(remitoId) ?? throw new NotFoundException(nameof(RemitoIngreso), remitoId);
                return (r.NumeroRemito ?? r.Id.ToString(), r.DepositoId);
            }
            var e = await _repository.GetById<RemitoEgreso>(remitoId) ?? throw new NotFoundException(nameof(RemitoEgreso), remitoId);
            return (e.NumeroRemito ?? e.Id.ToString(), e.DepositoId);
        }

        /// <summary>El remito como lo devuelve el historial, ya corregido.</summary>
        private async Task<RemitoResponseGet> Obtener(bool esIngreso, int remitoId)
        {
            if (esIngreso)
            {
                var r = await _repository.Query<RemitoIngreso>()
                    .Include(x => x.Detalles).ThenInclude(d => d.Lote).Include(x => x.Detalles).ThenInclude(d => d.Producto)
                    .Include(x => x.Deposito).Include(x => x.Proveedor)
                    .FirstAsync(x => x.Id == remitoId);
                return new RemitoResponseGet(r.Id, r.NumeroRemito, r.ComprobanteAsociado, r.Fecha, r.DepositoId, r.Deposito.Nombre,
                    r.ProveedorId, r.Proveedor.RazonSocial, r.Observaciones, r.Detalles.Select(RemitoService.Detalle).ToList());
            }
            var e = await _repository.Query<RemitoEgreso>()
                .Include(x => x.Detalles).ThenInclude(d => d.Lote).Include(x => x.Detalles).ThenInclude(d => d.Producto)
                .Include(x => x.Deposito).Include(x => x.Cliente)
                .FirstAsync(x => x.Id == remitoId);
            return new RemitoResponseGet(e.Id, e.NumeroRemito, e.ComprobanteAsociado, e.Fecha, e.DepositoId, e.Deposito.Nombre,
                e.ClienteId, e.Cliente.RazonSocial, e.Observaciones, e.Detalles.Select(RemitoService.Detalle).ToList());
        }
    }
}

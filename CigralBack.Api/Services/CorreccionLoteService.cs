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
    /// Corrección de soporte de un lote de un producto: renombra el lote y/o cambia su vencimiento en todas
    /// sus existencias y líneas de remito. <c>Lote</c> es un código compartido entre productos, así que nunca
    /// se renombra el registro: se reasignan las existencias y líneas de ese producto a otro lote (que se busca
    /// o se crea). Si el código nuevo ya tiene una existencia en el mismo depósito y serie, se fusionan.
    /// </summary>
    public class CorreccionLoteService : ICorreccionLoteService
    {
        private readonly IRepository _repository;
        private readonly IExistenciaService _existencias;
        private readonly IHttpContextAccessor _http;

        public CorreccionLoteService(IRepository repository, IExistenciaService existencias, IHttpContextAccessor http)
        {
            _repository = repository;
            _existencias = existencias;
            _http = http;
        }

        private static string? Normalizar(string? codigo)
            => string.IsNullOrWhiteSpace(codigo) ? null : codigo.Trim().ToUpper();

        /// <summary>El producto, el lote actual y lo que tiene ese producto con ese lote; 404 si no hay nada.</summary>
        private async Task<(Producto Producto, Lote Lote, List<Existencia> Existencias, List<DetalleRemito> Lineas)> Cargar(int productoId, string codigoLote)
        {
            var producto = await _repository.GetById<Producto>(productoId)
                ?? throw new NotFoundException(nameof(Producto), productoId);
            var codigo = Normalizar(codigoLote) ?? throw new NotFoundException(nameof(Lote), codigoLote ?? "");
            var lote = await _repository.First<Lote>(l => l.CodigoLote.ToUpper() == codigo)
                ?? throw new NotFoundException(nameof(Lote), codigo);

            // Sin includes: Update() marca todo el grafo como modificado y ensuciaría la auditoría.
            var existencias = (await _repository.GetFiltered<Existencia>(
                e => e.ProductoId == productoId && e.LoteId == lote.Id, 1, int.MaxValue)).Items.OrderBy(e => e.Id).ToList();
            var lineas = (await _repository.GetFiltered<DetalleRemito>(
                d => d.ProductoId == productoId && d.LoteId == lote.Id, 1, int.MaxValue)).Items.OrderBy(d => d.Id).ToList();

            if (existencias.Count == 0 && lineas.Count == 0)
                throw new NotFoundException(nameof(Lote), codigo,
                    $"El producto {producto.Nombre} no tiene existencias ni líneas de remito con el lote {codigo}.");
            return (producto, lote, existencias, lineas);
        }

        public async Task<CorreccionLotePrevia> VistaPrevia(int productoId, string codigoLote, string? codigoNuevo)
        {
            var (producto, lote, existencias, lineas) = await Cargar(productoId, codigoLote);
            var nuevo = Normalizar(codigoNuevo);
            var cambiaCodigo = nuevo != null && nuevo != lote.CodigoLote.ToUpper();
            var loteDestino = cambiaCodigo ? await _repository.First<Lote>(l => l.CodigoLote.ToUpper() == nuevo) : null;

            // Lo que el producto ya tiene con el código nuevo (para ver qué se fusiona).
            var yaExistentes = loteDestino == null ? new List<Existencia>()
                : (await _repository.GetFiltered<Existencia>(e => e.ProductoId == productoId && e.LoteId == loteDestino.Id, 1, int.MaxValue)).Items.ToList();
            var lineasDestino = loteDestino == null ? 0
                : (await _repository.GetFiltered<DetalleRemito>(d => d.ProductoId == productoId && d.LoteId == loteDestino.Id, 1, 2)).TotalCount;

            var depositos = (await _repository.GetFiltered<Deposito>(d => true, 1, int.MaxValue)).Items.ToDictionary(d => d.Id, d => d.Nombre);
            var ids = existencias.Select(e => e.Id).ToList();
            var consignado = (await _repository.GetFiltered<Consignacion>(c => ids.Contains(c.ExistenciaId), 1, int.MaxValue)).Items
                .GroupBy(c => c.ExistenciaId).ToDictionary(g => g.Key, g => g.Sum(c => c.Cantidad));

            var filas = existencias.Select(e =>
            {
                var destino = cambiaCodigo
                    ? yaExistentes.FirstOrDefault(x => x.DepositoId == e.DepositoId && x.NumSerie == e.NumSerie)
                    : null;
                return new ExistenciaLotePrevia(e.Id, e.DepositoId, depositos.GetValueOrDefault(e.DepositoId, $"#{e.DepositoId}"),
                    e.NumSerie, e.FechaVencimiento, e.Cantidad, consignado.GetValueOrDefault(e.Id), destino?.Id);
            }).ToList();

            var remitos = await RemitosDe(lineas);
            return new CorreccionLotePrevia(productoId, producto.Nombre, lote.CodigoLote, cambiaCodigo ? nuevo! : lote.CodigoLote,
                cambiaCodigo && (yaExistentes.Count > 0 || lineasDestino > 0), filas,
                lineas.Count(l => l.RemitoIngresoId != null), lineas.Count(l => l.RemitoEgresoId != null), remitos);
        }

        /// <summary>Remitos (con número) a los que pertenecen las líneas, con cuántas líneas tiene cada uno.</summary>
        private async Task<List<RemitoLotePrevia>> RemitosDe(List<DetalleRemito> lineas)
        {
            var resultado = new List<RemitoLotePrevia>();
            var idsIngreso = lineas.Where(l => l.RemitoIngresoId != null).Select(l => l.RemitoIngresoId!.Value).Distinct().ToList();
            var idsEgreso = lineas.Where(l => l.RemitoEgresoId != null).Select(l => l.RemitoEgresoId!.Value).Distinct().ToList();
            if (idsIngreso.Count > 0)
            {
                var numeros = await _repository.Query<RemitoIngreso>().AsNoTracking().Where(r => idsIngreso.Contains(r.Id))
                    .Select(r => new { r.Id, r.NumeroRemito }).ToListAsync();
                resultado.AddRange(numeros.OrderBy(n => n.Id).Select(n => new RemitoLotePrevia("ingreso", n.Id, n.NumeroRemito ?? n.Id.ToString(),
                    lineas.Count(l => l.RemitoIngresoId == n.Id))));
            }
            if (idsEgreso.Count > 0)
            {
                var numeros = await _repository.Query<RemitoEgreso>().AsNoTracking().Where(r => idsEgreso.Contains(r.Id))
                    .Select(r => new { r.Id, r.NumeroRemito }).ToListAsync();
                resultado.AddRange(numeros.OrderBy(n => n.Id).Select(n => new RemitoLotePrevia("egreso", n.Id, n.NumeroRemito ?? n.Id.ToString(),
                    lineas.Count(l => l.RemitoEgresoId == n.Id))));
            }
            return resultado;
        }

        public async Task<CorreccionLoteResponse> Corregir(CorreccionLoteRequest r)
        {
            var motivo = CorreccionSoporte.Motivo(r.TicketId, r.Motivo);
            var (producto, lote, existencias, lineas) = await Cargar(r.ProductoId, r.CodigoLote);
            var ticket = await CorreccionSoporte.Ticket(_repository, r.TicketId);

            var codigoViejo = lote.CodigoLote.ToUpper();
            var nuevo = Normalizar(r.CodigoNuevo);
            var cambiaCodigo = nuevo != null && nuevo != codigoViejo;
            var vencNuevo = r.CambiarVencimiento ? r.FechaVencimiento?.Date : null;

            // ¿Hay algo que cambiar?
            var cambiaVenc = r.CambiarVencimiento
                && (existencias.Any(e => e.FechaVencimiento?.Date != vencNuevo) || lineas.Any(l => l.FechaVencimiento?.Date != vencNuevo));
            if (!cambiaCodigo && !cambiaVenc)
                throw new DomainException(DomainErrorCode.CorreccionSinCambios, "No hay cambios para guardar.");

            // Vencimientos anteriores, para el comentario del ticket.
            var vencAnteriores = existencias.Select(e => e.FechaVencimiento).Concat(lineas.Select(l => l.FechaVencimiento))
                .Select(f => f?.Date).Distinct().OrderBy(f => f).Select(f => CorreccionSoporte.Fecha(f)).ToList();

            using var transaction = await _repository.BeginTransaction();

            Lote destino = lote;
            if (cambiaCodigo)
                destino = await _repository.First<Lote>(l => l.CodigoLote.ToUpper() == nuevo)
                    ?? await _repository.Add(new Lote { CodigoLote = nuevo! });

            var observaciones = CorreccionSoporte.Observaciones(ticket, motivo, $"Lote {codigoViejo} → {nuevo}");
            var fusionadas = 0;

            foreach (var e in existencias)
            {
                if (cambiaCodigo)
                {
                    var (_, fusion) = await _existencias.MoverExistencia(e, destino.Id, e.NumSerie, e.DepositoId,
                        r.CambiarVencimiento ? vencNuevo : e.FechaVencimiento, e.Cantidad, observaciones,
                        conservarVencimientoDestino: !r.CambiarVencimiento);
                    if (fusion != null) fusionadas++;
                }
                else if (r.CambiarVencimiento && e.FechaVencimiento?.Date != vencNuevo)
                {
                    e.FechaVencimiento = vencNuevo;
                    await _repository.Update(e);
                }
            }

            // Las líneas de remito no mueven stock: el stock ya se movió arriba.
            foreach (var l in lineas)
            {
                var cambia = false;
                if (cambiaCodigo) { l.LoteId = destino.Id; cambia = true; }
                if (r.CambiarVencimiento && l.FechaVencimiento?.Date != vencNuevo) { l.FechaVencimiento = vencNuevo; cambia = true; }
                if (cambia) await _repository.Update(l);
            }

            var ingresos = lineas.Count(l => l.RemitoIngresoId != null);
            var egresos = lineas.Count(l => l.RemitoEgresoId != null);
            var resultante = cambiaCodigo ? nuevo! : codigoViejo;

            var partes = new List<string>
            {
                $"{existencias.Count} existencia{(existencias.Count == 1 ? "" : "s")}" + (fusionadas > 0 ? $" ({fusionadas} fusionada{(fusionadas == 1 ? "" : "s")})" : ""),
                $"{lineas.Count} línea{(lineas.Count == 1 ? "" : "s")} de remito",
            };
            var resumen = cambiaCodigo
                ? $"Lote {codigoViejo} de {producto.Nombre} corregido a {nuevo}: {string.Join(", ", partes)}"
                : $"Lote {codigoViejo} de {producto.Nombre}: {string.Join(", ", partes)}";
            if (cambiaVenc)
                resumen += $"; vencimiento {string.Join(" / ", vencAnteriores)} → {CorreccionSoporte.Fecha(vencNuevo)}";
            resumen += ".";

            if (ticket != null)
                await CorreccionSoporte.Comentar(_repository, _http, ticket, resumen, motivo);

            await transaction.CommitAsync();
            return new CorreccionLoteResponse(producto.Id, producto.Nombre, codigoViejo, resultante,
                existencias.Count, fusionadas, ingresos, egresos, cambiaVenc, resumen);
        }
    }
}

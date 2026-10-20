using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain;
using CigralBackend.Domain.Enums;
using CigralBackend.Domain.Exceptions;
using CigralBackend.Infraestructure.Database.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services
{
    /// <summary>
    /// Ajuste de stock por conteo físico: cada línea trae lo que el sistema mostraba y lo contado, y el servicio
    /// aplica la diferencia con <see cref="IExistenciaService.AjustarExistenciaExacta"/>. Todo o nada, en una transacción.
    /// </summary>
    public class AjusteStockService : IAjusteStockService
    {
        public const int MaxLineas = 300;

        private static readonly string[] Motivos = { "Inventario", "Rotura", "Vencimiento", "Perdida", "Encontrado", "Otro" };

        private readonly IRepository _repository;
        private readonly IExistenciaService _existencias;
        private readonly IHttpContextAccessor _http;

        public AjusteStockService(IRepository repository, IExistenciaService existencias, IHttpContextAccessor http)
        {
            _repository = repository;
            _existencias = existencias;
            _http = http;
        }

        public async Task<ResultadoOperacion<AjusteStockResponse>> Ajustar(AjusteStockRequest request)
        {
            var lineas = request.Lineas ?? new List<LineaAjusteRequest>();
            if (lineas.Count == 0)
                throw new DomainException(DomainErrorCode.CantidadInvalida, "No hay líneas para ajustar.");
            if (lineas.Count > MaxLineas)
                throw new DomainException(DomainErrorCode.CantidadInvalida, $"Un ajuste admite hasta {MaxLineas} líneas.");

            var categoria = Motivos.FirstOrDefault(m => string.Equals(m, request.Motivo?.Trim(), StringComparison.OrdinalIgnoreCase))
                ?? throw new DomainException(DomainErrorCode.CantidadInvalida,
                    $"El motivo tiene que ser uno de: {string.Join(", ", Motivos)}.");
            var detalle = CorreccionSoporte.Motivo(request.TicketId, request.Detalle);
            var ticket = await CorreccionSoporte.Ticket(_repository, request.TicketId);

            var referencia = $"AJ-{DateTime.Now:yyyyMMdd-HHmmss}";
            var observaciones = CorreccionSoporte.Observaciones(ticket, detalle, $"Ajuste {referencia} · {categoria}");

            using var transaction = await _repository.BeginTransaction();

            var diferencias = new List<DiferenciaAjusteResponse>();
            var claves = new HashSet<string>();

            for (int i = 0; i < lineas.Count; i++)
            {
                try
                {
                    var diferencia = await AplicarLinea(lineas[i], observaciones, claves);
                    if (diferencia != null) diferencias.Add(diferencia);
                }
                catch (Exception ex) when (ex is DomainException || ex is NotFoundException)
                {
                    await transaction.RollbackAsync();
                    return ResultadoOperacion<AjusteStockResponse>.Fallo(
                        "No se guardó ningún ajuste porque una línea tiene un problema.",
                        new List<ErrorDetalleDto> { new ErrorDetalleDto(i, ex.Message) });
                }
            }

            var sumadas = diferencias.Where(d => d.Despues > d.Antes).Sum(d => d.Despues - d.Antes);
            var restadas = diferencias.Where(d => d.Despues < d.Antes).Sum(d => d.Antes - d.Despues);
            var respuesta = new AjusteStockResponse(referencia, lineas.Count, diferencias.Count, sumadas, restadas, diferencias);

            if (ticket != null)
                await CorreccionSoporte.Comentar(_repository, _http, ticket, TextoTicket(respuesta, categoria), detalle);

            await transaction.CommitAsync();
            return ResultadoOperacion<AjusteStockResponse>.Ok(respuesta);
        }

        /// <summary>Aplica una línea y devuelve su diferencia, o null si no hubo cambio.</summary>
        private async Task<DiferenciaAjusteResponse?> AplicarLinea(LineaAjusteRequest l, string observaciones, HashSet<string> claves)
        {
            if (l.CantidadContada < 0)
                throw new DomainException(DomainErrorCode.CantidadInvalida, "La cantidad contada no puede ser negativa.");

            Existencia? existencia = null;
            int productoId, depositoId;
            int? loteId;
            string? serie, codigoLote;
            DateTime? vencimiento = null;

            if (l.ExistenciaId.HasValue)
            {
                existencia = await _repository.GetById<Existencia>(l.ExistenciaId.Value)
                    ?? throw new NotFoundException(nameof(Existencia), l.ExistenciaId.Value);
                productoId = existencia.ProductoId;
                depositoId = existencia.DepositoId;
                loteId = existencia.LoteId;
                serie = existencia.NumSerie;
                codigoLote = loteId.HasValue ? (await _repository.GetById<Lote>(loteId.Value))?.CodigoLote : null;
            }
            else
            {
                if (!l.ProductoId.HasValue || !l.DepositoId.HasValue)
                    throw new DomainException(DomainErrorCode.CantidadInvalida,
                        "La línea tiene que indicar una existencia, o el producto y el depósito.");
                productoId = l.ProductoId.Value;
                depositoId = l.DepositoId.Value;
                codigoLote = string.IsNullOrWhiteSpace(l.CodigoLote) ? null : l.CodigoLote.Trim().ToUpper();
                serie = string.IsNullOrWhiteSpace(l.NumSerie) ? null : l.NumSerie.Trim();
                vencimiento = l.FechaVencimiento?.Date;
                loteId = codigoLote == null ? null : (await _repository.First<Lote>(x => x.CodigoLote == codigoLote))?.Id;
            }

            var producto = await _repository.GetById<Producto>(productoId)
                ?? throw new NotFoundException(nameof(Producto), productoId);
            var deposito = await _repository.GetById<Deposito>(depositoId)
                ?? throw new NotFoundException(nameof(Deposito), depositoId);

            // Una misma combinación no puede estar en dos líneas: el segundo conteo pisaría o duplicaría al primero.
            if (!claves.Add($"{productoId}|{depositoId}|{codigoLote}|{serie}"))
                throw new DomainException(DomainErrorCode.CantidadInvalida, "Esta existencia ya está en otra línea del ajuste.");

            if (existencia != null)
            {
                if (existencia.Cantidad != l.CantidadEsperada)
                    throw new DomainException(DomainErrorCode.CorreccionStockYaMovido,
                        $"El stock cambió mientras contabas (el sistema ahora tiene {existencia.Cantidad}); revisá la línea.");

                var antes = existencia.Cantidad;
                var delta = l.CantidadContada - antes;
                if (delta == 0) return null;

                try
                {
                    await _existencias.AjustarExistenciaExacta(depositoId, productoId, loteId, serie, null, delta, observaciones);
                }
                catch (DomainException ex) when (ex.Code == DomainErrorCode.CorreccionStockYaMovido)
                {
                    var consignado = (await _repository.GetFiltered<Consignacion>(c => c.ExistenciaId == l.ExistenciaId!.Value, 1, int.MaxValue))
                        .Items.Sum(c => c.Cantidad);
                    throw new DomainException(DomainErrorCode.StockEnConsignacion,
                        $"Hay {consignado} en consignación; el conteo no puede ser menor.");
                }
                return new DiferenciaAjusteResponse(l.ExistenciaId, productoId, producto.Nombre, depositoId, deposito.Nombre,
                    codigoLote, serie, antes, l.CantidadContada, false);
            }

            // Línea nueva: mercadería que no estaba en el sistema. Con 0 no hay nada que cargar.
            if (serie == null && codigoLote == null && string.IsNullOrEmpty(producto.CodigoGenerico))
                throw new DomainException(DomainErrorCode.SerieYCodigoLoteNoEspecificados,
                    "Debe especificar al menos un número de serie, un código de lote o un código genérico para cargar mercadería nueva.");
            if (producto.EsUnitario && l.CantidadContada > 1)
                throw new DomainException(DomainErrorCode.ProductoUnitarioCantidadInvalida,
                    "Un producto unitario no puede tener más de 1 unidad por existencia.");
            if (l.CantidadContada == 0) return null;

            if (loteId == null && codigoLote != null)
                loteId = (await _repository.Add(new Lote { CodigoLote = codigoLote })).Id;

            var existente = await _repository.First<Existencia>(e =>
                e.ProductoId == productoId && e.DepositoId == depositoId && e.LoteId == loteId &&
                (serie == null ? e.NumSerie == null : e.NumSerie == serie));
            var anterior = existente?.Cantidad ?? 0;

            await _existencias.AjustarExistenciaExacta(depositoId, productoId, loteId, serie, vencimiento, l.CantidadContada, observaciones);
            return new DiferenciaAjusteResponse(existente?.Id, productoId, producto.Nombre, depositoId, deposito.Nombre,
                codigoLote, serie, anterior, anterior + l.CantidadContada, existente == null);
        }

        private static string TextoTicket(AjusteStockResponse r, string categoria)
        {
            var texto = $"Ajuste {r.Referencia} ({categoria}): {r.Lineas} línea(s) contada(s), {r.ConDiferencia} con diferencia (+{r.UnidadesSumadas} / −{r.UnidadesRestadas} u.).";
            foreach (var d in r.Diferencias.Take(10))
            {
                var partes = new List<string> { d.DepositoNombre };
                if (d.CodigoLote != null) partes.Add($"lote {d.CodigoLote}");
                if (d.NumSerie != null) partes.Add($"serie {d.NumSerie}");
                texto += $"\n- {d.ProductoNombre} ({string.Join(" · ", partes)}): {d.Antes} → {d.Despues}" + (d.EsNueva ? " (nueva)" : "");
            }
            if (r.Diferencias.Count > 10) texto += $"\n… y {r.Diferencias.Count - 10} más.";
            return texto;
        }
    }
}

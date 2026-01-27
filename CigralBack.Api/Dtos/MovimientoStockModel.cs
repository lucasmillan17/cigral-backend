using CigralBackend.Domain;
using System;
using System.ComponentModel.DataAnnotations;

namespace CigralBackend.Application.Dtos
{
    /// <summary>
    /// Filtros para consultar movimientos de stock.
    /// </summary>
    public record MovimientoStockFilters
    (
        int? ProductoId,
        int? DepositoId,
        int? LoteId,
        TipoMovimiento? Tipo,
        int? RemitoIngresoId,
        int? RemitoEgresoId,
        DateTime? FechaDesde,
        DateTime? FechaHasta,
        [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor a 0")]
        int PageNumber = 1,
        [Range(1, 100, ErrorMessage = "El tamaño de página no puede superar los 100 items")]
        int PageSize = 10
    );

    /// <summary>
    /// Respuesta con información del movimiento de stock.
    /// </summary>
    public record MovimientoStockResponse
    (
        int Id,
        string Tipo,
        DateTime FechaMovimiento,
        int ProductoId,
        string ProductoNombre,
        int DepositoId,
        string DepositoNombre,
        int? LoteId,
        string? CodigoLote,
        string? NumeroSerie,
        int Cantidad,
        int StockAnterior,
        int StockNuevo,
        int? RemitoIngresoId,
        int? RemitoEgresoId,
        string? Usuario,
        string? Observaciones
    );
}

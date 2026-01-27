using System;
using System.Collections.Generic;

namespace CigralBackend.Application.Dtos
{
    /// <summary>
    /// DTO ligero para items próximos a vencer (dashboard).
    /// </summary>
    public record ProductoProximoVencerDto
    (
        int ExistenciaId,
        int ProductoId,
        string ProductoNombre,
        string ProductoGtin,
        int DepositoId,
        string DepositoNombre,
        int? LoteId,
        string? CodigoLote,
        string? NumeroSerie,
        DateTime FechaVencimiento,
        int DiasParaVencer,
        int Cantidad
    );

    /// <summary>
    /// Estadísticas de vencimientos agrupadas por rango.
    /// </summary>
    public record VencimientoStats
    (
        string Rango,           // "0-30 días", "31-90 días", etc.
        int DiasMinimo,
        int DiasMaximo,
        int TotalProductos,     // Cantidad de productos diferentes
        int TotalLotes,         // Cantidad de lotes
        int CantidadTotal,      // Suma de todas las cantidades
        List<ProductoProximoVencerDto> Items
    );

    /// <summary>
    /// Dashboard completo de vencimientos.
    /// </summary>
    public record DashboardVencimientosResponse
    (
        DateTime FechaConsulta,
        int TotalProductosProximosVencer,
        int TotalLotesProximosVencer,
        int CantidadTotalProximaVencer,
        List<VencimientoStats> Rangos
    );

    /// <summary>
    /// Filtros para consultar productos próximos a vencer.
    /// </summary>
    public record VencimientoFilters
    (
        int? DiasDesde,         // Ej: 0 (hoy)
        int? DiasHasta,         // Ej: 90 (3 meses)
        int? DepositoId,
        int? ProductoId,
        bool IncluirVencidos = false
    );
}

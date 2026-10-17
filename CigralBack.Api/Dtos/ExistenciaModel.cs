using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace CigralBackend.Application.Dtos
{
    public record ExistenciaModelRequest
    (
        [Required(ErrorMessage = "El depósito es obligatorio")]
        int DepositoId,
        [Required(ErrorMessage = "El producto es obligatorio")]
        int ProductoId,
        [MaxLength(100, ErrorMessage = "El número de serie no puede superar los 100 caracteres")]
        string? NumSerie,
        string? CodigoLote,
        DateTime? FechaVencimiento, // Opcional: hay productos que no vencen
        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor o igual a 0")]
        int Cantidad,
        string ? InformacionAdicional, // Para guardar cualquier otro dato que queramos en formato JSON (ej: campos personalizados del parser, etc.)
        bool EsDevolucion = false // Para marcar esta existencia como una devolución (si es true, se puede mostrar en rojo en la grilla, etc.)
    );

    public record ExistenciaModelResponse(
        int Id, // ID de la existencia (para editar/borrar)

        // --- Datos del Producto (Flattened / Aplanados) ---
        int ProductoId,        // Útil para navegar al detalle del producto
        string ProductoNombre, // ¡INDISPENSABLE! Para mostrar en la grilla
        string ProductoGtin,   // Muy útil para que el usuario verifique visualmente
        string? ProductoCodigoInterno, // Muy útil para que el usuario verifique visualmente

        // --- Datos del Depósito ---
        int DepositoId,
        string DepositoNombre, // Para saber DÓNDE está (ej: "Depósito Central")

        // --- Datos del Lote ---
        int? LoteId,
        string? CodigoLote,    // El humano quiere ver "LOTE-A24", no el ID 58

        // --- Datos de Existencia ---
        string? NumSerie,
        DateTime? FechaVencimiento,
        int Cantidad,
        string? InformacionAdicional // Para mostrar cualquier otro dato que hayamos guardado en formato JSON (ej: campos personalizados del parser, etc.)
    );

    public enum OrdenExistencia
    {
        Id,
        NombreProducto,
        FechaVencimiento,
        Cantidad
    }

    public record ExistenciaFilters
    (
        int? DepositoId,
        int? ProductoId,
        int? LoteId,
        string? CodigoLote,
        string? NumSerie,
        string? CodigoGenerico,

        // Filtros de vencimiento
        DateTime? FechaVencimientoDesde,
        DateTime? FechaVencimientoHasta,
        int? DiasParaVencer,  // Ej: 90 = productos que vencen en los próximos 90 días
        string? NombreProducto, // Búsqueda por nombre (puede ser parte del nombre)
        bool? SoloConVencimiento = null, // true = solo con fecha de vencimiento, false = solo sin vencimiento

        OrdenExistencia? OrdenarPor = OrdenExistencia.Id, // Por defecto ordena por Id
        bool EsDescendente = false, // Por defecto ascendente


        [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor a 0")]
        int PageNumber = 1,
        [Range(1, 100, ErrorMessage = "El tamaño de página no puede superar los 100 items")]
        int PageSize = 10
    );

    public record StockDisponibleResponse(int Cantidad);

    /// <summary>
    /// Existencias de un mismo producto, lote, serie y vencimiento sumadas entre depósitos,
    /// con el detalle de cuánto hay en cada uno.
    /// </summary>
    public record ExistenciaAgrupadaResponse(
        int ProductoId,
        string ProductoNombre,
        string ProductoGtin,
        int LoteId,
        string CodigoLote,
        string NumSerie,
        DateTime? FechaVencimiento,
        int CantidadTotal,
        List<ExistenciaPorDepositoResponse> Depositos
    );

    public record ExistenciaPorDepositoResponse(int ExistenciaId, int DepositoId, string DepositoNombre, int Cantidad);

    /// <summary>
    /// Corrección de una existencia hecha por soporte: estado final completo deseado.
    /// Debe indicar un ticket o un motivo.
    /// </summary>
    public record ExistenciaCorreccionRequest
    (
        [MaxLength(100, ErrorMessage = "El código de lote no puede superar los 100 caracteres")]
        string? CodigoLote,
        [MaxLength(100, ErrorMessage = "El número de serie no puede superar los 100 caracteres")]
        string? NumSerie,
        DateTime? FechaVencimiento,
        [Range(1, int.MaxValue, ErrorMessage = "El depósito es obligatorio")]
        int DepositoId,
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor o igual a 0")]
        int Cantidad,
        int? TicketId,
        [MaxLength(500, ErrorMessage = "El motivo no puede superar los 500 caracteres")]
        string? Motivo
    );
}

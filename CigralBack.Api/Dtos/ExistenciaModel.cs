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
        DateTime FechaVencimiento,
        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor o igual a 0")]
        int Cantidad
    );

    public record ExistenciaModelResponse(
        int Id, // ID de la existencia (para editar/borrar)

        // --- Datos del Producto (Flattened / Aplanados) ---
        int ProductoId,        // Útil para navegar al detalle del producto
        string ProductoNombre, // ¡INDISPENSABLE! Para mostrar en la grilla
        string ProductoGtin,   // Muy útil para que el usuario verifique visualmente

        // --- Datos del Depósito ---
        int DepositoId,
        string DepositoNombre, // Para saber DÓNDE está (ej: "Depósito Central")

        // --- Datos del Lote ---
        int? LoteId,
        string? CodigoLote,    // El humano quiere ver "LOTE-A24", no el ID 58

        // --- Datos de Existencia ---
        string? NumSerie,
        DateTime? FechaVencimiento,
        int Cantidad
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
}

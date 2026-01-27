using System.ComponentModel.DataAnnotations;

namespace CigralBackend.Application.Dtos
{
    /// <summary>
    /// Request para crear o actualizar un depósito.
    /// </summary>
    public record DepositoModelRequest
    (
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        string Nombre,

        [Required(ErrorMessage = "El código es obligatorio")]
        [MaxLength(20, ErrorMessage = "El código no puede superar los 20 caracteres")]
        string Codigo,

        bool Activo = true
    );

    /// <summary>
    /// Respuesta con información del depósito.
    /// </summary>
    public record DepositoModelResponse
    (
        int Id,
        string Nombre,
        string Codigo,
        bool Activo
    );

    /// <summary>
    /// Filtros para buscar depósitos.
    /// </summary>
    public record DepositoFilters
    (
        string? Nombre,
        string? Codigo,
        bool? Activo,
        [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor a 0")]
        int PageNumber = 1,
        [Range(1, 100, ErrorMessage = "El tamaño de página no puede superar los 100 items")]
        int PageSize = 10
    );
}

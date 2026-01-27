using System.ComponentModel.DataAnnotations;

namespace CigralBackend.Application.Dtos
{
    /// <summary>
    /// Request para crear o actualizar un proveedor.
    /// </summary>
    public record ProveedorModelRequest
    (
        [Required(ErrorMessage = "La razón social es obligatoria")]
        [MaxLength(200, ErrorMessage = "La razón social no puede superar los 200 caracteres")]
        string RazonSocial,

        [MaxLength(13, ErrorMessage = "El GLN debe tener 13 caracteres")]
        [MinLength(13, ErrorMessage = "El GLN debe tener 13 caracteres")]
        string? GLN,

        [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
        [MaxLength(100, ErrorMessage = "El email no puede superar los 100 caracteres")]
        string? Email,

        [MaxLength(11, ErrorMessage = "El CUIT no puede superar los 11 caracteres")]
        string? Cuit,

        [MaxLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres")]
        string? Telefono,

        [MaxLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres")]
        string? Direccion
    );

    /// <summary>
    /// Respuesta con información del proveedor.
    /// </summary>
    public record ProveedorModelResponse
    (
        int Id,
        string? RazonSocial,
        string? GLN,
        string? Email,
        string? Cuit,
        string? Telefono,
        string? Direccion
    );

    /// <summary>
    /// Filtros para buscar proveedores.
    /// </summary>
    public record ProveedorFilters
    (
        string? RazonSocial,
        string? GLN,
        string? Cuit,
        [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor a 0")]
        int PageNumber = 1,
        [Range(1, 100, ErrorMessage = "El tamaño de página no puede superar los 100 items")]
        int PageSize = 10
    );
}

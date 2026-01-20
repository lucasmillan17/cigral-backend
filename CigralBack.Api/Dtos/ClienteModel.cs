using System.ComponentModel.DataAnnotations;

namespace CigralBackend.Application.Dtos
{
    public record ClienteModelRequest
    (
        [Required(ErrorMessage = "La razón social es obligatoria")]
        [MaxLength(200, ErrorMessage = "La razón social no puede superar los 200 caracteres")]
        string? RazonSocial,
        [Required(ErrorMessage = "El GLN es obligatorio")]
        [MaxLength(13, ErrorMessage = "El GLN debe tener 13 caracteres")]
        string GLN,
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
}

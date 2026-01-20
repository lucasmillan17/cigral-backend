using System.ComponentModel.DataAnnotations;

namespace CigralBackend.Application.Dtos
{
    public record DepositoModelRequest
    (
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        string Nombre,
        [Required(ErrorMessage = "El código es obligatorio")]
        [MaxLength(20, ErrorMessage = "El código no puede superar los 20 caracteres")]
        string Codigo,
        [Required(ErrorMessage = "El estado activo es obligatorio")]
        bool Activo
    );
}

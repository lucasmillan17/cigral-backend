using System.ComponentModel.DataAnnotations;

namespace CigralBackend.Application.Dtos
{
    public record RemitoClienteModelRequest
    (
        [Required(ErrorMessage = "La fecha es obligatoria")]
        DateTime Fecha,
        [Required(ErrorMessage = "Los detalles son obligatorios")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un detalle")]
        List<DetalleRemitoModelRequest>? Detalles,
        [MaxLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres")]
        string? Observaciones,
        [MaxLength(50, ErrorMessage = "El número de remito no puede superar los 50 caracteres")]
        string? NumeroRemito,
        [Required(ErrorMessage = "El cliente es obligatorio")]
        int ClienteId
    );
}

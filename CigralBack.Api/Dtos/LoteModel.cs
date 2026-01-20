using System.ComponentModel.DataAnnotations;

namespace CigralBackend.Application.Dtos
{
    public record LoteModelRequest
    (
        [Required(ErrorMessage = "El código de lote es obligatorio")]
        [MaxLength(50, ErrorMessage = "El código de lote no puede superar los 50 caracteres")]
        string CodigoLote,
        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
        DateTime FechaVencimiento,
        [Required(ErrorMessage = "La cantidad disponible es obligatoria")]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad disponible debe ser mayor o igual a 0")]
        int CantidadDisponible,
        [Required(ErrorMessage = "El producto es obligatorio")]
        int ProductoId
    );
}

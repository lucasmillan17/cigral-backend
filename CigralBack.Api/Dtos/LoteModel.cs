using System.ComponentModel.DataAnnotations;

namespace CigralBackend.Application.Dtos
{
    public record LoteModelRequest
    (
        [Required(ErrorMessage = "El código de lote es obligatorio")]
        [MaxLength(50, ErrorMessage = "El código de lote no puede superar los 50 caracteres")]
        string CodigoLote,
        DateTime? FechaVencimiento,
        [Required(ErrorMessage = "El producto es obligatorio")]
        int ProductoId
    );
}

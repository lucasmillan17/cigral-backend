using System.ComponentModel.DataAnnotations;

namespace CigralBackend.Application.Dtos
{
    public record DetalleRemitoModelRequest
    (
        [Required(ErrorMessage = "El producto es obligatorio")]
        Guid ProductoId,
        [Required(ErrorMessage = "El lote es obligatorio")]
        Guid LoteId,
        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        int Cantidad
    );
}

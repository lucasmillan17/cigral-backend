using System.ComponentModel.DataAnnotations;

namespace CigralBackend.Application.Dtos
{
    public record DetalleRemitoModelRequest
    (
        [Required(ErrorMessage = "El producto es obligatorio")]
        int ProductoId,
        string? NumSerie,
        string? CodigoLote,
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        int? Cantidad
    );
}

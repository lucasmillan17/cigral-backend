using System;
using System.ComponentModel.DataAnnotations;

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
        int? LoteId,
        DateTime? FechaVencimiento,
        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor o igual a 0")]
        int Cantidad
    );
}

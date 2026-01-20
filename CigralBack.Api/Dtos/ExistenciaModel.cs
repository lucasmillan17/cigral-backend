using System;
using System.ComponentModel.DataAnnotations;

namespace CigralBackend.Application.Dtos
{
    public record ExistenciaModelRequest
    (
        [Required(ErrorMessage = "El depósito es obligatorio")]
        Guid DepositoId,
        [Required(ErrorMessage = "El producto es obligatorio")]
        Guid ProductoId,
        [MaxLength(100, ErrorMessage = "El número de serie no puede superar los 100 caracteres")]
        string? NumSerie,
        Guid? LoteId,
        DateTime? FechaVencimiento,
        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor o igual a 0")]
        int Cantidad
    );
}

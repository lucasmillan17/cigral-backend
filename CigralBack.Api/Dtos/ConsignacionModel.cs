using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Application.Dtos
{
    public record ConsignacionRequest
    (
        [Required(ErrorMessage = "El Id de existencia es obligatorio")]
        int ExistenciaId,
        [Required(ErrorMessage = "El Id de cliente es obligatorio")]
        int ClienteId,
        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        int Cantidad
    );
    public record ConsignacionResponse
    (
        int Id,
        int ExistenciaId,
        string Cliente,
        int Cantidad,
        DateTime FechaModificacion
    );

    public record ConsignacionFilters(
        string? ClienteNombre = null,
        string? ProductoNombre = null,
        string? CodigoLote = null,
        string? NumSerie = null,
        int? DepositoId = null,
        int PageNumber = 1,
        int PageSize = 10,
        bool EsDescendente = true
    );

    public record GetConsignacionResponse(
         int Id,
         int ExistenciaId,
         string ProductoNombre,
         string CodigoLote,
         string NumSerie,
         string DepositoNombre, // Lo mantenemos para que la tabla muestre dónde está la mercadería
         int ClienteId,
         string ClienteRazonSocial,
         int Cantidad,
         DateTime FechaModificacion
     );

}

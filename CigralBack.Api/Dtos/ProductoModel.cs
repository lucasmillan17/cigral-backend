using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Application.Dtos
{
    public record ProductoModelRequest
    (
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        string Nombre,
        [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres")]
        string Descripcion,
        [Required(ErrorMessage = "El GTIN es obligatorio")]
        [MaxLength(14, ErrorMessage = "El GTIN no puede superar los 14 caracteres")]
        string GTIN,
        [Required(ErrorMessage = "El campo EsUnitario es obligatorio")]
        bool EsUnitario,
        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser un valor positivo")]
        decimal? Precio
    );

    public record ProductoModelResponse
    (
        int Id,
        string Nombre,
        string Descripcion,
        string GTIN,
        bool EsUnitario,
        decimal? Precio
    );
}

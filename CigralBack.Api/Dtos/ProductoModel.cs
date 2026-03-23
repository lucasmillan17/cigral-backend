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
        [MaxLength(14, ErrorMessage = "El GTIN no puede superar los 14 caracteres")]
        string? GTIN,
        string? CodigoGenerico,
        string? CodigoInterno,
        [Required(ErrorMessage = "El campo EsUnitario es obligatorio")]
        bool? EsUnitario,
        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser un valor positivo")]
        decimal? Precio,
        string? Marca
    );

    public record ProductoModelResponse
    (
        int Id,
        string? Marca,
        string Nombre,
        string Descripcion,
        string? GTIN,
        string? CodigoGenerico,
        string? CodigoInterno,
        decimal? Precio
    );

    public record ProductoFilters
    (
        string? Nombre,
        string? Gtin,
        string? CodigoGenerico,
        string? CodigoInterno,
        string? Marca,
        [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor a 0")]
        int PageNumber = 1,
        [Range(1, 100, ErrorMessage = "El tamaño de página no puede superar los 100 items")]
        int PageSize = 10
    );
}

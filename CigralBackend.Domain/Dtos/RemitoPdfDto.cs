using System;
using System.Collections.Generic;

namespace CigralBackend.Domain.Dtos
{
    /// <summary>
    /// DTO con información completa de remito para generar PDF.
    /// </summary>
    public record RemitoPdfDto
    {
        // Información del Remito
        public string NumeroRemito { get; init; } = string.Empty;
        public DateTime Fecha { get; init; }
        public string? Observaciones { get; init; }
        public string TipoRemito { get; init; } = string.Empty; // "Ingreso" o "Egreso"

        // Información del Cliente/Proveedor
        public string? RazonSocial { get; init; }
        public string? CUIT { get; init; }
        public string? Direccion { get; init; }
        public string? Telefono { get; init; }
        public string? Email { get; init; }

        // Detalles del Remito
        public List<DetalleRemitoPdfDto> Detalles { get; init; } = new();

        // Totales
        public int CantidadTotal { get; init; }
        public int CantidadItems { get; init; }
    }

    /// <summary>
    /// DTO para detalle de remito en PDF.
    /// </summary>
    public record DetalleRemitoPdfDto
    {
        public string ProductoNombre { get; init; } = string.Empty;
        public string ProductoGtin { get; init; } = string.Empty;
        public string? CodigoLote { get; init; }
        public DateTime? FechaVencimiento { get; init; }
        public string? NumeroSerie { get; init; }
        public int Cantidad { get; init; }
        public string DepositoNombre { get; init; } = string.Empty;
    }
}

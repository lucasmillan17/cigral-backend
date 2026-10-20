using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CigralBackend.Application.Dtos
{
    /// <summary>
    /// Request para registrar un remito (ingreso o egreso).
    /// </summary>
    public record RemitoRequest
    (
        [Required(ErrorMessage = "El depósito es obligatorio")]
        int DepositoId,

        [Required(ErrorMessage = "La entidad (proveedor o cliente) es obligatoria")]
        int EntidadId, // ProveedorId para ingreso, ClienteId para egreso

        string? NumeroRemito,

        string? ComprobanteAsociado,

        string? Observaciones,

        [Required(ErrorMessage = "Los detalles son obligatorios")]
        [MinLength(1, ErrorMessage = "Debe haber al menos un detalle")]
        List<RemitoDetalleRequest> Detalles,
        bool EsDevolucion = false
    );

    /// <summary>
    /// Request para actualizar un remito (solo datos que no afectan stock).
    /// </summary>
    public record UpdateRemitoRequest
    (
        string? NumeroRemito,

        string? ComprobanteAsociado,

        string? Observaciones
    );

    /// <summary>
    /// Detalle de un item en el remito.
    /// </summary>
    public record RemitoDetalleRequest
    (
        [Required(ErrorMessage = "El producto es obligatorio")]
        int ProductoId,

        string? CodigoLote,

        [MaxLength(100, ErrorMessage = "El número de serie no puede superar los 100 caracteres")]
        string? NumeroSerie,

        DateTime? FechaVencimiento, // Opcional: hay productos que no vencen

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        int Cantidad,

        string? InformacionAdicional // Para guardar cualquier otro dato que queramos en formato JSON (ej: campos personalizados del parser, etc.)
    );

    public record RemitoDetalleResponse
    (
        int ProductoId,
        string? CodigoLote,
        string? NumeroSerie,
        DateTime? FechaVencimiento,
        int Cantidad,
        int Id = 0,
        string? ProductoNombre = null
    );

    /// <summary>
    /// Corrección de soporte de una línea de remito: estado final completo. Cantidad 0 quita la línea.
    /// Con AjustarStock (por defecto) el stock acompaña el cambio; sin él, cambia solo el documento.
    /// </summary>
    public record CorreccionLineaRequest
    {
        [MaxLength(100, ErrorMessage = "El código de lote no puede superar los 100 caracteres")]
        public string? CodigoLote { get; init; }

        [MaxLength(100, ErrorMessage = "El número de serie no puede superar los 100 caracteres")]
        public string? NumeroSerie { get; init; }

        public DateTime? FechaVencimiento { get; init; }

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser 0 o más")]
        public int Cantidad { get; init; }

        public bool AjustarStock { get; init; } = true;

        public int? TicketId { get; init; }

        [MaxLength(500, ErrorMessage = "El motivo no puede superar los 500 caracteres")]
        public string? Motivo { get; init; }
    }

    /// <summary>Línea nueva agregada por soporte a un remito ya emitido.</summary>
    public record AgregarLineaRequest : CorreccionLineaRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "El producto es obligatorio")]
        public int ProductoId { get; init; }
    }

    /// <summary>Cambio de cliente (egreso) o proveedor (ingreso) de un remito. No mueve stock.</summary>
    public record CorreccionEntidadRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Elegí el cliente o proveedor")]
        public int EntidadId { get; init; }

        public int? TicketId { get; init; }

        [MaxLength(500, ErrorMessage = "El motivo no puede superar los 500 caracteres")]
        public string? Motivo { get; init; }
    }

    /// <summary>
    /// Respuesta con información del remito creado.
    /// </summary>
    public record RemitoResponse
    (
        int Id,
        string? NumeroRemito,
        string? ComprobanteAsociado,
        DateTime Fecha,
        int DepositoId,
        int EntidadId,
        string? Observaciones,
        int CantidadDetalles,
        int CantidadTotal
    );

    public record RemitoResponseGet
    (
        int Id,
        string? NumeroRemito,
        string? ComprobanteAsociado,
        DateTime Fecha,
        int DepositoId,
        string NombreDeposito,
        int EntidadId,
        string NombreEntidad,
        string? Observaciones,
        List<RemitoDetalleResponse> Detalles
    );

    public enum OrdenRemito
    {
        Id,
        Fecha
    }

    public record RemitoFilters(
        int? DepositoId,
        int? EntidadId,
        DateTime? FechaDesde,
        DateTime? FechaHasta,
        string? NumeroRemito,

        OrdenRemito? OrdenarPor = OrdenRemito.Id,
        bool EsDescendente = true,

        int PageNumber = 1,
        int PageSize = 20
    );

    public record SiguienteRemitoResponse(
        string? SiguienteNumeroRemito
    );

    public record UltimoRemitoRequest(
        int DepositoId,
        bool EsIngreso
    );

    public record ErrorDetalleDto(
        int Orden,
        string Mensaje
    );
}

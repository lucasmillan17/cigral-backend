using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CigralBackend.Application.Dtos
{
    public record LoteModelRequest
    (
        [Required(ErrorMessage = "El código de lote es obligatorio")]
        [MaxLength(50, ErrorMessage = "El código de lote no puede superar los 50 caracteres")]
        string CodigoLote
    );

    /// <summary>
    /// Corrección de soporte de un lote de un producto: renombrarlo (si el código nuevo ya existe para el producto,
    /// se fusionan) y/o cambiar su vencimiento, en todas sus existencias y líneas de remito.
    /// Debe indicar un ticket o un motivo.
    /// </summary>
    public record CorreccionLoteRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "El producto es obligatorio")]
        public int ProductoId { get; init; }

        [Required(ErrorMessage = "El código de lote actual es obligatorio")]
        [MaxLength(100, ErrorMessage = "El código de lote no puede superar los 100 caracteres")]
        public string CodigoLote { get; init; } = "";

        /// <summary>Código nuevo; null o igual al actual = el código no cambia.</summary>
        [MaxLength(100, ErrorMessage = "El código de lote no puede superar los 100 caracteres")]
        public string? CodigoNuevo { get; init; }

        public bool CambiarVencimiento { get; init; }

        /// <summary>Vencimiento nuevo (si CambiarVencimiento); null = sin vencimiento.</summary>
        public DateTime? FechaVencimiento { get; init; }

        public int? TicketId { get; init; }

        [MaxLength(500, ErrorMessage = "El motivo no puede superar los 500 caracteres")]
        public string? Motivo { get; init; }
    }

    /// <summary>Una existencia que cambia con la corrección; FusionaConExistenciaId si se va a sumar a otra.</summary>
    public record ExistenciaLotePrevia(
        int Id, int DepositoId, string Deposito, string? NumSerie, DateTime? FechaVencimiento,
        int Cantidad, int Consignado, int? FusionaConExistenciaId);

    /// <summary>Un remito con líneas del lote.</summary>
    public record RemitoLotePrevia(string Tipo, int RemitoId, string Numero, int Lineas);

    /// <summary>Qué va a pasar con una corrección de lote, sin cambiar nada.</summary>
    public record CorreccionLotePrevia(
        int ProductoId, string Producto, string CodigoLote, string CodigoResultante,
        bool FusionaLotes,
        List<ExistenciaLotePrevia> Existencias, int LineasIngreso, int LineasEgreso, List<RemitoLotePrevia> Remitos);

    /// <summary>Resultado de una corrección de lote.</summary>
    public record CorreccionLoteResponse(
        int ProductoId, string Producto, string CodigoLote, string CodigoResultante,
        int ExistenciasAfectadas, int ExistenciasFusionadas, int LineasIngreso, int LineasEgreso,
        bool VencimientoCambiado, string Resumen);
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CigralBackend.Application.Dtos
{
    /// <summary>
    /// Ajuste de stock por conteo físico: se manda lo que hay realmente y el sistema calcula las diferencias.
    /// Hace falta un ticket o un detalle.
    /// </summary>
    public class AjusteStockRequest
    {
        /// <summary>Categoría: Inventario, Rotura, Vencimiento, Perdida, Encontrado u Otro.</summary>
        [Required(ErrorMessage = "El motivo es obligatorio")]
        public string Motivo { get; init; } = "";

        [MaxLength(500, ErrorMessage = "El detalle no puede superar los 500 caracteres")]
        public string? Detalle { get; init; }

        public int? TicketId { get; init; }

        /// <summary>De 1 a 300 líneas.</summary>
        public List<LineaAjusteRequest> Lineas { get; init; } = new();
    }

    /// <summary>
    /// Una línea del conteo. Con <c>ExistenciaId</c> se cuenta una existencia que ya existe;
    /// sin él se carga mercadería que no estaba en el sistema (producto, depósito y lote/serie).
    /// </summary>
    public class LineaAjusteRequest
    {
        public int? ExistenciaId { get; init; }

        public int? ProductoId { get; init; }
        public int? DepositoId { get; init; }

        [MaxLength(100, ErrorMessage = "El código de lote no puede superar los 100 caracteres")]
        public string? CodigoLote { get; init; }

        [MaxLength(100, ErrorMessage = "El número de serie no puede superar los 100 caracteres")]
        public string? NumSerie { get; init; }

        public DateTime? FechaVencimiento { get; init; }

        /// <summary>Lo que el sistema mostraba al contar (en una línea nueva, 0). Si cambió, la línea falla.</summary>
        public int CantidadEsperada { get; init; }

        /// <summary>Lo que se contó realmente (0 o más).</summary>
        public int CantidadContada { get; init; }
    }

    public record DiferenciaAjusteResponse(
        int? ExistenciaId,
        int ProductoId,
        string ProductoNombre,
        int DepositoId,
        string DepositoNombre,
        string? CodigoLote,
        string? NumSerie,
        int Antes,
        int Despues,
        bool EsNueva
    );

    public record AjusteStockResponse(
        string Referencia,
        int Lineas,
        int ConDiferencia,
        int UnidadesSumadas,
        int UnidadesRestadas,
        List<DiferenciaAjusteResponse> Diferencias
    );
}

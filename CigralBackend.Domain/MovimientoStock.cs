using CigralBackend.Domain.Bases;
using System;

namespace CigralBackend.Domain
{
    /// <summary>
    /// Registro de auditoría para todos los movimientos de stock.
    /// </summary>
    public class MovimientoStock : EntityBase
    {
        public MovimientoStock() { }

        /// <summary>
        /// Tipo de movimiento (Ingreso, Egreso, Ajuste)
        /// </summary>
        public TipoMovimiento Tipo { get; set; }

        /// <summary>
        /// Fecha y hora del movimiento
        /// </summary>
        public DateTime FechaMovimiento { get; set; }

        /// <summary>
        /// Producto relacionado
        /// </summary>
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }

        /// <summary>
        /// Depósito donde se realizó el movimiento
        /// </summary>
        public int DepositoId { get; set; }
        public Deposito Deposito { get; set; }

        /// <summary>
        /// Lote si aplica
        /// </summary>
        public int? LoteId { get; set; }
        public Lote? Lote { get; set; }

        /// <summary>
        /// Número de serie si aplica
        /// </summary>
        public string? NumeroSerie { get; set; }

        /// <summary>
        /// Cantidad del movimiento (positivo para ingreso, negativo para egreso)
        /// </summary>
        public int Cantidad { get; set; }

        /// <summary>
        /// Stock anterior antes del movimiento
        /// </summary>
        public int StockAnterior { get; set; }

        /// <summary>
        /// Stock nuevo después del movimiento
        /// </summary>
        public int StockNuevo { get; set; }

        /// <summary>
        /// Remito de ingreso asociado (si aplica)
        /// </summary>
        public int? RemitoIngresoId { get; set; }
        public RemitoIngreso? RemitoIngreso { get; set; }

        /// <summary>
        /// Remito de egreso asociado (si aplica)
        /// </summary>
        public int? RemitoEgresoId { get; set; }
        public RemitoEgreso? RemitoEgreso { get; set; }

        /// <summary>
        /// Usuario que realizó el movimiento (si aplica)
        /// </summary>
        public string? Usuario { get; set; }

        /// <summary>
        /// Observaciones del movimiento
        /// </summary>
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// Tipos de movimientos de stock
    /// </summary>
    public enum TipoMovimiento
    {
        /// <summary>
        /// Entrada de mercadería (remito de proveedor)
        /// </summary>
        Ingreso = 1,

        /// <summary>
        /// Salida de mercadería (remito de cliente)
        /// </summary>
        Egreso = 2,

        /// <summary>
        /// Ajuste manual de stock
        /// </summary>
        AjustePositivo = 3,

        /// <summary>
        /// Ajuste manual de stock (disminución)
        /// </summary>
        AjusteNegativo = 4,

        /// <summary>
        /// Transferencia entre depósitos
        /// </summary>
        Transferencia = 5,
    }
}

using CigralBackend.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Domain
{
    /// <summary>
    /// Código de lote, compartido por todas las existencias, detalles y movimientos que lo usan.
    /// No pertenece a un producto ni tiene vencimiento: el mismo código puede venir del fabricante
    /// en productos distintos, y el vencimiento se guarda en cada existencia y detalle de remito.
    /// </summary>
    public class Lote : EntityBase
    {
        public Lote() { }

        public string CodigoLote { get; set; }
    }
}

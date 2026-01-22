using CigralBackend.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Domain
{
    public class Existencia : EntityBase
    {
        public Existencia() { }
        public Deposito Deposito { get; set; }
        public int DepositoId { get; set; }
        public Producto Producto { get; set; }
        public int ProductoId { get; set; }
        public string? NumSerie { get; set; }
        public Lote? Lote { get; set; }
        public int? LoteId { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public int Cantidad { get; set; }
    }
}

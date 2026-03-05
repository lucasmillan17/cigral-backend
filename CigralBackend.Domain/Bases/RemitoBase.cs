using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Domain.Bases
{
    public abstract class RemitoBase : EntityBase
    {
        public DateTime Fecha { get; set; }
        public List<DetalleRemito> Detalles { get; set; }
        public string? Observaciones { get; set; }
        public string? NumeroRemito { get; set; }
        public int DepositoId { get; set; }
        public Deposito Deposito { get; set; }
    }
}

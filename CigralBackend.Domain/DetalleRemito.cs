using CigralBackend.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Domain
{
    public class DetalleRemito : EntityBase
    {
        public DetalleRemito() { }
        public Producto Producto { get; set; }
        public Lote Lote { get; set; }
        public int Cantidad { get; set; }

    }
}

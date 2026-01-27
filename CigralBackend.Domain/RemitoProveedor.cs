using CigralBackend.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Domain
{
    public class RemitoIngreso : RemitoBase
    {
        public RemitoIngreso() 
        {
            Detalles = new List<DetalleRemito>();
        }
        
        public int ProveedorId { get; set; }
        public Proveedor Proveedor { get; set; }
        
        public List<DetalleRemito> Detalles { get; set; }
    }
}

using CigralBackend.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Domain
{
    public class RemitoEgreso : RemitoBase
    {
        public RemitoEgreso() 
        {
            Detalles = new List<DetalleRemito>();
        }
        
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }
        
        public List<DetalleRemito> Detalles { get; set; }
    }
}

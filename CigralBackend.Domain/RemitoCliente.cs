using CigralBackend.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Domain
{
    public class RemitoCliente : RemitoBase
    {
        public RemitoCliente() { }
        public Cliente Cliente { get; set; }
        public int ClienteId { get; set; }
    }
}

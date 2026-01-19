using CigralBackend.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Domain
{
    public class RemitoProveedor : RemitoBase
    {
        public RemitoProveedor() { }
        public Proveedor Proveedor { get; set; }
        public Guid ProveedorId { get; set; }
    }
}

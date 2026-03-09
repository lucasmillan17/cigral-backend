using CigralBackend.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Domain
{
    public class Deposito : EntityBase
    {
        public Deposito() { }
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public bool Activo { get; set; } = true;
    }
}

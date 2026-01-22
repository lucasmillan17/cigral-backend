using CigralBackend.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Domain
{
    public class Marca : EntityBase
    {
        public Marca() { }
        public Marca(string nombre) 
        {
            Nombre = nombre;
        }

        public string Nombre { get; set; }
    }
}

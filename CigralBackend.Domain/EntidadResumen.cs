using CigralBackend.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Domain
{
    public class EntidadResumen : EntityBase
    {
        public EntidadResumen() { }

        public int IdOriginal { get; set; }
        public string TipoEntidad { get; set; } // "Cliente" o "Proveedor"
        public string? RazonSocial { get; set; }
        public string? GLN { get; set; }
        public string? Email { get; set; }
        public string? Cuit { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
    }
}

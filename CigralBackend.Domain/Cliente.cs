using CigralBackend.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Domain
{
    public class Cliente : EntityBase
    {
        public Cliente() { }
        public string? RazonSocial { get; set; }
        public string? GLN { get; set; }
        public string? Email { get; set; }
        public string? Cuit { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public List<RemitoEgreso> Remitos { get; set; }
    }
}

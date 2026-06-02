using CigralBackend.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Domain
{
    public class Consignacion : EntityBase
    {
        public Consignacion() { }
        public Existencia Existencia { get; set; }
        public int ExistenciaId { get; set; }
        public Cliente Cliente { get; set; }
        public int ClienteId { get; set; }
        public int Cantidad { get; set; }
        public DateTime FechaModificacion { get; set; }
    }
}

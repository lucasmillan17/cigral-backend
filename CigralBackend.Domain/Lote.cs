using CigralBackend.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Domain
{
    public class Lote : EntityBase
    {
        public Lote() { }

        public string CodigoLote { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int CantidadDisponible { get; set; }
        public Producto Producto { get; set; }
        public int ProductoId { get; set; }
        public bool Activo { get; set; } = true;
    }
}

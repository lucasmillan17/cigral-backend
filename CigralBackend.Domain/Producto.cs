using CigralBackend.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Domain
{
    public class Producto : EntityBase
    {
        public Producto() { }

        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string GTIN { get; set; }
        public List<Lote>? Lotes { get; set; }
        public int? MarcaId { get; set; }
        public Marca? Marca { get; set; }
        public decimal? Precio { get; set; }
        public bool EsUnitario { get; set; }
    }
}

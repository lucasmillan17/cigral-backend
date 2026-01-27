using CigralBackend.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Domain
{
    public class DetalleRemito : EntityBase
    {
        public DetalleRemito() { }
        
        public int RemitoIngresoId { get; set; }
        public RemitoIngreso? RemitoIngreso { get; set; }
        
        public int RemitoEgresoId { get; set; }
        public RemitoEgreso? RemitoEgreso { get; set; }
        
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }
        
        public int? LoteId { get; set; }
        public Lote? Lote { get; set; }
        
        public string? NumeroSerie { get; set; }
        
        public int Cantidad { get; set; }
    }
}

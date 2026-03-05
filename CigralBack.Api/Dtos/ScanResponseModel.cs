using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Application.Dtos
{
    public class ScanResponseDto
    {
        // --- Datos que vienen del Parser ---
        public string Gtin { get; set; }
        public string Lote { get; set; }
        public string NumeroSerie { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public int Cantidad { get; set; }

        // --- Datos de Contexto (Base de Datos) ---
        public bool ExisteProducto { get; set; }
        public int? ProductoId { get; set; }
        public string? NombreProducto { get; set; }
        public string? InformacionAdicional { get; set; }
    }
}

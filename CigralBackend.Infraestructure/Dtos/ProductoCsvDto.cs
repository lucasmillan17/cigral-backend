using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Infraestructure.Dtos
{
    public class ProductoCsvDto
    {
        [Name("Codigo")]
        public string Codigo { get; set; }

        [Name("Denominacion")]
        public string Denominacion { get; set; }

        [Name("Unidades")]
        public decimal? Unidades { get; set; }

        [Name("Unid. medida")]
        public string? UnidadMedida { get; set; }

        [Name("Cant.Minima")]
        public decimal? CantidadMinima { get; set; }
    }
}

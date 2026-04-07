using CsvHelper.Configuration.Attributes;

namespace CigralBackend.Infraestructure.Dtos
{
    public class ClienteCsvDto
    {
        [Name("Razon Social")]
        public string? RazonSocial { get; set; }

        [Name("Numero")] // Aquí viene el CUIT en tu CSV
        public string? Cuit { get; set; }

        [Name("Linea 1 direccion")]
        public string? Linea1 { get; set; }

        [Name("Linea 2 direccion")]
        public string? Linea2 { get; set; }

        [Name("Linea 3 direccion")]
        public string? Linea3 { get; set; }

        [Name("Telefono")]
        public string? Telefono { get; set; }
    }

    public class ProveedorCsvDto
    {
        [Name("Denominacion")]
        [NameIndex(0)] // Obligamos a tomar la 1ra columna "Denominacion"
        public string? RazonSocial { get; set; }

        [Name("Nro.ident.impositiva")] // Aquí viene el CUIT en tu CSV
        public string? Cuit { get; set; }

        [Name("Direccion")]
        public string? Direccion1 { get; set; }

        [Name("Direccion 2")]
        public string? Direccion2 { get; set; }

        [Name("Direccion 3")]
        public string? Direccion3 { get; set; }

        [Name("Telefono")]
        public string? Telefono { get; set; }
    }
}
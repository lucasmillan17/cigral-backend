using CigralBackend.Infraestructure.Dtos;
using CigralBackend.Infraestructure.Services.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using System.Text;

namespace CigralBackend.Infraestructure.Services
{
    public class CsvCatalogParserService : ICatalogParserService
    {
        public List<ProductoCsvDto> ParsearCatalogo(Stream archivoStream)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ";",
                MissingFieldFound = null
            };

            // Usamos leaveOpen: true por si el stream necesita seguir vivo después, 
            // aunque el garbage collector igual limpiará el StreamReader.
            using var reader = new StreamReader(archivoStream, leaveOpen: true);
            using var csv = new CsvReader(reader, config);

            // Esto lee el CSV y lo mapea automáticamente al DTO
            return csv.GetRecords<ProductoCsvDto>().ToList();
        }

        public List<ClienteCsvDto> ParsearClientes(Stream archivoStream)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null
            };

            using var reader = new StreamReader(archivoStream, Encoding.Latin1, leaveOpen: true);
            using var csv = new CsvReader(reader, config);

            return csv.GetRecords<ClienteCsvDto>().ToList();
        }

        public List<ProveedorCsvDto> ParsearProveedores(Stream archivoStream)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null
            };

            using var reader = new StreamReader(archivoStream, Encoding.Latin1, leaveOpen: true);
            using var csv = new CsvReader(reader, config);

            return csv.GetRecords<ProveedorCsvDto>().ToList();
        }
    }
}
using CigralBackend.Infraestructure.Dtos;

namespace CigralBackend.Infraestructure.Services.Interfaces
{
    public interface ICatalogParserService
    {
        List<ProductoCsvDto> ParsearCatalogo(Stream archivoStream);
        List<ClienteCsvDto> ParsearClientes(Stream archivoStream);
        List<ProveedorCsvDto> ParsearProveedores(Stream archivoStream);
    }
}
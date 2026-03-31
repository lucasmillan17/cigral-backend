using CigralBackend.Infraestructure.Dtos;

namespace CigralBackend.Infraestructure.Services.Interfaces
{
    public interface ICatalogParserService
    {
        List<ProductoCsvDto> ParsearCatalogo(Stream archivoStream);
    }
}
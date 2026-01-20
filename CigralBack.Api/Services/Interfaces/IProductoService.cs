using CigralBackend.Application.Dtos;

namespace CigralBackend.Application.Services.Interfaces
{
    public interface IProductoService
    {
        Task CreateProducto(ProductoModelRequest r);
    }
}
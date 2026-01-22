using CigralBackend.Application.Dtos;
using CigralBackend.Domain.Wrappers;

namespace CigralBackend.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz de servicio para operaciones de productos.
    /// </summary>
    public interface IProductoService
    {
        /// <summary>
        /// Crea un nuevo producto.
        /// </summary>
        /// <param name="r">Datos del producto</param>
        /// <returns>El producto creado</returns>
        Task<ProductoModelResponse> CreateProducto(ProductoModelRequest r);

        /// <summary>
        /// Obtiene todos los productos con paginacion.
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Tamano de pagina</param>
        /// <returns>Resultado paginado de productos</returns>
        Task<PagedResult<ProductoModelResponse>> GetAllProductos(int pageNumber, int pageSize);

        /// <summary>
        /// Obtiene productos filtrados con paginacion.
        /// </summary>
        /// <param name="f">Filtros de busqueda</param>
        /// <returns>Resultado paginado de productos filtrados</returns>
        Task<PagedResult<ProductoModelResponse>> GetProductoFiltered(ProductoFilters f);

        /// <summary>
        /// Obtiene un producto por su ID.
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>El producto encontrado</returns>
        Task<ProductoModelResponse> GetProductoById(int id);

        /// <summary>
        /// Actualiza un producto existente.
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <param name="r">Nuevos datos del producto</param>
        /// <returns>El producto actualizado</returns>
        Task<ProductoModelResponse> UpdateProducto(int id, ProductoModelRequest r);

        /// <summary>
        /// Elimina un producto.
        /// </summary>
        /// <param name="id">ID del producto a eliminar</param>
        Task DeleteProducto(int id);
    }
}
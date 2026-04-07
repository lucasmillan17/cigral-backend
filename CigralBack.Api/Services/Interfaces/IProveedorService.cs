using CigralBackend.Application.Dtos;
using CigralBackend.Domain.Wrappers;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de proveedores.
    /// </summary>
    public interface IProveedorService
    {
        /// <summary>
        /// Crea un nuevo proveedor.
        /// </summary>
        Task<ProveedorModelResponse> CreateProveedor(ProveedorModelRequest request);

        /// <summary>
        /// Obtiene un proveedor por su ID.
        /// </summary>
        Task<ProveedorModelResponse> GetProveedorById(int id);

        /// <summary>
        /// Obtiene proveedores filtrados con paginación.
        /// </summary>
        Task<PagedResult<ProveedorModelResponse>> GetProveedores(ProveedorFilters filters);

        /// <summary>
        /// Actualiza un proveedor existente.
        /// </summary>
        Task<ProveedorModelResponse> UpdateProveedor(int id, ProveedorModelRequest request);

        /// <summary>
        /// Elimina un proveedor.
        /// </summary>
        Task DeleteProveedor(int id);
        Task ImportarProveedoresCsvAsync(Stream fileStream);
    }
}

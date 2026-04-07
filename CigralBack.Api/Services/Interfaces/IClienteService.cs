using CigralBackend.Application.Dtos;
using CigralBackend.Domain.Wrappers;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de clientes.
    /// </summary>
    public interface IClienteService
    {
        /// <summary>
        /// Crea un nuevo cliente.
        /// </summary>
        Task<ClienteModelResponse> CreateCliente(ClienteModelRequest request);

        /// <summary>
        /// Obtiene un cliente por su ID.
        /// </summary>
        Task<ClienteModelResponse> GetClienteById(int id);

        /// <summary>
        /// Obtiene clientes filtrados con paginación.
        /// </summary>
        Task<PagedResult<ClienteModelResponse>> GetClientes(ClienteFilters filters);

        /// <summary>
        /// Actualiza un cliente existente.
        /// </summary>
        Task<ClienteModelResponse> UpdateCliente(int id, ClienteModelRequest request);

        /// <summary>
        /// Elimina un cliente.
        /// </summary>
        Task DeleteCliente(int id);
        Task<PagedResult<EntidadResumenResponse>> GetEntidades(ClienteFilters filters);
        Task ImportarClientesCsvAsync(Stream fileStream);
    }
}

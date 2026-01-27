using CigralBackend.Application.Dtos;
using CigralBackend.Domain.Wrappers;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de depósitos.
    /// </summary>
    public interface IDepositoService
    {
        /// <summary>
        /// Crea un nuevo depósito.
        /// </summary>
        Task<DepositoModelResponse> CreateDeposito(DepositoModelRequest request);

        /// <summary>
        /// Obtiene un depósito por su ID.
        /// </summary>
        Task<DepositoModelResponse> GetDepositoById(int id);

        /// <summary>
        /// Obtiene depósitos filtrados con paginación.
        /// </summary>
        Task<PagedResult<DepositoModelResponse>> GetDepositos(DepositoFilters filters);

        /// <summary>
        /// Actualiza un depósito existente.
        /// </summary>
        Task<DepositoModelResponse> UpdateDeposito(int id, DepositoModelRequest request);

        /// <summary>
        /// Elimina un depósito.
        /// </summary>
        Task DeleteDeposito(int id);
    }
}

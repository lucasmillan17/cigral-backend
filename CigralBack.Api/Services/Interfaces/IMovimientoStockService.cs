using CigralBackend.Application.Dtos;
using CigralBackend.Domain.Wrappers;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de auditoría de movimientos de stock.
    /// </summary>
    public interface IMovimientoStockService
    {
        /// <summary>
        /// Obtiene movimientos de stock filtrados con paginación.
        /// </summary>
        /// <param name="filters">Filtros a aplicar</param>
        /// <returns>Resultado paginado de movimientos</returns>
        Task<PagedResult<MovimientoStockResponse>> GetMovimientos(MovimientoStockFilters filters);

        /// <summary>
        /// Obtiene un movimiento de stock por su ID.
        /// </summary>
        /// <param name="id">ID del movimiento</param>
        /// <returns>El movimiento encontrado</returns>
        Task<MovimientoStockResponse> GetMovimientoById(int id);
    }
}

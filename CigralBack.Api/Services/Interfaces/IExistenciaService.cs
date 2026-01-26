using CigralBackend.Application.Dtos;
using CigralBackend.Domain.Wrappers;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de existencias.
    /// </summary>
    public interface IExistenciaService
    {
        /// <summary>
        /// Aumenta el stock de un producto. Si la existencia no existe, la crea. Si existe, suma la cantidad.
        /// </summary>
        /// <param name="r">Datos del movimiento de stock</param>
        /// <returns>La existencia actualizada o creada</returns>
        Task<ExistenciaModelResponse> AumentarStock(ExistenciaModelRequest r);

        /// <summary>
        /// Disminuye el stock de un producto. Valida que haya stock suficiente.
        /// </summary>
        /// <param name="r">Datos del movimiento de stock</param>
        /// <returns>La existencia actualizada</returns>
        Task<ExistenciaModelResponse> DisminuirStock(ExistenciaModelRequest r);

        /// <summary>
        /// Obtiene una existencia por su ID.
        /// </summary>
        /// <param name="id">ID de la existencia</param>
        /// <returns>La existencia encontrada</returns>
        Task<ExistenciaModelResponse> GetExistenciaById(int id);

        /// <summary>
        /// Obtiene existencias filtradas con paginacion.
        /// </summary>
        /// <param name="filters">Filtros a aplicar</param>
        /// <returns>Resultado paginado de existencias</returns>
        Task<PagedResult<ExistenciaModelResponse>> GetExistencias(ExistenciaFilters filters);

        /// <summary>
        /// Elimina una existencia del sistema (solo si cantidad = 0).
        /// </summary>
        /// <param name="id">ID de la existencia a eliminar</param>
        Task DeleteExistencia(int id);
    }
}

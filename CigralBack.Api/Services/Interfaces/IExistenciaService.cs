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
        /// Registra el movimiento en la auditoría.
        /// </summary>
        /// <param name="r">Datos del movimiento de stock</param>
        /// <param name="remitoIngresoId">ID del remito de ingreso (opcional)</param>
        /// <param name="observaciones">Observaciones adicionales (opcional)</param>
        /// <returns>La existencia actualizada o creada</returns>
        Task<ExistenciaModelResponse> AumentarStock(
            ExistenciaModelRequest r,
            int? remitoIngresoId = null,
            string? observaciones = null);

        /// <summary>
        /// Disminuye el stock de un producto. Valida que haya stock suficiente.
        /// Registra el movimiento en la auditoría.
        /// </summary>
        /// <param name="r">Datos del movimiento de stock</param>
        /// <param name="remitoEgresoId">ID del remito de egreso (opcional)</param>
        /// <param name="observaciones">Observaciones adicionales (opcional)</param>
        /// <returns>La existencia actualizada</returns>
        Task<ExistenciaModelResponse> DisminuirStock(
            ExistenciaModelRequest r,
            int? remitoEgresoId = null,
            string? observaciones = null);

        /// <summary>
        /// Obtiene una existencia por su ID.
        /// </summary>
        /// <param name="id">ID de la existencia</param>
        /// <returns>La existencia encontrada</returns>
        Task<ExistenciaModelResponse> GetExistenciaById(int id);

        /// <summary>
        /// Obtiene existencias filtradas con paginación.
        /// Ahora incluye filtros por fecha de vencimiento y días para vencer.
        /// </summary>
        /// <param name="filters">Filtros a aplicar</param>
        /// <returns>Resultado paginado de existencias</returns>
        Task<PagedResult<ExistenciaModelResponse>> GetExistencias(ExistenciaFilters filters);

        /// <summary>
        /// Obtiene dashboard de productos próximos a vencer agrupados por rangos.
        /// </summary>
        /// <returns>Dashboard con estadísticas de vencimientos</returns>
        Task<DashboardVencimientosResponse> GetDashboardVencimientos();

        /// <summary>
        /// Obtiene productos próximos a vencer según filtros específicos.
        /// </summary>
        /// <param name="filters">Filtros de vencimiento</param>
        /// <returns>Lista de productos próximos a vencer</returns>
        Task<List<ProductoProximoVencerDto>> GetProductosProximosVencer(VencimientoFilters filters);

        /// <summary>
        /// Elimina una existencia del sistema (solo si cantidad = 0).
        /// </summary>
        /// <param name="id">ID de la existencia a eliminar</param>
        Task DeleteExistencia(int id);
        Task<int> GetStockDisponible(int productoId, string? codigoLote = null, string? numSerie = null);
        Task<StockDisponibleResponse> GetStockDisponible(int existenciaId);
    }
}

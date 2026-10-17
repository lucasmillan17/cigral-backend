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
        /// Aplica varios aumentos o disminuciones sin remito en una sola transacción (todos o ninguno).
        /// </summary>
        Task<ResultadoOperacion<List<ExistenciaModelResponse>>> AplicarMovimientos(
            List<ExistenciaModelRequest> movimientos,
            bool esAumento);

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
        /// Igual que GetExistencias, pero sumando las existencias de un mismo producto, lote, serie y
        /// vencimiento que están en distintos depósitos. Pagina por grupo.
        /// </summary>
        Task<PagedResult<ExistenciaAgrupadaResponse>> GetExistenciasAgrupadas(ExistenciaFilters filters);

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

        /// <summary>
        /// Corrección de soporte: cambia lote, serie, vencimiento, depósito o cantidad de una existencia.
        /// Si la nueva combinación ya existe en otra existencia, se suman y la original se elimina.
        /// Registra los movimientos de ajuste y, si hay ticket, deja un comentario en él.
        /// </summary>
        Task<ExistenciaModelResponse> CorregirExistencia(int id, ExistenciaCorreccionRequest r);
        Task<int> GetStockDisponible(int productoId, string? codigoLote = null, string? numSerie = null);
        Task<StockDisponibleResponse> GetStockDisponible(int existenciaId);
    }
}

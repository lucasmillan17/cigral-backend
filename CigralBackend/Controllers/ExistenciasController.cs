using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain;
using CigralBackend.Domain.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CigralBackend.Controllers
{
    /// <summary>
    /// Controlador para operaciones de stock y existencias.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ExistenciasController : ControllerBase
    {
        private readonly IExistenciaService _existenciaService;
        private readonly IAjusteStockService _ajusteStockService;

        public ExistenciasController(IExistenciaService existenciaService, IAjusteStockService ajusteStockService)
        {
            _existenciaService = existenciaService;
            _ajusteStockService = ajusteStockService;
        }

        /// <summary>
        /// Obtiene existencias con filtros y paginación.
        /// Ahora incluye filtros por fecha de vencimiento y días para vencer.
        /// </summary>
        /// <param name="filters">Filtros de búsqueda</param>
        /// <returns>Lista paginada de existencias</returns>
        /// <remarks>
        /// Ejemplos de uso:
        /// 
        /// - Todos los productos: GET /api/existencias
        /// - Por depósito: GET /api/existencias?depositoId=1
        /// - Productos que vencen en 30 días: GET /api/existencias?diasParaVencer=30
        /// - Productos que vencen entre fechas: GET /api/existencias?fechaVencimientoDesde=2025-01-01&amp;fechaVencimientoHasta=2025-03-31
        /// - Solo productos con vencimiento: GET /api/existencias?soloConVencimiento=true
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ExistenciaModelResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetExistencias([FromQuery] ExistenciaFilters filters)
        {
            var existencias = await _existenciaService.GetExistencias(filters);
            return Ok(existencias);
        }

        /// <summary>
        /// Existencias sumadas por producto, lote, serie y vencimiento, con el detalle por depósito.
        /// Acepta los mismos filtros que GET /api/existencias y pagina por grupo.
        /// </summary>
        [HttpGet("agrupadas")]
        [ProducesResponseType(typeof(PagedResult<ExistenciaAgrupadaResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetExistenciasAgrupadas([FromQuery] ExistenciaFilters filters)
        {
            return Ok(await _existenciaService.GetExistenciasAgrupadas(filters));
        }

        /// <summary>
        /// Obtiene una existencia por su ID.
        /// </summary>
        /// <param name="id">ID de la existencia</param>
        /// <returns>La existencia solicitada</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ExistenciaModelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var existencia = await _existenciaService.GetExistenciaById(id);
            return Ok(existencia);
        }

        /// <summary>
        /// Dashboard de productos próximos a vencer.
        /// Retorna productos agrupados por rangos de días (0-30, 31-60, 61-90, etc.)
        /// </summary>
        /// <returns>Dashboard con estadísticas de vencimientos</returns>
        /// <remarks>
        /// Este endpoint es ideal para mostrar un semáforo en el frontend:
        /// 
        /// - Rojo (0-30 días): Productos críticos
        /// - Amarillo (31-90 días): Productos próximos a vencer
        /// - Verde (91-180 días): Productos con vencimiento lejano
        /// 
        /// Retorna:
        /// - Total de productos/lotes próximos a vencer
        /// - Datos agrupados por rangos de días
        /// - Lista detallada de productos en cada rango
        /// </remarks>
        [HttpGet("dashboard/vencimientos")]
        [ProducesResponseType(typeof(DashboardVencimientosResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboardVencimientos()
        {
            var dashboard = await _existenciaService.GetDashboardVencimientos();
            return Ok(dashboard);
        }

        /// <summary>
        /// Obtiene productos próximos a vencer con filtros personalizados.
        /// </summary>
        /// <param name="diasDesde">Días desde hoy (ej: 0 = hoy)</param>
        /// <param name="diasHasta">Días hasta (ej: 90 = 3 meses)</param>
        /// <param name="depositoId">Filtrar por depósito (opcional)</param>
        /// <param name="productoId">Filtrar por producto (opcional)</param>
        /// <param name="incluirVencidos">Incluir productos ya vencidos</param>
        /// <returns>Lista de productos próximos a vencer</returns>
        /// <remarks>
        /// Ejemplos de uso:
        /// 
        /// - Productos que vencen en los próximos 30 días:
        ///   GET /api/existencias/proximos-vencer?diasDesde=0&amp;diasHasta=30
        ///   
        /// - Productos que vencen entre 30 y 60 días:
        ///   GET /api/existencias/proximos-vencer?diasDesde=30&amp;diasHasta=60
        ///   
        /// - Productos vencidos:
        ///   GET /api/existencias/proximos-vencer?diasDesde=-365&amp;diasHasta=-1&amp;incluirVencidos=true
        /// </remarks>
        [HttpGet("proximos-vencer")]
        [ProducesResponseType(typeof(List<ProductoProximoVencerDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductosProximosVencer(
            [FromQuery] int? diasDesde,
            [FromQuery] int? diasHasta,
            [FromQuery] int? depositoId,
            [FromQuery] int? productoId,
            [FromQuery] bool incluirVencidos = false)
        {
            var filters = new VencimientoFilters(
                DiasDesde: diasDesde,
                DiasHasta: diasHasta,
                DepositoId: depositoId,
                ProductoId: productoId,
                IncluirVencidos: incluirVencidos
            );

            var productos = await _existenciaService.GetProductosProximosVencer(filters);
            return Ok(productos);
        }

        /// <summary>
        /// Aumenta el stock de un producto manualmente (ajuste positivo).
        /// </summary>
        /// <param name="request">Datos del ajuste de stock</param>
        /// <returns>La existencia actualizada o creada</returns>
        [HttpPost("aumentar")]
        [ProducesResponseType(typeof(ExistenciaModelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AumentarStock([FromBody] ExistenciaModelRequest request)
        {
            var existencia = await _existenciaService.AumentarStock(request);
            return Ok(existencia);
        }

        /// <summary>
        /// Disminuye el stock de un producto manualmente (ajuste negativo).
        /// </summary>
        /// <param name="request">Datos del ajuste de stock</param>
        /// <returns>La existencia actualizada</returns>
        [HttpPost("disminuir")]
        [ProducesResponseType(typeof(ExistenciaModelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DisminuirStock([FromBody] ExistenciaModelRequest request)
        {
            var existencia = await _existenciaService.DisminuirStock(request);
            return Ok(existencia);
        }

        /// <summary>
        /// Ingreso sin remito de varias líneas en una sola transacción: se guardan todas o ninguna.
        /// </summary>
        /// <returns>422 con { mensaje, errores: [{ orden, mensaje }] } si alguna línea falla (orden 0 = primera)</returns>
        [HttpPost("aumentar-varios")]
        [ProducesResponseType(typeof(List<ExistenciaModelResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AumentarVarios([FromBody] List<ExistenciaModelRequest> movimientos)
            => await AplicarMovimientos(movimientos, esAumento: true);

        /// <summary>
        /// Egreso sin remito de varias líneas en una sola transacción: se guardan todas o ninguna.
        /// </summary>
        /// <returns>422 con { mensaje, errores: [{ orden, mensaje }] } si alguna línea falla (orden 0 = primera)</returns>
        [HttpPost("disminuir-varios")]
        [ProducesResponseType(typeof(List<ExistenciaModelResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DisminuirVarios([FromBody] List<ExistenciaModelRequest> movimientos)
            => await AplicarMovimientos(movimientos, esAumento: false);

        private async Task<IActionResult> AplicarMovimientos(List<ExistenciaModelRequest> movimientos, bool esAumento)
        {
            if (movimientos == null || movimientos.Count == 0)
                return BadRequest(new { message = "No hay movimientos para guardar." });

            var resultado = await _existenciaService.AplicarMovimientos(movimientos, esAumento);
            if (!resultado.Exito)
            {
                return UnprocessableEntity(new
                {
                    mensaje = resultado.MensajeGeneral,
                    errores = resultado.ErroresDetalle
                });
            }
            return Ok(resultado.Datos);
        }

        /// <summary>
        /// Elimina una existencia (solo si cantidad = 0).
        /// </summary>
        /// <param name="id">ID de la existencia</param>
        /// <returns>No content si fue exitoso</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _existenciaService.DeleteExistencia(id);
            return NoContent();
        }

        /// <summary>
        /// Corrección de soporte: lote, serie, vencimiento, depósito o cantidad de una existencia.
        /// Requiere ticket o motivo. Si la nueva combinación ya existe, se suman y la original se elimina.
        /// </summary>
        [HttpPut("{id}/correccion")]
        [Authorize(Roles = Roles.Soporte)]
        [ProducesResponseType(typeof(ExistenciaModelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Corregir(int id, [FromBody] ExistenciaCorreccionRequest request)
        {
            return Ok(await _existenciaService.CorregirExistencia(id, request));
        }

        /// <summary>
        /// Ajuste de stock por conteo físico: se manda lo contado de cada línea y el sistema calcula la diferencia.
        /// Todo o nada. Requiere ticket o detalle. Solo soporte.
        /// </summary>
        /// <returns>422 con { mensaje, errores: [{ orden, mensaje }] } si alguna línea falla (orden 0 = primera)</returns>
        [HttpPost("ajuste")]
        [Authorize(Roles = Roles.Soporte)]
        [ProducesResponseType(typeof(AjusteStockResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Ajustar([FromBody] AjusteStockRequest request)
        {
            var resultado = await _ajusteStockService.Ajustar(request);
            if (!resultado.Exito)
            {
                return UnprocessableEntity(new
                {
                    mensaje = resultado.MensajeGeneral,
                    errores = resultado.ErroresDetalle
                });
            }
            return Ok(resultado.Datos);
        }

        [HttpGet("{id}/stock-disponible")]
        [ProducesResponseType(typeof(StockDisponibleResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStockDisponible(int id)
        {
            var cantidad = await _existenciaService.GetStockDisponible(id);
            return Ok(cantidad);
        }
    }
}

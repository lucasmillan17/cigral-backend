using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CigralBackend.Controllers
{
    /// <summary>
    /// Controlador para consultar la auditoría de movimientos de stock.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuditoriaController : ControllerBase
    {
        private readonly IMovimientoStockService _movimientoStockService;

        public AuditoriaController(IMovimientoStockService movimientoStockService)
        {
            _movimientoStockService = movimientoStockService;
        }

        /// <summary>
        /// Obtiene movimientos de stock con filtros y paginación.
        /// </summary>
        /// <param name="filters">Filtros de búsqueda</param>
        /// <returns>Lista paginada de movimientos de stock</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<MovimientoStockResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMovimientos([FromQuery] MovimientoStockFilters filters)
        {
            var movimientos = await _movimientoStockService.GetMovimientos(filters);
            return Ok(movimientos);
        }

        /// <summary>
        /// Obtiene un movimiento de stock por su ID.
        /// </summary>
        /// <param name="id">ID del movimiento</param>
        /// <returns>El movimiento solicitado</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MovimientoStockResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var movimiento = await _movimientoStockService.GetMovimientoById(id);
            return Ok(movimiento);
        }
    }
}

using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CigralBackend.Controllers
{
    /// <summary>
    /// Controlador para operaciones de existencias e inventario.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ExistenciasController : ControllerBase
    {
        private readonly IExistenciaService _existenciaService;

        public ExistenciasController(IExistenciaService existenciaService)
        {
            _existenciaService = existenciaService;
        }

        /// <summary>
        /// Obtiene existencias filtradas con paginacion.
        /// </summary>
        /// <param name="filters">Filtros de busqueda</param>
        /// <returns>Lista paginada de existencias</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ExistenciaModelResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetExistencias([FromQuery] ExistenciaFilters filters)
        {
            var existencias = await _existenciaService.GetExistencias(filters);
            return Ok(existencias);
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
        /// Aumenta el stock de un producto. Si la existencia no existe, la crea.
        /// </summary>
        /// <param name="request">Datos del movimiento de entrada de stock</param>
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
        /// Disminuye el stock de un producto. Valida que haya stock suficiente.
        /// </summary>
        /// <param name="request">Datos del movimiento de salida de stock</param>
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
        /// Elimina una existencia (solo si cantidad = 0).
        /// </summary>
        /// <param name="id">ID de la existencia a eliminar</param>
        /// <returns>No content si fue exitoso</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            await _existenciaService.DeleteExistencia(id);
            return NoContent();
        }
    }
}

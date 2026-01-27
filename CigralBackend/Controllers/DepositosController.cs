using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CigralBackend.Controllers
{
    /// <summary>
    /// Controlador para operaciones CRUD de depósitos.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DepositosController : ControllerBase
    {
        private readonly IDepositoService _depositoService;

        public DepositosController(IDepositoService depositoService)
        {
            _depositoService = depositoService;
        }

        /// <summary>
        /// Obtiene depósitos con filtros y paginación.
        /// </summary>
        /// <param name="filters">Filtros de búsqueda</param>
        /// <returns>Lista paginada de depósitos</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<DepositoModelResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDepositos([FromQuery] DepositoFilters filters)
        {
            var depositos = await _depositoService.GetDepositos(filters);
            return Ok(depositos);
        }

        /// <summary>
        /// Obtiene un depósito por su ID.
        /// </summary>
        /// <param name="id">ID del depósito</param>
        /// <returns>El depósito solicitado</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DepositoModelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var deposito = await _depositoService.GetDepositoById(id);
            return Ok(deposito);
        }

        /// <summary>
        /// Crea un nuevo depósito.
        /// </summary>
        /// <param name="request">Datos del depósito</param>
        /// <returns>El depósito creado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(DepositoModelResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] DepositoModelRequest request)
        {
            var deposito = await _depositoService.CreateDeposito(request);
            return CreatedAtAction(nameof(GetById), new { id = deposito.Id }, deposito);
        }

        /// <summary>
        /// Actualiza un depósito existente.
        /// </summary>
        /// <param name="id">ID del depósito</param>
        /// <param name="request">Nuevos datos del depósito</param>
        /// <returns>El depósito actualizado</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DepositoModelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] DepositoModelRequest request)
        {
            var deposito = await _depositoService.UpdateDeposito(id, request);
            return Ok(deposito);
        }

        /// <summary>
        /// Elimina un depósito.
        /// </summary>
        /// <param name="id">ID del depósito</param>
        /// <returns>No content si fue exitoso</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _depositoService.DeleteDeposito(id);
            return NoContent();
        }
    }
}

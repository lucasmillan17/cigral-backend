using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CigralBackend.Controllers
{
    /// <summary>
    /// Controlador para operaciones CRUD de clientes.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        /// <summary>
        /// Obtiene clientes con filtros y paginación.
        /// </summary>
        /// <param name="filters">Filtros de búsqueda</param>
        /// <returns>Lista paginada de clientes</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ClienteModelResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClientes([FromQuery] ClienteFilters filters)
        {
            var clientes = await _clienteService.GetClientes(filters);
            return Ok(clientes);
        }

        /// <summary>
        /// Obtiene un cliente por su ID.
        /// </summary>
        /// <param name="id">ID del cliente</param>
        /// <returns>El cliente solicitado</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ClienteModelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var cliente = await _clienteService.GetClienteById(id);
            return Ok(cliente);
        }

        /// <summary>
        /// Crea un nuevo cliente.
        /// </summary>
        /// <param name="request">Datos del cliente</param>
        /// <returns>El cliente creado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ClienteModelResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] ClienteModelRequest request)
        {
            var cliente = await _clienteService.CreateCliente(request);
            return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
        }

        /// <summary>
        /// Actualiza un cliente existente.
        /// </summary>
        /// <param name="id">ID del cliente</param>
        /// <param name="request">Nuevos datos del cliente</param>
        /// <returns>El cliente actualizado</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ClienteModelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] ClienteModelRequest request)
        {
            var cliente = await _clienteService.UpdateCliente(id, request);
            return Ok(cliente);
        }

        /// <summary>
        /// Elimina un cliente.
        /// </summary>
        /// <param name="id">ID del cliente</param>
        /// <returns>No content si fue exitoso</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _clienteService.DeleteCliente(id);
            return NoContent();
        }

        [HttpGet("entidades")]
        [ProducesResponseType(typeof(PagedResult<EntidadResumenResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEntidades([FromQuery] ClienteFilters filters)
        {
            var clientes = await _clienteService.GetEntidades(filters);
            return Ok(clientes);
        }

    }
}

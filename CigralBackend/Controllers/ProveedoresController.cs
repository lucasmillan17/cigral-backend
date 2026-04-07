using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CigralBackend.Controllers
{
    /// <summary>
    /// Controlador para operaciones CRUD de proveedores.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedoresController : ControllerBase
    {
        private readonly IProveedorService _proveedorService;

        public ProveedoresController(IProveedorService proveedorService)
        {
            _proveedorService = proveedorService;
        }

        /// <summary>
        /// Obtiene proveedores con filtros y paginación.
        /// </summary>
        /// <param name="filters">Filtros de búsqueda</param>
        /// <returns>Lista paginada de proveedores</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ProveedorModelResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProveedores([FromQuery] ProveedorFilters filters)
        {
            var proveedores = await _proveedorService.GetProveedores(filters);
            return Ok(proveedores);
        }

        /// <summary>
        /// Obtiene un proveedor por su ID.
        /// </summary>
        /// <param name="id">ID del proveedor</param>
        /// <returns>El proveedor solicitado</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProveedorModelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var proveedor = await _proveedorService.GetProveedorById(id);
            return Ok(proveedor);
        }

        /// <summary>
        /// Crea un nuevo proveedor.
        /// </summary>
        /// <param name="request">Datos del proveedor</param>
        /// <returns>El proveedor creado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProveedorModelResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] ProveedorModelRequest request)
        {
            var proveedor = await _proveedorService.CreateProveedor(request);
            return CreatedAtAction(nameof(GetById), new { id = proveedor.Id }, proveedor);
        }

        /// <summary>
        /// Actualiza un proveedor existente.
        /// </summary>
        /// <param name="id">ID del proveedor</param>
        /// <param name="request">Nuevos datos del proveedor</param>
        /// <returns>El proveedor actualizado</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProveedorModelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] ProveedorModelRequest request)
        {
            var proveedor = await _proveedorService.UpdateProveedor(id, request);
            return Ok(proveedor);
        }

        /// <summary>
        /// Elimina un proveedor.
        /// </summary>
        /// <param name="id">ID del proveedor</param>
        /// <returns>No content si fue exitoso</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _proveedorService.DeleteProveedor(id);
            return NoContent();
        }

        [HttpPost("importar")]
        public async Task<IActionResult> ImportarProveedores(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("El archivo es inválido o está vacío.");
            }

            try
            {
                using var stream = file.OpenReadStream();
                await _proveedorService.ImportarProveedoresCsvAsync(stream);
                return Ok(new { message = "Proveedores importados correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al procesar el archivo de proveedores", detail = ex.Message });
            }
        }
    }
}

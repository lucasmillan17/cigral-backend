using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CigralBackend.Controllers
{
    /// <summary>
    /// Controlador para operaciones de marcas.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MarcasController : ControllerBase
    {
        private readonly IMarcaService _marcaService;

        public MarcasController(IMarcaService marcaService)
        {
            _marcaService = marcaService;
        }

        /// <summary>
        /// Obtiene todas las marcas del sistema.
        /// </summary>
        /// <returns>Lista de todas las marcas</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<MarcaResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var marcas = await _marcaService.GetMarcasAsync();
            return Ok(marcas);
        }

        /// <summary>
        /// Obtiene marcas filtradas por nombre.
        /// </summary>
        /// <param name="nombre">Nombre o parte del nombre a buscar</param>
        /// <returns>Lista de marcas que coinciden</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<MarcaResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchByNombre([FromQuery] string nombre)
        {
            var marcas = await _marcaService.GetMarcasByNombre(nombre);
            return Ok(marcas);
        }

        /// <summary>
        /// Obtiene una marca por su ID.
        /// </summary>
        /// <param name="id">ID de la marca</param>
        /// <returns>La marca solicitada</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MarcaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var marca = await _marcaService.GetMarcaById(id);
            return Ok(marca);
        }

        /// <summary>
        /// Crea una nueva marca.
        /// </summary>
        /// <param name="request">Datos de la marca a crear</param>
        /// <returns>La marca creada</returns>
        [HttpPost]
        [ProducesResponseType(typeof(MarcaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] MarcaRequest request)
        {
            var createdMarca = await _marcaService.CreateMarca(request);
            return CreatedAtAction(nameof(GetById), new { id = createdMarca.Id }, createdMarca);
        }

        /// <summary>
        /// Actualiza una marca existente.
        /// </summary>
        /// <param name="id">ID de la marca a actualizar</param>
        /// <param name="request">Nuevos datos de la marca</param>
        /// <returns>La marca actualizada</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MarcaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] MarcaRequest request)
        {
            var updatedMarca = await _marcaService.UpdateMarca(id, request);
            return Ok(updatedMarca);
        }

        /// <summary>
        /// Elimina una marca.
        /// </summary>
        /// <param name="id">ID de la marca a eliminar</param>
        /// <returns>No content si fue exitoso</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            await _marcaService.DeleteMarca(id);
            return NoContent();
        }
    }
}

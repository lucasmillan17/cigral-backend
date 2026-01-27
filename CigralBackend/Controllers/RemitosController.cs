using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CigralBackend.Controllers
{
    /// <summary>
    /// Controlador para operaciones de remitos de ingreso y egreso.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RemitosController : ControllerBase
    {
        private readonly IRemitoService _remitoService;

        public RemitosController(IRemitoService remitoService)
        {
            _remitoService = remitoService;
        }

        /// <summary>
        /// Registra un remito de ingreso (entrada de mercadería de proveedor).
        /// Aumenta automáticamente el stock en el depósito especificado.
        /// </summary>
        /// <param name="request">Datos del remito de ingreso</param>
        /// <returns>Información del remito creado</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/remitos/ingreso
        ///     {
        ///        "depositoId": 1,
        ///        "entidadId": 5,
        ///        "numeroRemito": "REM-001",
        ///        "observaciones": "Ingreso de mercadería",
        ///        "detalles": [
        ///          {
        ///            "productoId": 10,
        ///            "loteId": 3,
        ///            "numeroSerie": null,
        ///            "cantidad": 100
        ///          }
        ///        ]
        ///     }
        ///
        /// </remarks>
        [HttpPost("ingreso")]
        [ProducesResponseType(typeof(RemitoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RegistrarIngreso([FromBody] RemitoRequest request)
        {
            var remito = await _remitoService.RegistrarIngreso(request);
            return CreatedAtAction(
                actionName: nameof(RegistrarIngreso),
                routeValues: new { id = remito.Id },
                value: remito
            );
        }

        /// <summary>
        /// Registra un remito de egreso (salida de mercadería a cliente).
        /// Disminuye automáticamente el stock en el depósito especificado.
        /// </summary>
        /// <param name="request">Datos del remito de egreso</param>
        /// <returns>Información del remito creado</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/remitos/egreso
        ///     {
        ///        "depositoId": 1,
        ///        "entidadId": 8,
        ///        "numeroRemito": "REM-SAL-001",
        ///        "observaciones": "Venta a cliente",
        ///        "detalles": [
        ///          {
        ///            "productoId": 10,
        ///            "loteId": 3,
        ///            "numeroSerie": null,
        ///            "cantidad": 50
        ///          }
        ///        ]
        ///     }
        ///
        /// </remarks>
        [HttpPost("egreso")]
        [ProducesResponseType(typeof(RemitoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RegistrarEgreso([FromBody] RemitoRequest request)
        {
            var remito = await _remitoService.RegistrarEgreso(request);
            return CreatedAtAction(
                actionName: nameof(RegistrarEgreso),
                routeValues: new { id = remito.Id },
                value: remito
            );
        }

        /// <summary>
        /// Actualiza un remito de ingreso (solo número y observaciones, NO afecta stock).
        /// </summary>
        /// <param name="id">ID del remito de ingreso</param>
        /// <param name="request">Datos a actualizar</param>
        /// <returns>Información del remito actualizado</returns>
        [HttpPut("ingreso/{id}")]
        [ProducesResponseType(typeof(RemitoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateIngreso(int id, [FromBody] UpdateRemitoRequest request)
        {
            var remito = await _remitoService.UpdateRemito(id, request, esIngreso: true);
            return Ok(remito);
        }

        /// <summary>
        /// Actualiza un remito de egreso (solo número y observaciones, NO afecta stock).
        /// </summary>
        /// <param name="id">ID del remito de egreso</param>
        /// <param name="request">Datos a actualizar</param>
        /// <returns>Información del remito actualizado</returns>
        [HttpPut("egreso/{id}")]
        [ProducesResponseType(typeof(RemitoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEgreso(int id, [FromBody] UpdateRemitoRequest request)
        {
            var remito = await _remitoService.UpdateRemito(id, request, esIngreso: false);
            return Ok(remito);
        }
    }
}

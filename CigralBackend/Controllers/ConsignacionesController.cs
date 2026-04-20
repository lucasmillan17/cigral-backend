using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CigralBackend.Api.Controllers
{
    [Authorize]
    [ApiController]    
    [Route("api/[controller]")]
    public class ConsignacionesController : ControllerBase
    {
        private readonly IConsignacionService _consignacionService;

        public ConsignacionesController(IConsignacionService consignacionService)
        {
            _consignacionService = consignacionService;
        }

        /// <summary>
        /// Crea una nueva consignación o aumenta la cantidad de una existente.
        /// </summary>
        [HttpPost("aumentar")]
        public async Task<IActionResult> AumentarConsignacion([FromBody] ConsignacionRequest request)
        {
            var response = await _consignacionService.AumentarConsignacion(request);
            return Ok(response);
        }

        /// <summary>
        /// Disminuye la cantidad de una consignación. Si llega a 0, la elimina.
        /// </summary>
        [HttpPut("{id}/disminuir")]
        public async Task<IActionResult> DisminuirConsignacion(int id, [FromBody] DisminuirConsignacionRequest request)
        {
            var response = await _consignacionService.DisminuirConsignacion(id, request.Cantidad);

            if (response == null)
            {
                // Devolvemos 200 OK con un mensaje personalizado para el frontend
                return Ok(new { mensaje = "La consignación llegó a 0 y fue eliminada exitosamente." });
            }

            return Ok(response);
        }

        /// <summary>
        /// Obtiene la lista de consignaciones paginadas y filtradas.
        /// </summary>
        [ProducesResponseType(typeof(GetConsignacionResponse), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> GetConsignaciones([FromQuery] ConsignacionFilters filters)
        {
            var response = await _consignacionService.GetConsignaciones(filters);
            return Ok(response);
        }
    }
}

// Puedes colocar este DTO en tu carpeta de Dtos, o dejarlo acá si solo se usa en este controlador.
namespace CigralBackend.Application.Dtos
{
    public record DisminuirConsignacionRequest(int Cantidad);
}
using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CigralBackend.Controllers
{
    /// <summary>Correcciones de soporte sobre lotes de un producto.</summary>
    [Authorize(Roles = Roles.Soporte)]
    [ApiController]
    [Route("api/[controller]")]
    public class LotesController : ControllerBase
    {
        private readonly ICorreccionLoteService _correcciones;

        public LotesController(ICorreccionLoteService correcciones)
        {
            _correcciones = correcciones;
        }

        /// <summary>
        /// Qué pasaría al corregir el lote de un producto (existencias afectadas, con cuáles se fusionan,
        /// líneas de remito), sin cambiar nada.
        /// </summary>
        [HttpGet("correccion/vista-previa")]
        [ProducesResponseType(typeof(CorreccionLotePrevia), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VistaPrevia([FromQuery] int productoId, [FromQuery] string codigoLote, [FromQuery] string? codigoNuevo)
            => Ok(await _correcciones.VistaPrevia(productoId, codigoLote, codigoNuevo));

        /// <summary>
        /// Corrección de soporte: renombra el lote del producto (si el código nuevo ya existe se fusionan) y/o
        /// cambia su vencimiento, en todas sus existencias y líneas de remito. Requiere ticket o motivo.
        /// </summary>
        [HttpPut("correccion")]
        [ProducesResponseType(typeof(CorreccionLoteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Corregir([FromBody] CorreccionLoteRequest request)
            => Ok(await _correcciones.Corregir(request));
    }
}

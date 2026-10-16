using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain;
using CigralBackend.Domain.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CigralBackend.Controllers
{
    /// <summary>
    /// Tickets de soporte: los usuarios reportan (por ahora, códigos que no cargan bien) y el equipo
    /// con rol Soporte los revisa. Cada usuario ve solo sus tickets; Soporte ve todos.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private static readonly JsonSerializerOptions OpcionesJson = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
        };

        private readonly ITicketService _service;

        public TicketsController(ITicketService service)
        {
            _service = service;
        }

        /// <summary>
        /// Crea un ticket. Formulario multipart: campo "datos" con el JSON de TicketCreateRequest
        /// y hasta 5 imágenes en "fotos" (3 MB cada una).
        /// </summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        [ProducesResponseType(typeof(TicketDetalleResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Crear([FromForm] string datos, [FromForm] List<IFormFile>? fotos)
        {
            TicketCreateRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<TicketCreateRequest>(datos, OpcionesJson);
            }
            catch (JsonException)
            {
                return BadRequest(new { mensaje = "El campo \"datos\" no es un JSON válido." });
            }
            if (request == null)
                return BadRequest(new { mensaje = "Faltan los datos del ticket." });

            var errores = new List<ValidationResult>();
            if (!Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true))
                return BadRequest(new { mensaje = string.Join(" ", errores.Select(e => e.ErrorMessage)) });

            var archivos = new List<TicketArchivo>();
            foreach (var foto in fotos ?? new List<IFormFile>())
            {
                using var ms = new MemoryStream();
                await foto.CopyToAsync(ms);
                archivos.Add(new TicketArchivo(Path.GetFileName(foto.FileName), foto.ContentType ?? "application/octet-stream", ms.ToArray()));
            }

            var ticket = await _service.Crear(request, archivos, UsuarioActual());
            return CreatedAtAction(nameof(Obtener), new { id = ticket.Id }, ticket);
        }

        /// <summary>Bandeja: Soporte ve todos los tickets; el resto, solo los suyos.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<TicketResumenResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] TicketFilters filtros)
        {
            return Ok(await _service.Listar(filtros, UsuarioActual()));
        }

        /// <summary>Cantidad de tickets abiertos y en revisión (para el contador del menú).</summary>
        [HttpGet("pendientes")]
        [ProducesResponseType(typeof(TicketPendientesResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Pendientes()
        {
            return Ok(await _service.Pendientes(UsuarioActual()));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TicketDetalleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Obtener(int id)
        {
            return Ok(await _service.Obtener(id, UsuarioActual()));
        }

        /// <summary>Devuelve una foto adjunta.</summary>
        [HttpGet("{id:int}/adjuntos/{adjuntoId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Adjunto(int id, int adjuntoId)
        {
            var adjunto = await _service.ObtenerAdjunto(id, adjuntoId, UsuarioActual());
            return File(adjunto.Datos, adjunto.ContentType, adjunto.NombreArchivo);
        }

        [HttpPost("{id:int}/comentarios")]
        [ProducesResponseType(typeof(TicketDetalleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Comentar(int id, [FromBody] TicketComentarioRequest request)
        {
            return Ok(await _service.Comentar(id, request.Texto, UsuarioActual()));
        }

        /// <summary>Cambia el estado (abierto, en revisión, resuelto, descartado). Solo Soporte.</summary>
        [HttpPut("{id:int}/estado")]
        [Authorize(Roles = Roles.Soporte)]
        [ProducesResponseType(typeof(TicketDetalleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] TicketEstadoRequest request)
        {
            return Ok(await _service.CambiarEstado(id, request.Estado, UsuarioActual()));
        }

        private UsuarioActual UsuarioActual() => new(
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0,
            User.FindFirstValue("nombreCompleto") ?? User.Identity?.Name ?? "Usuario",
            User.IsInRole(Roles.Soporte));
    }
}

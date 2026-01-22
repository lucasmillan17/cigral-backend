using CigralBackend.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CigralBackend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParserController : ControllerBase
    {

        private readonly IBarCodeParser _barCodeParser;
        public ParserController(IBarCodeParser barCodeParser)
        {
            _barCodeParser = barCodeParser;
        }

        [HttpGet("analyze")]
        public IActionResult Analyze([FromQuery] string rawCode)
        {
            // El controlador NO sabe que es GS1, solo sabe que "parsea".
            var result = _barCodeParser.Parse(rawCode);

            // ... resto de la lógica de búsqueda en BD ...
            return Ok(result);
        }

    }
}

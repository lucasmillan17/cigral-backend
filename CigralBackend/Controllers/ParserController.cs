using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CigralBackend.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ParserController : ControllerBase
    {

        private readonly IBarCodeParser _barCodeParser;
        private readonly IProductoService _productoService;
        public ParserController(IBarCodeParser barCodeParser, IProductoService productoService)
        {
            _barCodeParser = barCodeParser;
            _productoService = productoService;
        }

        [HttpGet("analyze")]
        [ProducesResponseType(typeof(ScanResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Analyze([FromQuery] string rawCode)
        {
            // El controlador NO sabe que es GS1, solo sabe que "parsea".
            var result = _barCodeParser.Parse(rawCode);

            if (!result.EsValido)
                return BadRequest("El código escaneado no contiene un GTIN válido.");

            var filtro = new Application.Dtos.ProductoFilters
            (
                Nombre: null,
                Gtin: result.Gtin,
                CodigoGenerico: null,
                Marca: null,
                PageNumber: 1,
                PageSize: 1
            );

            var busqueda = await _productoService.GetProductoFiltered(filtro);

            var producto = busqueda.Items.FirstOrDefault();

            bool existeProducto = producto != null;
            
            return Ok(new ScanResponseDto
            {
                Gtin = result.Gtin,
                Lote = result.Lote,
                NumeroSerie = result.NumeroSerie,
                FechaVencimiento = result.FechaVencimiento,
                Cantidad = result.Cantidad,
                ExisteProducto = existeProducto,
                ProductoId = producto?.Id,
                NombreProducto = producto?.Nombre,
                InformacionAdicional = result?.InformacionAdicional
            });
        }

    }
}

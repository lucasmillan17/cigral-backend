using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CigralBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        private readonly IProductoService _productoService;
        public ProductsController(IProductoService productoService)
        {
            _productoService = productoService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductoModelRequest producto)
        {
            await _productoService.CreateProducto(producto);
            return Ok(producto);
        }
    }
}

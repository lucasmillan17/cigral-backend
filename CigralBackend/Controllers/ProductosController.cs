using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CigralBackend.Controllers
{
    /// <summary>
    /// Controlador para operaciones CRUD de productos.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoService _productoService;

        public ProductosController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        /// <summary>
        /// Crea un nuevo producto.
        /// </summary>
        /// <param name="producto">Datos del producto a crear</param>
        /// <returns>El producto creado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProductoModelResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProduct([FromBody] ProductoModelRequest producto)
        {
            var createdProduct = await _productoService.CreateProducto(producto);
            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }

        /// <summary>
        /// Obtiene productos filtrados con paginacion.
        /// </summary>
        /// <param name="productoFilters">Filtros de busqueda</param>
        /// <returns>Lista paginada de productos</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ProductoModelResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProducts([FromQuery] ProductoFilters productoFilters)
        {
            var products = await _productoService.GetProductoFiltered(productoFilters);
            return Ok(products);
        }

        /// <summary>
        /// Obtiene un producto por su ID.
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>El producto solicitado</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductoModelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var producto = await _productoService.GetProductoById(id);
            return Ok(producto);
        }

        /// <summary>
        /// Actualiza un producto existente.
        /// </summary>
        /// <param name="id">ID del producto a actualizar</param>
        /// <param name="producto">Nuevos datos del producto</param>
        /// <returns>El producto actualizado</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProductoModelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductoModelUpdateRequest producto)
        {
            var updatedProduct = await _productoService.UpdateProducto(id, producto);
            return Ok(updatedProduct);
        }

        /// <summary>
        /// Elimina un producto.
        /// </summary>
        /// <param name="id">ID del producto a eliminar</param>
        /// <returns>No content si fue exitoso</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _productoService.DeleteProducto(id);
            return NoContent();
        }

        [HttpPost("importar-catalogo/{proveedorId}")]
        public async Task<IActionResult> ImportarCatalogoCsv(int proveedorId, string marca, IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("No se envió ningún archivo.");

            // Aquí llamaríamos al servicio que procesa la lógica
            await _productoService.ImportarDesdeCsvAsync(proveedorId, marca, archivo.OpenReadStream());

            return Ok(new { Mensaje = "Catálogo importado correctamente." });
        }

        [HttpPut("actualizar-gtin/{productoId}")]
        public async Task<IActionResult> ActualizarGtin(int productoId, [FromBody] string nuevoGtin)
        {
            if (string.IsNullOrEmpty(nuevoGtin))
                return BadRequest("El GTIN no puede estar vacío.");
            await _productoService.UpdateGTINProducto(productoId, nuevoGtin);
            return Ok(new { Mensaje = "GTIN actualizado correctamente." });
        }
    }
}

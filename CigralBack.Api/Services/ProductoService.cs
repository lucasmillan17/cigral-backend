using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain;
using CigralBackend.Infraestructure.Database.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IRepository _repository;
        public ProductoService(IRepository productoRepository)
        {
            _repository = productoRepository;
        }

        private ProductoModelResponse ResponseGenerator(Producto p)
        {
            return new ProductoModelResponse(
                p.Id,
                p.Nombre,
                p.Descripcion,
                p.GTIN,
                p.EsUnitario,
                p.Precio
            );
        }
        public async Task CreateProducto(ProductoModelRequest r)
        {

            var existingProducto = await _repository.First<Producto>(p => p.GTIN == r.GTIN);
            if (existingProducto != null)
            {
                throw new InvalidOperationException($"El producto con GTIN {r.GTIN} ya existe.");
            }

            var producto = new Producto()
            {
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                GTIN = r.GTIN,
                EsUnitario = r.EsUnitario,
                Precio = r.Precio
            };
            await _repository.Add<Producto>(producto);
        }

        public async Task<List<ProductoModelResponse>> GetAllProductos(int pageNumber, int pageSize)
        {
            var productos = await _repository.GetAll<Producto>();
            return productos.Items.Select(p => ResponseGenerator(p)).ToList();
        }

        public async Task<List<ProductoModelResponse>> GetProductoFiltered(string nombre, string gtin, int pageNumber, int pageSize)
        {
            var productos = await _repository.GetFiltered<Producto>(p =>
                (string.IsNullOrEmpty(nombre) || p.Nombre.Contains(nombre)) &&
                (string.IsNullOrEmpty(gtin) || p.GTIN.Contains(gtin))
            , pageNumber, pageSize);
            return productos.Items.Select(p => ResponseGenerator(p)).ToList();
        }
    }
}

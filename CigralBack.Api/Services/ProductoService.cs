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

        public async Task CreateProducto(ProductoModelRequest r)
        {
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
    }
}

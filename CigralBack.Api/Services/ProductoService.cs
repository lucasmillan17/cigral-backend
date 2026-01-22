using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain;
using CigralBackend.Domain.Enums;
using CigralBackend.Domain.Exceptions;
using CigralBackend.Domain.Wrappers;
using CigralBackend.Infraestructure.Database.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services
{
    /// <summary>
    /// Servicio de aplicacion para operaciones de productos.
    /// </summary>
    public class ProductoService : IProductoService
    {
        private readonly IRepository _repository;

        public ProductoService(IRepository productoRepository)
        {
            _repository = productoRepository;
        }

        /// <summary>
        /// Mapea un resultado paginado de productos a productos response.
        /// </summary>
        private PagedResult<ProductoModelResponse> MapeoProductosResponse(PagedResult<Producto> productos)
        {
            var productosMapeados = productos.Items.Select(p => ResponseGenerator(p)).ToList();
            return new PagedResult<ProductoModelResponse>
            {
                Items = productosMapeados,
                TotalCount = productos.TotalCount,
                PageNumber = productos.PageNumber,
                PageSize = productos.PageSize
            };
        }

        /// <summary>
        /// Genera un ProductoModelResponse desde una entidad Producto.
        /// </summary>
        private ProductoModelResponse ResponseGenerator(Producto p)
        {
            return new ProductoModelResponse(
                p.Id,
                p.Marca?.Nombre,
                p.Nombre,
                p.Descripcion,
                p.GTIN,
                p.Precio
            );
        }

        /// <summary>
        /// Crea un nuevo producto en el sistema.
        /// </summary>
        /// <param name="r">Datos del producto a crear</param>
        /// <returns>El producto creado</returns>
        /// <exception cref="DomainException">Si el GTIN ya existe</exception>
        public async Task<ProductoModelResponse> CreateProducto(ProductoModelRequest r)
        {
            // Validar que el GTIN no este duplicado
            var existingProducto = await _repository.First<Producto>(p => p.GTIN == r.GTIN);
            if (existingProducto != null)
            {
                throw new DomainException(
                    DomainErrorCode.GtinDuplicado,
                    $"El producto con GTIN {r.GTIN} ya existe."
                );
            }

            // Validar que el nombre no este duplicado
            var productoConMismoNombre = await _repository.First<Producto>(p => p.Nombre == r.Nombre);
            if (productoConMismoNombre != null)
            {
                throw new DomainException(
                    DomainErrorCode.NombreProductoDuplicado,
                    $"Ya existe un producto con el nombre '{r.Nombre}'."
                );
            }

            // Si se especifica una marca, validar que exista
            var marca = (Marca?)null;
            if (!string.IsNullOrEmpty(r.Marca))
            {
                marca = await _repository.First<Marca>(m => m.Nombre == r.Marca);
                if (marca == null)
                {
                    throw new DomainException(
                        DomainErrorCode.MarcaNoValida,
                        $"La marca con nombre {r.Marca} no existe."
                    );
                }
            }

            // Crear el producto
            var producto = new Producto()
            {
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                GTIN = r.GTIN,
                EsUnitario = r.EsUnitario ?? false,
                Precio = r.Precio,
                Marca = marca
            };

            await _repository.Add<Producto>(producto);

            return ResponseGenerator(producto);
        }

        /// <summary>
        /// Obtiene todos los productos con paginacion.
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Tamano de pagina</param>
        /// <returns>Resultado paginado de productos</returns>
        public async Task<PagedResult<ProductoModelResponse>> GetAllProductos(int pageNumber, int pageSize)
        {
            var productos = await _repository.GetAll<Producto>(pageNumber, pageSize, "Marca");

            return MapeoProductosResponse(productos);
        }

        /// <summary>
        /// Obtiene productos filtrados con paginacion.
        /// </summary>
        /// <param name="f">Filtros a aplicar</param>
        /// <returns>Resultado paginado de productos filtrados</returns>
        public async Task<PagedResult<ProductoModelResponse>> GetProductoFiltered(ProductoFilters f)
        {
            var productos = await _repository.GetFiltered<Producto>(p =>
                (string.IsNullOrEmpty(f.Nombre) || p.Nombre.Contains(f.Nombre)) &&
                (string.IsNullOrEmpty(f.Gtin) || p.GTIN.Contains(f.Gtin))
            , f.PageNumber, f.PageSize, "Marca");

            return MapeoProductosResponse(productos);
        }

        /// <summary>
        /// Obtiene un producto por su ID.
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>El producto encontrado</returns>
        /// <exception cref="NotFoundException">Si el producto no existe</exception>
        public async Task<ProductoModelResponse> GetProductoById(int id)
        {
            var producto = await _repository.GetById<Producto>(id, "Marca");

            if (producto == null)
            {
                throw new NotFoundException(nameof(Producto), id);
            }

            return ResponseGenerator(producto);
        }

        /// <summary>
        /// Actualiza un producto existente.
        /// </summary>
        /// <param name="id">ID del producto a actualizar</param>
        /// <param name="r">Nuevos datos del producto</param>
        /// <returns>El producto actualizado</returns>
        /// <exception cref="NotFoundException">Si el producto no existe</exception>
        /// <exception cref="DomainException">Si hay conflictos de negocio</exception>
        public async Task<ProductoModelResponse> UpdateProducto(int id, ProductoModelRequest r)
        {
            // Validar que el producto exista
            var producto = await _repository.GetById<Producto>(id);
            if (producto == null)
            {
                throw new NotFoundException(nameof(Producto), id);
            }

            // Validar que el GTIN no este duplicado en otro producto
            var productoConMismoGtin = await _repository.First<Producto>(
                p => p.GTIN == r.GTIN && p.Id != id
            );
            if (productoConMismoGtin != null)
            {
                throw new DomainException(
                    DomainErrorCode.GtinDuplicado,
                    $"El GTIN {r.GTIN} ya existe en otro producto."
                );
            }

            // Validar que el nombre no este duplicado en otro producto
            var productoConMismoNombre = await _repository.First<Producto>(
                p => p.Nombre == r.Nombre && p.Id != id
            );
            if (productoConMismoNombre != null)
            {
                throw new DomainException(
                    DomainErrorCode.NombreProductoDuplicado,
                    $"Ya existe otro producto con el nombre '{r.Nombre}'."
                );
            }

            // Si se especifica una marca, validar que exista
            if (r.MarcaId.HasValue)
            {
                var marca = await _repository.GetById<Marca>(r.MarcaId.Value);
                if (marca == null)
                {
                    throw new DomainException(
                        DomainErrorCode.MarcaNoValida,
                        $"La marca con ID {r.MarcaId.Value} no existe."
                    );
                }
            }

            // Actualizar los campos
            producto.Nombre = r.Nombre;
            producto.Descripcion = r.Descripcion;
            producto.GTIN = r.GTIN;
            producto.EsUnitario = r.EsUnitario ?? false;
            producto.Precio = r.Precio;
            producto.MarcaId = r.MarcaId;

            await _repository.Update(producto);

            return ResponseGenerator(producto);
        }

        /// <summary>
        /// Elimina un producto del sistema.
        /// </summary>
        /// <param name="id">ID del producto a eliminar</param>
        /// <exception cref="NotFoundException">Si el producto no existe</exception>
        public async Task DeleteProducto(int id)
        {
            var producto = await _repository.GetById<Producto>(id);
            if (producto == null)
            {
                throw new NotFoundException(nameof(Producto), id);
            }

            await _repository.Delete(producto);
        }
    }
}

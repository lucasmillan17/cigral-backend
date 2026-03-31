using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain;
using CigralBackend.Domain.Enums;
using CigralBackend.Domain.Exceptions;
using CigralBackend.Domain.Wrappers;
using CigralBackend.Infraestructure.Database.Interfaces;
using CigralBackend.Infraestructure.Dtos;
using CigralBackend.Infraestructure.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
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
        private readonly IMarcaService _marcaService;
        private readonly ICatalogParserService _catalogParser;

        public ProductoService(IRepository productoRepository, IMarcaService marcaService, ICatalogParserService CatalogParser)
        {
            _repository = productoRepository;
            _marcaService = marcaService;
            _catalogParser = CatalogParser;

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
                p.CodigoGenerico,
                p.CodigoInterno,
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
            int idMarca = 0;
            // Validar que tenga GTIN o CodigoGenerico
            if (string.IsNullOrEmpty(r.GTIN) && string.IsNullOrEmpty(r.CodigoGenerico))
            {
                throw new DomainException(
                    DomainErrorCode.GtinDuplicado,
                    "El producto debe tener al menos un GTIN o un Codigo Generico."
                );
            }
            // Validar que el GTIN o CodigoGenerico no este duplicado
            var existingProducto = await _repository.First<Producto>(p =>
            (string.IsNullOrEmpty(r.GTIN) || p.GTIN == r.GTIN) &&
            (string.IsNullOrEmpty(r.CodigoGenerico) || p.CodigoGenerico == r.CodigoGenerico)
            );

            if (existingProducto != null)
            {
                throw new DomainException(
                    DomainErrorCode.GtinDuplicado,
                    $"El producto con GTIN {r.GTIN}o Codigo Generico {r.CodigoGenerico} o Codigo Interno {r.CodigoInterno} ya existe."
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
                    var MarcaNueva = await _marcaService.CreateMarca(new MarcaRequest(Nombre: r.Marca));
                    idMarca = MarcaNueva.Id;
                }
                else
                {
                    idMarca = marca.Id;
                }
            }

            // Crear el producto
            var producto = new Producto()
            {
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                GTIN = r.GTIN,
                CodigoGenerico = r.CodigoGenerico,
                CodigoInterno = r.CodigoInterno,
                EsUnitario = r.EsUnitario ?? false,
                Precio = r.Precio,
                MarcaId = idMarca
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

        /// <summary>
        /// Obtiene productos filtrados con paginacion.
        /// </summary>
        /// <param name="f">Filtros a aplicar</param>
        /// <returns>Resultado paginado de productos filtrados</returns>
        public async Task<PagedResult<ProductoModelResponse>> GetProductoFiltered(ProductoFilters f)
        {
            var productos = await _repository.GetFiltered<Producto>(p =>
                // ==========================================================
                // BLOQUE 1: BÚSQUEDA GLOBAL (Con OR ||)
                // Si f.BusquedaGlobal está vacío, todo este bloque da 'true' y se ignora.
                // Si tiene texto, exige que coincida con AL MENOS UNA de estas columnas.
                // ==========================================================
                (string.IsNullOrEmpty(f.BusquedaGlobal) ||
                (
                    p.Nombre.Contains(f.BusquedaGlobal) ||
                    (p.CodigoInterno != null && p.CodigoInterno.Contains(f.BusquedaGlobal)) ||
                    (p.GTIN != null && p.GTIN.Contains(f.BusquedaGlobal)) ||
                    (p.Marca != null && p.Marca.Nombre.Contains(f.BusquedaGlobal))
                ))
                && // <--- Unimos el bloque global con los filtros específicos usando AND
                   // ==========================================================
                   // BLOQUE 2: FILTROS ESPECÍFICOS (Con AND &&)
                   // Se evalúan individualmente. Si el frontend no los envía, se ignoran.
                   // ==========================================================
                (string.IsNullOrEmpty(f.Nombre) || p.Nombre.Contains(f.Nombre)) &&
                (string.IsNullOrEmpty(f.Gtin) || (p.GTIN != null && p.GTIN.Contains(f.Gtin))) &&
                (string.IsNullOrEmpty(f.CodigoGenerico) || (p.CodigoGenerico != null && p.CodigoGenerico.Contains(f.CodigoGenerico))) &&
                (string.IsNullOrEmpty(f.CodigoInterno) || (p.CodigoInterno != null && p.CodigoInterno.Contains(f.CodigoInterno))),

                pageNumber: f.PageNumber,
                pageSize: f.PageSize,
                include: new[] { "Marca" }
            );

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
        public async Task<ProductoModelResponse> UpdateProducto(int id, ProductoModelUpdateRequest r)
        {
            int idMarca = 0;
            // Validar que el producto exista
            var producto = await _repository.GetById<Producto>(id);
            if (producto == null)
            {
                throw new NotFoundException(nameof(Producto), id);
            }

            // Validar que el GTIN no este duplicado en otro producto
            var productoConMismoGtin = await _repository.First<Producto>(p =>
                p.Id != id && // Excluimos el producto actual
                (
                    (!string.IsNullOrEmpty(r.GTIN) && p.GTIN == r.GTIN) ||
                    (!string.IsNullOrEmpty(r.CodigoGenerico) && p.CodigoGenerico == r.CodigoGenerico) ||
                    (!string.IsNullOrEmpty(r.CodigoInterno) && p.CodigoInterno == r.CodigoInterno)
                )
            );
            if (productoConMismoGtin != null)
            {
                throw new DomainException(
                    DomainErrorCode.GtinDuplicado,
                    $"El GTIN {r.GTIN} o CodigoGenerico {r.CodigoGenerico} o CodigoInterno {r.CodigoInterno} ya existe en otro producto."
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

            var marca = (Marca?)null;
            // Si se especifica una marca, validar que exista
            if (!string.IsNullOrEmpty(r.Marca))
            {
                marca = await _repository.First<Marca>(p => p.Nombre == r.Marca);
                if (marca == null)
                {
                    var nuevaMarca = new MarcaRequest(Nombre: r.Marca);
                    var Marca = await _marcaService.CreateMarca(nuevaMarca);
                    idMarca = Marca.Id;
                }
            }

            // Actualizar los campos
            producto.Nombre = r.Nombre;
            producto.Descripcion = r.Descripcion;
            producto.GTIN = r.GTIN;
            producto.CodigoGenerico = r.CodigoGenerico;
            producto.CodigoInterno = r.CodigoInterno;
            producto.Precio = r.Precio;
            producto.MarcaId = marca != null ? marca.Id : idMarca;

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

        public async Task ImportarDesdeCsvAsync(int proveedorId, string marcaNombre, Stream archivoStream)
        {
            // 1. Validar proveedor (Podrías omitir este paso si ya no vas a vincular el ID en ninguna tabla, 
            // pero lo dejamos por si a futuro decides registrar en una bitácora quién fue el proveedor del archivo).
            var proveedor = await _repository.GetById<Proveedor>(proveedorId);
            if (proveedor == null) throw new NotFoundException(nameof(Proveedor), proveedorId);

            // 2. Delegar la lectura del archivo a la Infraestructura
            var registros = _catalogParser.ParsearCatalogo(archivoStream);

            // 3. Comenzar transacción (Usamos _context directamente para mantener la consistencia con SaveChanges)
            using var transaction = await _repository.BeginTransaction();

            try
            {
                foreach (var fila in registros)
                {
                    int idMarca = 0;
                    var nombreLimpio = fila.Denominacion.Trim();
                    var codigoRef = fila.Codigo.Trim();

                    // Verificamos la marca, y la creamos si no existe
                    var marca = (Marca?)null;
                    if (!string.IsNullOrEmpty(marcaNombre))
                    {
                        marca = await _repository.First<Marca>(m => m.Nombre == marcaNombre);
                        if (marca == null)
                        {
                            var MarcaNueva = await _marcaService.CreateMarca(new MarcaRequest(Nombre: marcaNombre));
                            idMarca = MarcaNueva.Id;
                        }
                        else
                        {
                            idMarca = marca.Id;
                        }
                    }

                    // Buscamos si el producto ya existe por Nombre O por CodigoInterno
                    var productoExistente = await _repository.First<Producto>(p =>
                        p.Nombre == nombreLimpio || p.CodigoInterno == codigoRef);

                    if (productoExistente == null)
                    {
                        // Si no existe, lo creamos con el código del Excel como CodigoInterno
                        productoExistente = new Producto
                        {
                            Nombre = nombreLimpio,
                            CodigoInterno = codigoRef,
                            MarcaId = idMarca,
                            Descripcion = "",
                            Activo = true
                        };
                        await _repository.Add(productoExistente);
                    }
                    else
                    {
                        // Opcional: Si el producto ya existía por nombre, pero no tenía código interno o marca, se lo actualizamos
                        if (string.IsNullOrEmpty(productoExistente.CodigoInterno) || productoExistente.Marca == null)
                        {
                            productoExistente.CodigoInterno = codigoRef;
                            productoExistente.MarcaId = idMarca; // También podríamos actualizar la marca si se especificó
                            await _repository.Update(productoExistente);
                        }
                    }
                }

                // Confirmamos la transacción
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // Relanzamos para que el ExceptionHandlingMiddleware lo atrape
            }
        }

        public async Task<ProductoModelResponse> UpdateGTINProducto(int idProducto, string nuevoGTIN)
        {
            var GtinExistente = await _repository.First<Producto>(p => p.GTIN == nuevoGTIN && p.Id != idProducto);
            if (GtinExistente != null) throw new DomainException(
                DomainErrorCode.GtinDuplicado,
                $"El GTIN {nuevoGTIN} ya existe en otro producto."
            );

            var producto = await _repository.GetById<Producto>(idProducto);
            if (producto == null) throw new NotFoundException(nameof(Producto), idProducto);
            producto.GTIN = nuevoGTIN;
            await _repository.Update(producto);

            return ResponseGenerator(producto);

        }
    }
}

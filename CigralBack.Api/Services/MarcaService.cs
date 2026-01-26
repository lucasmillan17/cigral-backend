using CigralBackend.Infraestructure.Database.Interfaces;
using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain;
using CigralBackend.Domain.Enums;
using CigralBackend.Domain.Exceptions;
using CigralBackend.Domain.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services
{
    /// <summary>
    /// Servicio de aplicacion para operaciones de marcas.
    /// </summary>
    public class MarcaService : IMarcaService
    {
        private readonly IRepository _repository;

        public MarcaService(IRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Genera un MarcaResponse desde una entidad Marca.
        /// </summary>
        private MarcaResponse ResponseGenerator(Marca marca)
        {
            return new MarcaResponse(
                marca.Id,
                marca.Nombre
            );
        }

        /// <summary>
        /// Obtiene todas las marcas del sistema.
        /// </summary>
        /// <returns>Lista de marcas</returns>
        public async Task<List<MarcaResponse>> GetMarcasAsync()
        {
            var marcas = await _repository.GetAll<Marca>();
            return marcas.Items.Select(p => ResponseGenerator(p)).ToList();
        }

        /// <summary>
        /// Obtiene marcas filtradas por nombre.
        /// </summary>
        /// <param name="nombre">Nombre o parte del nombre a buscar</param>
        /// <returns>Lista de marcas que coinciden</returns>
        public async Task<List<MarcaResponse>> GetMarcasByNombre(string nombre)
        {
            var marcas = await _repository.GetFiltered<Marca>(m => m.Nombre.Contains(nombre), 1, 50);
            return marcas.Items.Select(p => ResponseGenerator(p)).ToList();
        }

        /// <summary>
        /// Crea una nueva marca en el sistema.
        /// </summary>
        /// <param name="r">Datos de la marca a crear</param>
        /// <returns>La marca creada</returns>
        /// <exception cref="DomainException">Si el nombre ya existe</exception>
        public async Task<MarcaResponse> CreateMarca(MarcaRequest r)
        {
            // Validar que la marca no exista
            var existingMarca = await _repository.First<Marca>(m => m.Nombre == r.Nombre);
            if (existingMarca != null)
            {
                throw new DomainException(
                    DomainErrorCode.MarcaDuplicada,
                    $"Ya existe una marca con el nombre '{r.Nombre}'."
                );
            }

            var nuevaMarca = new Marca
            {
                Nombre = r.Nombre
            };

            var createdMarca = await _repository.Add<Marca>(nuevaMarca);
            return ResponseGenerator(createdMarca);
        }

        /// <summary>
        /// Obtiene una marca por su ID.
        /// </summary>
        /// <param name="id">ID de la marca</param>
        /// <returns>La marca encontrada</returns>
        /// <exception cref="NotFoundException">Si la marca no existe</exception>
        public async Task<MarcaResponse> GetMarcaById(int id)
        {
            var marca = await _repository.GetById<Marca>(id);
            
            if (marca == null)
            {
                throw new NotFoundException(nameof(Marca), id);
            }

            return ResponseGenerator(marca);
        }

        /// <summary>
        /// Actualiza una marca existente.
        /// </summary>
        /// <param name="id">ID de la marca a actualizar</param>
        /// <param name="r">Nuevos datos de la marca</param>
        /// <returns>La marca actualizada</returns>
        /// <exception cref="NotFoundException">Si la marca no existe</exception>
        /// <exception cref="DomainException">Si el nuevo nombre ya existe en otra marca</exception>
        public async Task<MarcaResponse> UpdateMarca(int id, MarcaRequest r)
        {
            // Validar que la marca exista
            var marca = await _repository.GetById<Marca>(id);
            if (marca == null)
            {
                throw new NotFoundException(nameof(Marca), id);
            }

            // Validar que el nombre no esté duplicado en otra marca
            var marcaConMismoNombre = await _repository.First<Marca>(
                m => m.Nombre == r.Nombre && m.Id != id
            );
            if (marcaConMismoNombre != null)
            {
                throw new DomainException(
                    DomainErrorCode.MarcaDuplicada,
                    $"Ya existe otra marca con el nombre '{r.Nombre}'."
                );
            }

            // Actualizar el nombre
            marca.Nombre = r.Nombre;
            await _repository.Update(marca);

            return ResponseGenerator(marca);
        }

        /// <summary>
        /// Elimina una marca del sistema.
        /// </summary>
        /// <param name="id">ID de la marca a eliminar</param>
        /// <exception cref="NotFoundException">Si la marca no existe</exception>
        /// <exception cref="DomainException">Si la marca tiene productos asociados</exception>
        public async Task DeleteMarca(int id)
        {
            var marca = await _repository.GetById<Marca>(id);
            if (marca == null)
            {
                throw new NotFoundException(nameof(Marca), id);
            }

            // Validar que no tenga productos asociados
            var productosConMarca = await _repository.First<Producto>(p => p.MarcaId == id);
            if (productosConMarca != null)
            {
                throw new DomainException(
                    DomainErrorCode.MarcaTieneProductos,
                    $"No se puede eliminar la marca '{marca.Nombre}' porque tiene productos asociados."
                );
            }

            await _repository.Delete(marca);
        }
    }
}

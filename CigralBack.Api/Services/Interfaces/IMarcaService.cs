using CigralBackend.Application.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de marcas.
    /// </summary>
    public interface IMarcaService
    {
        /// <summary>
        /// Obtiene todas las marcas del sistema.
        /// </summary>
        /// <returns>Lista de marcas</returns>
        Task<List<MarcaResponse>> GetMarcasAsync();

        /// <summary>
        /// Obtiene marcas filtradas por nombre.
        /// </summary>
        /// <param name="nombre">Nombre o parte del nombre a buscar</param>
        /// <returns>Lista de marcas que coinciden</returns>
        Task<List<MarcaResponse>> GetMarcasByNombre(string nombre);

        /// <summary>
        /// Obtiene una marca por su ID.
        /// </summary>
        /// <param name="id">ID de la marca</param>
        /// <returns>La marca encontrada</returns>
        Task<MarcaResponse> GetMarcaById(int id);

        /// <summary>
        /// Crea una nueva marca en el sistema.
        /// </summary>
        /// <param name="r">Datos de la marca a crear</param>
        /// <returns>La marca creada</returns>
        Task<MarcaResponse> CreateMarca(MarcaRequest r);

        /// <summary>
        /// Actualiza una marca existente.
        /// </summary>
        /// <param name="id">ID de la marca a actualizar</param>
        /// <param name="r">Nuevos datos de la marca</param>
        /// <returns>La marca actualizada</returns>
        Task<MarcaResponse> UpdateMarca(int id, MarcaRequest r);

        /// <summary>
        /// Elimina una marca del sistema.
        /// </summary>
        /// <param name="id">ID de la marca a eliminar</param>
        Task DeleteMarca(int id);
    }
}

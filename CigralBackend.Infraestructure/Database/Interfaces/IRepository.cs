using CigralBackend.Domain.Bases;
using System.Linq.Expressions;

namespace CigralBackend.Infraestructure.Database.Interfaces
{
    /// <summary>
    /// Interfaz genérica para el repositorio que proporciona operaciones CRUD y consultas con paginación.
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Agrega una nueva entidad a la base de datos.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad que hereda de EntityBase</typeparam>
        /// <param name="entity">La entidad a agregar</param>
        /// <returns>La entidad agregada con su Id generado</returns>
        Task<T> Add<T>(T entity) where T : EntityBase;

        /// <summary>
        /// Actualiza una entidad existente en la base de datos.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad que hereda de EntityBase</typeparam>
        /// <param name="entity">La entidad con los cambios a aplicar</param>
        /// <returns>La entidad actualizada</returns>
        Task<T> Update<T>(T entity) where T : EntityBase;

        /// <summary>
        /// Elimina una entidad de la base de datos.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad que hereda de EntityBase</typeparam>
        /// <param name="entity">La entidad a eliminar</param>
        /// <returns>La entidad eliminada</returns>
        Task<T> Delete<T>(T entity) where T : EntityBase;

        /// <summary>
        /// Obtiene una entidad por su identificador único.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad que hereda de EntityBase</typeparam>
        /// <param name="id">El identificador único de la entidad</param>
        /// <param name="include">Propiedades de navegación a incluir (eager loading)</param>
        /// <returns>La entidad encontrada o null si no existe</returns>
        Task<T?> GetById<T>(Guid id, params string[] include) where T : EntityBase;

        /// <summary>
        /// Obtiene la primera entidad que cumple con el predicado especificado.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad que hereda de EntityBase</typeparam>
        /// <param name="predicate">Expresión lambda que define el criterio de búsqueda</param>
        /// <param name="include">Propiedades de navegación a incluir (eager loading)</param>
        /// <returns>La primera entidad que cumple la condición o null si no existe</returns>
        Task<T?> First<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase;

        /// <summary>
        /// Obtiene todas las entidades con soporte de paginación.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad que hereda de EntityBase</typeparam>
        /// <param name="pageNumber">Número de página (inicia en 1)</param>
        /// <param name="pageSize">Cantidad de elementos por página</param>
        /// <param name="include">Propiedades de navegación a incluir (eager loading)</param>
        /// <returns>Resultado paginado con los elementos y metadata de paginación</returns>
        Task<PagedResult<T>> GetAll<T>(int pageNumber = 1, int pageSize = 10, params string[] include) where T : EntityBase;

        /// <summary>
        /// Obtiene entidades filtradas con soporte de paginación.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad que hereda de EntityBase</typeparam>
        /// <param name="predicate">Expresión lambda que define el criterio de filtrado</param>
        /// <param name="pageNumber">Número de página (inicia en 1)</param>
        /// <param name="pageSize">Cantidad de elementos por página</param>
        /// <param name="include">Propiedades de navegación a incluir (eager loading)</param>
        /// <returns>Resultado paginado con los elementos filtrados y metadata de paginación</returns>
        Task<PagedResult<T>> GetFiltered<T>(Expression<Func<T, bool>> predicate, int pageNumber = 1, int pageSize = 10, params string[] include) where T : EntityBase;
    }

    /// <summary>
    /// Clase que encapsula el resultado de una consulta paginada.
    /// </summary>
    /// <typeparam name="T">Tipo de los elementos en el resultado</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Lista de elementos de la página actual.
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        /// Cantidad total de elementos en la consulta (sin paginación).
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Número de la página actual (inicia en 1).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Cantidad de elementos por página.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Cantidad total de páginas disponibles.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>
        /// Indica si existe una página anterior.
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Indica si existe una página siguiente.
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Constructor que inicializa la lista de elementos vacía.
        /// </summary>
        public PagedResult()
        {
            Items = new List<T>();
        }
    }
}

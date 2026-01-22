using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Domain.Wrappers
{
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

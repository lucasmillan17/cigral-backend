using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Domain.Bases
{
    /// <summary>
    /// Clase base abstracta para todas las entidades del dominio.
    /// Proporciona un identificador único (GUID) común para todas las entidades.
    /// </summary>
    public abstract class EntityBase
    {
        /// <summary>
        /// Constructor protegido para la clase base.
        /// </summary>
        protected EntityBase()
        {
        }

        /// <summary>
        /// Identificador único de la entidad (GUID).
        /// Se genera automáticamente al crear una nueva entidad.
        /// </summary>
        public Guid Id { get; set; }
    }
}

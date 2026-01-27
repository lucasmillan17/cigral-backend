using Microsoft.AspNetCore.Identity;
using System;

namespace CigralBackend.Domain
{
    /// <summary>
    /// Usuario de la aplicación extendiendo IdentityUser.
    /// </summary>
    public class ApplicationUser : IdentityUser<int>
    {
        /// <summary>
        /// Nombre completo del usuario.
        /// </summary>
        public string? NombreCompleto { get; set; }

        /// <summary>
        /// Indica si el usuario es administrador.
        /// </summary>
        public bool EsAdmin { get; set; }

        /// <summary>
        /// Fecha de creación del usuario.
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Última fecha de login.
        /// </summary>
        public DateTime? UltimoLogin { get; set; }
    }
}

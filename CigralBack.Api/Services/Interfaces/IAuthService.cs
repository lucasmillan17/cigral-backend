using CigralBackend.Application.Dtos;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de autenticación.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Autentica un usuario y genera un token JWT.
        /// </summary>
        Task<AuthResponse> Login(LoginRequest request);

        /// <summary>
        /// Registra un nuevo usuario (solo admin).
        /// </summary>
        Task<UsuarioResponse> Register(RegisterRequest request, string adminUsername);

        /// <summary>
        /// Valida si un usuario es administrador.
        /// </summary>
        Task<bool> IsAdmin(string username);
    }
}

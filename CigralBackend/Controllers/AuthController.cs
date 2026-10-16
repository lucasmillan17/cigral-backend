using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CigralBackend.Controllers
{
    /// <summary>
    /// Controlador de autenticación.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Login de usuario. Genera un token JWT.
        /// </summary>
        /// <param name="request">Credenciales de login</param>
        /// <returns>Token JWT y datos del usuario</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.Login(request);
            return Ok(response);
        }

        /// <summary>
        /// Registro de nuevo usuario. Solo accesible por administradores.
        /// </summary>
        /// <param name="request">Datos del nuevo usuario</param>
        /// <returns>Datos del usuario creado</returns>
        [HttpPost("register")]
        [Authorize]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Obtener username del usuario autenticado
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("Token inválido");
            }

            var usuario = await _authService.Register(request, username);
            return CreatedAtAction(nameof(Register), new { id = usuario.Id }, usuario);
        }

        /// <summary>
        /// Cambiar contraseña del usuario autenticado.
        /// Comprobacion: la nueva contraseña no puede ser igual a la actual.
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
                return Unauthorized("Token inválido");

            await _authService.ChangePassword(username, request.CurrentPassword, request.NewPassword);
            return NoContent();
        }

        /// <summary>
        /// Reemplaza los roles de un usuario (Administrador, Soporte). Solo Soporte.
        /// Los cambios se ven en el próximo inicio de sesión del usuario.
        /// </summary>
        [HttpPost("usuarios/{id:int}/roles")]
        [Authorize(Roles = CigralBackend.Domain.Roles.Soporte)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AsignarRoles(int id, [FromBody] RolesUsuarioRequest request)
        {
            return Ok(await _authService.AsignarRoles(id, request.Roles));
        }
    }
}

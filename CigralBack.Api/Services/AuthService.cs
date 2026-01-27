using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain;
using CigralBackend.Domain.Enums;
using CigralBackend.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services
{
    /// <summary>
    /// Servicio de autenticación con ASP.NET Core Identity y JWT.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Autentica un usuario y genera un token JWT.
        /// </summary>
        public async Task<AuthResponse> Login(LoginRequest request)
        {
            // Buscar usuario por username
            var usuario = await _userManager.FindByNameAsync(request.Username);

            if (usuario == null)
            {
                throw new DomainException(
                    DomainErrorCode.CredencialesInvalidas,
                    "Usuario o contraseña incorrectos."
                );
            }

            // Verificar contraseña con Identity
            var result = await _signInManager.CheckPasswordSignInAsync(usuario, request.Password, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                throw new DomainException(
                    DomainErrorCode.CredencialesInvalidas,
                    "Usuario o contraseña incorrectos."
                );
            }

            // Verificar que el usuario esté activo (usando EmailConfirmed como flag activo)
            if (!usuario.EmailConfirmed)
            {
                throw new DomainException(
                    DomainErrorCode.UsuarioInactivo,
                    "El usuario está inactivo. Contacte al administrador."
                );
            }

            // Actualizar último login
            usuario.UltimoLogin = DateTime.Now;
            await _userManager.UpdateAsync(usuario);

            // Generar token JWT
            var token = await GenerateJwtToken(usuario);
            var expiracion = DateTime.Now.AddHours(8);

            return new AuthResponse(
                Token: token,
                Username: usuario.UserName!,
                NombreCompleto: usuario.NombreCompleto,
                EsAdmin: usuario.EsAdmin,
                Expiracion: expiracion
            );
        }

        /// <summary>
        /// Registra un nuevo usuario (solo admin).
        /// </summary>
        public async Task<UsuarioResponse> Register(RegisterRequest request, string adminUsername)
        {
            // Validar que quien registra sea admin
            var admin = await _userManager.FindByNameAsync(adminUsername);
            if (admin == null || !admin.EsAdmin)
            {
                throw new DomainException(
                    DomainErrorCode.PermisosDenegados,
                    "Solo los administradores pueden registrar nuevos usuarios."
                );
            }

            // Validar que el username no exista
            var existeUsuario = await _userManager.FindByNameAsync(request.Username);
            if (existeUsuario != null)
            {
                throw new DomainException(
                    DomainErrorCode.UsernameDeplicado,
                    $"El username '{request.Username}' ya existe."
                );
            }

            // Crear usuario
            var usuario = new ApplicationUser
            {
                UserName = request.Username,
                NombreCompleto = request.NombreCompleto,
                Email = request.Email ?? $"{request.Username}@cigral.local",
                EmailConfirmed = true, // Usar como flag "Activo"
                EsAdmin = request.EsAdmin,
                FechaCreacion = DateTime.Now
            };

            // Crear usuario con Identity (hashea la contraseña automáticamente)
            var result = await _userManager.CreateAsync(usuario, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new DomainException(
                    DomainErrorCode.UnknownError,
                    $"Error al crear usuario: {errors}"
                );
            }

            return new UsuarioResponse(
                Id: usuario.Id,
                Username: usuario.UserName!,
                NombreCompleto: usuario.NombreCompleto,
                Email: usuario.Email,
                EsAdmin: usuario.EsAdmin,
                Activo: usuario.EmailConfirmed, // EmailConfirmed = Activo
                FechaCreacion: usuario.FechaCreacion,
                UltimoLogin: usuario.UltimoLogin
            );
        }

        /// <summary>
        /// Valida si un usuario es administrador.
        /// </summary>
        public async Task<bool> IsAdmin(string username)
        {
            var usuario = await _userManager.FindByNameAsync(username);
            return usuario != null && usuario.EsAdmin;
        }

        /// <summary>
        /// Genera un token JWT para el usuario.
        /// </summary>
        private async Task<string> GenerateJwtToken(ApplicationUser usuario)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key no configurada");
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "CigralBackend";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "CigralBackend";

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.UserName!),
                new Claim(ClaimTypes.Email, usuario.Email ?? ""),
                new Claim("esAdmin", usuario.EsAdmin.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

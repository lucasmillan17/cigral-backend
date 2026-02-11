using System;
using System.ComponentModel.DataAnnotations;

namespace CigralBackend.Application.Dtos
{
    /// <summary>
    /// Request para login de usuario.
    /// </summary>
    public record LoginRequest
    (
        [Required(ErrorMessage = "El username es obligatorio")]
        string Username,

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        string Password
    );

    /// <summary>
    /// Request para registrar un nuevo usuario (solo admin).
    /// </summary>
    public record RegisterRequest
    (
        [Required(ErrorMessage = "El username es obligatorio")]
        [MaxLength(50, ErrorMessage = "El username no puede superar los 50 caracteres")]
        string Username,

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        string Password,

        [MaxLength(200, ErrorMessage = "El nombre completo no puede superar los 200 caracteres")]
        string? NombreCompleto,

        [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
        [MaxLength(100, ErrorMessage = "El email no puede superar los 100 caracteres")]
        string? Email,

        bool EsAdmin = false
    );

    /// <summary>
    /// Request para cambiar contraseña.
    /// </summary>
    public record ChangePasswordRequest
    (
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        string CurrentPassword,

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres")]
        string NewPassword
    );

    /// <summary>
    /// Respuesta de autenticación exitosa.
    /// </summary>
    public record AuthResponse
    (
        string Token,
        string Username,
        string? NombreCompleto,
        bool EsAdmin,
        DateTime Expiracion
    );

    /// <summary>
    /// Respuesta con información del usuario.
    /// </summary>
    public record UsuarioResponse
    (
        int Id,
        string Username,
        string? NombreCompleto,
        string? Email,
        bool EsAdmin,
        bool Activo,
        DateTime FechaCreacion,
        DateTime? UltimoLogin
    );
}

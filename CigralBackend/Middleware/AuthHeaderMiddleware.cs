using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CigralBackend.Middleware
{
    /// <summary>
    /// Middleware que exige el header "x-cigral-auth" en todas las peticiones.
    /// Reemplaza la regla que antes aplicaba el proxy de Cloudflare.
    /// La clave se lee de la configuracion "AuthHeader:Key" (variable de entorno AuthHeader__Key).
    /// Si no esta configurada, el middleware no exige nada (util en desarrollo).
    /// </summary>
    public class AuthHeaderMiddleware
    {
        public const string HeaderName = "x-cigral-auth";

        private readonly RequestDelegate _next;
        private readonly ILogger<AuthHeaderMiddleware> _logger;
        private readonly byte[]? _expectedKey;

        public AuthHeaderMiddleware(RequestDelegate next, ILogger<AuthHeaderMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;

            var key = configuration["AuthHeader:Key"];
            _expectedKey = string.IsNullOrWhiteSpace(key) ? null : Encoding.UTF8.GetBytes(key);

            if (_expectedKey == null)
            {
                _logger.LogWarning("AuthHeader:Key no esta configurada: no se valida el header {Header}.", HeaderName);
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Sin clave configurada o preflight de CORS (el navegador no envia headers propios en OPTIONS)
            if (_expectedKey == null || HttpMethods.IsOptions(context.Request.Method))
            {
                await _next(context);
                return;
            }

            var received = context.Request.Headers[HeaderName].ToString();

            // Comparacion en tiempo constante para no filtrar la clave por diferencias de tiempo
            if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(received), _expectedKey))
            {
                _logger.LogWarning(
                    "Peticion rechazada sin header {Header} valido: {Method} {Path} desde {Ip}",
                    HeaderName,
                    context.Request.Method,
                    context.Request.Path,
                    context.Connection.RemoteIpAddress
                );

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.ContentType = "application/json";

                var response = new ErrorResponse
                {
                    Error = "Unauthorized",
                    Message = "Cliente no autorizado.",
                    StatusCode = context.Response.StatusCode,
                    Timestamp = DateTime.UtcNow
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
                return;
            }

            await _next(context);
        }
    }
}

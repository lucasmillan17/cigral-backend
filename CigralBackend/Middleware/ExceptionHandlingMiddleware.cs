using CigralBackend.Domain.Enums;
using CigralBackend.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace CigralBackend.Middleware
{
    /// <summary>
    /// Middleware para manejo global de excepciones.
    /// Captura todas las excepciones no manejadas y las convierte en respuestas HTTP apropiadas.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invoca el siguiente middleware y captura excepciones.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Maneja la excepcion y genera la respuesta HTTP apropiada.
        /// </summary>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                NotFoundException notFoundEx => CreateNotFoundResponse(context, notFoundEx),
                DomainException domainEx => CreateDomainErrorResponse(context, domainEx),
                _ => CreateInternalServerErrorResponse(context, exception)
            };

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        /// <summary>
        /// Crea una respuesta para NotFoundException (404).
        /// </summary>
        private ErrorResponse CreateNotFoundResponse(HttpContext context, NotFoundException exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;

            _logger.LogWarning(
                "Entidad no encontrada: {EntityName} con clave {Key}",
                exception.EntityName,
                exception.Key
            );

            return new ErrorResponse
            {
                Error = "NotFound",
                Message = exception.Message,
                StatusCode = context.Response.StatusCode,
                Timestamp = DateTime.UtcNow,
                Details = new Dictionary<string, object>
                {
                    { "entityName", exception.EntityName },
                    { "key", exception.Key }
                }
            };
        }

        /// <summary>
        /// Crea una respuesta para DomainException (400).
        /// </summary>
        private ErrorResponse CreateDomainErrorResponse(HttpContext context, DomainException exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            _logger.LogWarning(
                "Error de dominio: {Code} - {Message}",
                exception.Code,
                exception.Message
            );

            return new ErrorResponse
            {
                Error = "DomainError",
                Message = exception.Message,
                StatusCode = context.Response.StatusCode,
                Timestamp = DateTime.UtcNow,
                Details = new Dictionary<string, object>
                {
                    { "code", exception.Code.ToString() },
                    { "codeValue", (int)exception.Code }
                }
            };
        }

        /// <summary>
        /// Crea una respuesta para excepciones no controladas (500).
        /// </summary>
        private ErrorResponse CreateInternalServerErrorResponse(HttpContext context, Exception exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            _logger.LogError(
                exception,
                "Error interno del servidor: {Message}",
                exception.Message
            );

            // En produccion, no exponer detalles del error
            var isDevelopment = context.RequestServices
                .GetRequiredService<IWebHostEnvironment>()
                .IsDevelopment();

            return new ErrorResponse
            {
                Error = "InternalServerError",
                Message = isDevelopment
                    ? exception.Message
                    : "Ocurrio un error inesperado. Por favor, contacte al administrador.",
                StatusCode = context.Response.StatusCode,
                Timestamp = DateTime.UtcNow,
                Details = isDevelopment
                    ? new Dictionary<string, object>
                    {
                        { "stackTrace", exception.StackTrace ?? string.Empty },
                        { "type", exception.GetType().Name }
                    }
                    : null
            };
        }
    }

    /// <summary>
    /// Modelo de respuesta de error estandarizado.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Tipo de error (NotFound, DomainError, InternalServerError).
        /// </summary>
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// Mensaje descriptivo del error.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Codigo de estado HTTP.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Timestamp del error en UTC.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Detalles adicionales del error (opcional).
        /// </summary>
        public Dictionary<string, object>? Details { get; set; }
    }
}

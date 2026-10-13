namespace CigralBackend.Middleware
{
    /// <summary>
    /// Metodos de extension para registrar middlewares personalizados.
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Registra el middleware de manejo global de excepciones.
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <returns>Application builder para encadenamiento</returns>
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }

        /// <summary>
        /// Registra el middleware que exige el header x-cigral-auth.
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <returns>Application builder para encadenamiento</returns>
        public static IApplicationBuilder UseAuthHeader(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuthHeaderMiddleware>();
        }
    }
}

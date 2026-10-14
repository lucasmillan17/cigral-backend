namespace CigralBackend.Infraestructure.Database
{
    /// <summary>
    /// Búsqueda de texto para usar dentro de consultas LINQ de Entity Framework.
    /// </summary>
    public static class Busqueda
    {
        /// <summary>
        /// Se traduce a la función SQL <c>coincide_busqueda</c> (migración BusquedaPorPalabras):
        /// separa <paramref name="texto"/> en palabras y exige que todas aparezcan en
        /// <paramref name="campo"/>, en cualquier orden y sin distinguir mayúsculas ni acentos.
        /// Si el texto está vacío, coincide siempre.
        /// Ejemplo: "resolute integrity 18" encuentra "RESOLUTE INTEGRITY RSINT22518X 2.25 x 18".
        /// </summary>
        /// <remarks>Solo se puede usar dentro de expresiones que EF traduce a SQL.</remarks>
        public static bool Coincide(string? campo, string? texto)
            => throw new NotSupportedException("Busqueda.Coincide solo se puede usar en consultas de Entity Framework.");
    }
}

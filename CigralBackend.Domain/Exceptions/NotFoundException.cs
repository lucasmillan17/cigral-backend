namespace CigralBackend.Domain.Exceptions
{
    /// <summary>
    /// Excepcion que se lanza cuando una entidad no es encontrada en la base de datos.
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Nombre de la entidad que no fue encontrada.
        /// </summary>
        public string EntityName { get; }

        /// <summary>
        /// Clave o identificador de la entidad que no fue encontrada.
        /// </summary>
        public object Key { get; }

        /// <summary>
        /// Constructor que crea una excepcion con un mensaje automatico.
        /// </summary>
        /// <param name="name">Nombre de la entidad (ej: "Producto", "Cliente")</param>
        /// <param name="key">Identificador de la entidad (ej: ID)</param>
        public NotFoundException(string name, object key)
            : base($"La entidad {name} ({key}) no fue encontrada.")
        {
            EntityName = name;
            Key = key;
        }

        /// <summary>
        /// Constructor que permite especificar un mensaje personalizado.
        /// </summary>
        /// <param name="name">Nombre de la entidad</param>
        /// <param name="key">Identificador de la entidad</param>
        /// <param name="message">Mensaje personalizado</param>
        public NotFoundException(string name, object key, string message)
            : base(message)
        {
            EntityName = name;
            Key = key;
        }
    }
}

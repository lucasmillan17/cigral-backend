using CigralBackend.Domain.Enums;

namespace CigralBackend.Domain.Exceptions
{
    /// <summary>
    /// Excepcion base para errores de dominio y reglas de negocio.
    /// </summary>
    public class DomainException : Exception
    {
        /// <summary>
        /// Codigo de error de dominio que identifica el tipo especifico de error.
        /// </summary>
        public DomainErrorCode Code { get; }

        /// <summary>
        /// Constructor que crea una excepcion de dominio con un codigo de error.
        /// </summary>
        /// <param name="code">Codigo de error de dominio</param>
        public DomainException(DomainErrorCode code)
            : base(GetDefaultMessage(code))
        {
            Code = code;
        }

        /// <summary>
        /// Constructor que crea una excepcion de dominio con un codigo y mensaje personalizado.
        /// </summary>
        /// <param name="code">Codigo de error de dominio</param>
        /// <param name="message">Mensaje descriptivo del error</param>
        public DomainException(DomainErrorCode code, string message)
            : base(message)
        {
            Code = code;
        }

        /// <summary>
        /// Constructor que permite incluir una excepcion interna.
        /// </summary>
        /// <param name="code">Codigo de error de dominio</param>
        /// <param name="message">Mensaje descriptivo del error</param>
        /// <param name="innerException">Excepcion interna que causo este error</param>
        public DomainException(DomainErrorCode code, string message, Exception innerException)
            : base(message, innerException)
        {
            Code = code;
        }

        /// <summary>
        /// Obtiene un mensaje por defecto basado en el codigo de error.
        /// </summary>
        /// <param name="code">Codigo de error</param>
        /// <returns>Mensaje descriptivo del error</returns>
        private static string GetDefaultMessage(DomainErrorCode code)
        {
            return code switch
            {
                DomainErrorCode.UnknownError => "Ocurrio un error desconocido.",
                DomainErrorCode.NetworkError => "Error de conexion de red.",
                
                DomainErrorCode.ProductoNoExiste => "El producto especificado no existe.",
                DomainErrorCode.GtinDuplicado => "El GTIN ya existe en otro producto.",
                DomainErrorCode.MarcaNoValida => "La marca especificada no es valida.",
                DomainErrorCode.NombreProductoDuplicado => "El nombre del producto ya existe.",
                
                DomainErrorCode.StockInsuficiente => "No hay suficiente stock disponible.",
                DomainErrorCode.LoteVencido => "El lote ha superado su fecha de vencimiento.",
                DomainErrorCode.DepositoNoEncontrado => "El deposito especificado no fue encontrado.",
                DomainErrorCode.SerieDuplicada => "El numero de serie ya existe.",
                DomainErrorCode.LoteNoEncontrado => "El lote especificado no existe.",
                DomainErrorCode.ExistenciaNoEncontrada => "La existencia no fue encontrada.",
                DomainErrorCode.StockEnConsignacion => "La existencia no fue encontrada.",

                DomainErrorCode.ClienteNoExiste => "El cliente especificado no existe.",
                DomainErrorCode.GlnClienteDuplicado => "El GLN del cliente ya existe.",
                DomainErrorCode.CuitClienteDuplicado => "El CUIT del cliente ya existe.",
                
                DomainErrorCode.ProveedorNoExiste => "El proveedor especificado no existe.",
                DomainErrorCode.GlnProveedorDuplicado => "El GLN del proveedor ya existe.",
                DomainErrorCode.CuitProveedorDuplicado => "El CUIT del proveedor ya existe.",
                
                DomainErrorCode.RemitoNoExiste => "El remito especificado no existe.",
                DomainErrorCode.NumeroRemitoDuplicado => "El numero de remito ya existe.",
                DomainErrorCode.RemitoSinDetalles => "El remito debe tener al menos un detalle.",
                DomainErrorCode.CantidadInvalida => "La cantidad especificada es invalida.",
                
                _ => "Error de dominio no especificado."
            };
        }
    }
}

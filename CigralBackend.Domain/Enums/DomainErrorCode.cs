namespace CigralBackend.Domain.Enums
{
    /// <summary>
    /// Codigos de error de dominio organizados por rangos numericos.
    /// </summary>
    public enum DomainErrorCode
    {
        // 1000 - Errores Generales
        /// <summary>
        /// Error desconocido o no especificado.
        /// </summary>
        UnknownError = 1000,

        /// <summary>
        /// Error de conexion de red.
        /// </summary>
        NetworkError = 1001,

        // 2000 - Errores de Productos
        /// <summary>
        /// El producto solicitado no existe en la base de datos.
        /// </summary>
        ProductoNoExiste = 2000,

        /// <summary>
        /// El GTIN ya existe en otro producto.
        /// </summary>
        GtinDuplicado = 2001,

        /// <summary>
        /// La marca especificada no es valida o no existe.
        /// </summary>
        MarcaNoValida = 2002,

        /// <summary>
        /// El nombre del producto ya existe.
        /// </summary>
        NombreProductoDuplicado = 2003,

        /// <summary>
        /// La marca con ese nombre ya existe.
        /// </summary>
        MarcaDuplicada = 2004,

        /// <summary>
        /// La marca tiene productos asociados y no puede eliminarse.
        /// </summary>
        MarcaTieneProductos = 2005,

        // 3000 - Errores de Stock/Inventario
        /// <summary>
        /// No hay suficiente stock disponible para la operacion solicitada.
        /// </summary>
        StockInsuficiente = 3000,

        /// <summary>
        /// El lote ha superado su fecha de vencimiento.
        /// </summary>
        LoteVencido = 3001,

        /// <summary>
        /// El deposito especificado no fue encontrado.
        /// </summary>
        DepositoNoEncontrado = 3002,

        /// <summary>
        /// El numero de serie ya existe en el sistema.
        /// </summary>
        SerieDuplicada = 3003,

        /// <summary>
        /// El lote especificado no existe.
        /// </summary>
        LoteNoEncontrado = 3004,

        /// <summary>
        /// La existencia solicitada no fue encontrada.
        /// </summary>
        ExistenciaNoEncontrada = 3005,

        /// <summary>
        /// Producto unitario debe tener cantidad 1.
        /// </summary>
        ProductoUnitarioCantidadInvalida = 3006,

        /// <summary>
        /// El código del depósito ya existe.
        /// </summary>
        CodigoDepositoDuplicado = 3007,

        /// <summary>
        /// No se especifico numero de serie ni codigo de lote.
        /// </summary>
        SerieYCodigoLoteNoEspecificados = 3008,

        // 4000 - Errores de Clientes
        /// <summary>
        /// El cliente especificado no existe.
        /// </summary>
        ClienteNoExiste = 4000,

        /// <summary>
        /// El GLN del cliente ya existe.
        /// </summary>
        GlnClienteDuplicado = 4001,

        /// <summary>
        /// El CUIT del cliente ya existe.
        /// </summary>
        CuitClienteDuplicado = 4002,

        // 5000 - Errores de Proveedores
        /// <summary>
        /// El proveedor especificado no existe.
        /// </summary>
        ProveedorNoExiste = 5000,

        /// <summary>
        /// El GLN del proveedor ya existe.
        /// </summary>
        GlnProveedorDuplicado = 5001,

        /// <summary>
        /// El CUIT del proveedor ya existe.
        /// </summary>
        CuitProveedorDuplicado = 5002,

        // 6000 - Errores de Remitos
        /// <summary>
        /// El remito especificado no existe.
        /// </summary>
        RemitoNoExiste = 6000,

        /// <summary>
        /// El numero de remito ya existe.
        /// </summary>
        NumeroRemitoDuplicado = 6001,

        /// <summary>
        /// El remito no tiene detalles.
        /// </summary>
        RemitoSinDetalles = 6002,

        /// <summary>
        /// La cantidad en el detalle del remito es invalida.
        /// </summary>
        CantidadInvalida = 6003,

        // 7000 - Errores de Autenticación
        /// <summary>
        /// Credenciales inválidas (usuario o contraseña incorrectos).
        /// </summary>
        CredencialesInvalidas = 7000,

        /// <summary>
        /// El username ya existe.
        /// </summary>
        UsernameDuplicado = 7001,

        /// <summary>
        /// Usuario no activo.
        /// </summary>
        UsuarioInactivo = 7002,

        /// <summary>
        /// Token JWT inválido o expirado.
        /// </summary>
        TokenInvalido = 7003,

        /// <summary>
        /// Se requieren permisos de administrador.
        /// </summary>
        PermisosDenegados = 7004,

        /// <summary>
        /// Usuario no existe.
        /// </summary>
        UsuarioNoExiste = 7005,

        /// <summary>
        /// Nueva contraseña igual a la anterior.
        /// </summary>
        ContrasenaDuplicada = 7006
    }
}

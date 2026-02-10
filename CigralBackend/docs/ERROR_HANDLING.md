# Sistema de Manejo de Errores de Dominio - CigralBackend

Este documento describe los `DomainErrorCode` disponibles, su significado y cómo deben ser manejados por el cliente (frontend).

Resumen:
- Los códigos están definidos en `CigralBackend.Domain/Enums/DomainErrorCode.cs` como un `enum` con valores numéricos.
- Las excepciones de negocio lanzan `DomainException` con una instancia de `DomainErrorCode` y un mensaje descriptivo.
- Se recomienda un middleware global que convierta `DomainException` en respuestas HTTP 400 con el `code` y `codeValue` para que el frontend los maneje fácilmente.

Formato de respuesta recomendado por el middleware para `DomainException`:

```json
{
  "error": "DomainError",
  "code": "NombreDelCodigo",
  "codeValue": 2001,
  "message": "Mensaje descriptivo para el usuario"
}
```

Lista completa y explicación de todos los `DomainErrorCode` actuales

- 1000 - Errores Generales
  - `UnknownError` (1000): Error no identificado. Usar cuando no hay detalle específico.
  - `NetworkError` (1001): Error de conexión o recursos de red.

- 2000 - Productos / Marcas
  - `ProductoNoExiste` (2000): El producto solicitado no existe.
  - `GtinDuplicado` (2001): El GTIN proporcionado ya existe en otro producto.
  - `MarcaNoValida` (2002): La marca indicada no existe o no es válida.
  - `NombreProductoDuplicado` (2003): El nombre del producto ya está en uso.
  - `MarcaDuplicada` (2004): Existe otra marca con el mismo identificador/nombre.
  - `MarcaTieneProductos` (2005): No se puede eliminar la marca porque tiene productos asociados.

- 3000 - Stock / Inventario
  - `StockInsuficiente` (3000): No hay suficiente stock para completar la operación.
  - `LoteVencido` (3001): El lote está vencido y no puede usarse.
  - `DepositoNoEncontrado` (3002): El depósito indicado no existe.
  - `SerieDuplicada` (3003): El número de serie ya existe para ese producto.
  - `LoteNoEncontrado` (3004): El lote indicado no existe.
  - `ExistenciaNoEncontrada` (3005): No existe registro de existencia para el producto/depósito indicado.
  - `ProductoUnitarioCantidadInvalida` (3006): Para productos unitarios la cantidad debe ser 1.
  - `CodigoDepositoDuplicado` (3007): El código del depósito ya existe.

- 4000 - Clientes
  - `ClienteNoExiste` (4000): El cliente no existe.
  - `GlnClienteDuplicado` (4001): El GLN del cliente ya está en uso.
  - `CuitClienteDuplicado` (4002): El CUIT del cliente ya está en uso.

- 5000 - Proveedores
  - `ProveedorNoExiste` (5000): El proveedor no existe.
  - `GlnProveedorDuplicado` (5001): El GLN del proveedor ya está en uso.
  - `CuitProveedorDuplicado` (5002): El CUIT del proveedor ya está en uso.

- 6000 - Remitos / Cantidades
  - `RemitoNoExiste` (6000): El remito no existe.
  - `NumeroRemitoDuplicado` (6001): El número de remito ya está en uso.
  - `RemitoSinDetalles` (6002): El remito no contiene detalles obligatorios.
  - `CantidadInvalida` (6003): La cantidad indicada es inválida (ej. <= 0).

- 7000 - Autenticación y Usuarios
  - `CredencialesInvalidas` (7000): Usuario o contraseña incorrectos.
  - `UsernameDuplicado` (7001): El nombre de usuario ya existe. (Se corrigió el typo anterior `UsernameDeplicado`)
  - `UsuarioInactivo` (7002): El usuario está inactivo y no puede autenticarse.
  - `TokenInvalido` (7003): Token JWT inválido o expirado.
  - `PermisosDenegados` (7004): Se requieren permisos de administrador o similares.
  - `UsuarioNoExiste` (7005): El usuario no fue encontrado.
  - `ContrasenaDuplicada` (7006): La nueva contraseña es igual a la anterior o no cumple la regla solicitada.

Notas importantes para el frontend

1. Usar `codeValue` (entero) cuando se necesite comparaciones rápidas o métricas, y `code` (string) para reglas de negocio legibles.
2. Nunca parsear mensajes libres (`message`) para control de flujo; usar `code`/`codeValue` para lógica.
3. Implementar un mapeo en el frontend que traduzca `DomainErrorCode` a mensajes de UI y acciones (por ejemplo: redirigir al login si `TokenInvalido`).
4. Algunos códigos se pueden mapear a estados HTTP especiales en el middleware:
   - `NotFoundException` -> 404 (no incluye `DomainErrorCode`, sino entidad + key)
   - `DomainException` -> 400 (incluye `code` y `codeValue`)
   - Errores infraestructura (p.ej. `UnknownError`) -> 500

Ejemplos de manejo en frontend (pseudocódigo)

- Caso: registrar usuario -> recibir `UsernameDuplicado`
  ```javascript
  if (response.code === 'UsernameDuplicado') {
    showError("El nombre de usuario ya está en uso.");
  }
  ```

- Caso: cambiar contraseña -> recibir `ContrasenaDuplicada`
  ```javascript
  if (response.codeValue === 7006) {
    showError("La nueva contraseña no puede ser igual a la anterior.");
  }
  ```

Cambios realizados en el repositorio

- Se corrigió el `enum` en `CigralBackend.Domain/Enums/DomainErrorCode.cs`: cambio de `UsernameDeplicado` a `UsernameDuplicado` y se agregó `ContrasenaDuplicada`.
- Se actualizó `AuthService` (`CigralBack.Api/Services/AuthService.cs`) para usar `UsernameDuplicado` y lanzar `ContrasenaDuplicada` cuando corresponda.
- Se actualizó esta documentación para listar todos los códigos y sus significados.

Recomendación final

Añadir/poner en uso un middleware global de manejo de excepciones que traduzca `DomainException` a una respuesta JSON estandarizada (ver ejemplo al inicio). De este modo el frontend tendrá un contrato claro y estable para manejar errores.

Si quieres, puedo:
- Agregar el middleware de ejemplo al proyecto y registrar su uso en `Program.cs`.
- Generar un archivo JSON con la lista completa de `DomainErrorCode` para que el equipo de frontend lo consuma automáticamente.

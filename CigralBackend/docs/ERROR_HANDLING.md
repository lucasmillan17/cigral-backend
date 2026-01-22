# Sistema de Manejo de Errores de Dominio - CigralBackend

## Resumen de Implementacion

Se ha implementado un sistema robusto de manejo de errores de dominio siguiendo las mejores practicas de Clean Architecture.

## Archivos Creados

### 1. Domain Layer - Enums

**Archivo**: `CigralBackend.Domain/Enums/DomainErrorCode.cs`

Enum que define codigos de error organizados por rangos numericos:

- **1000 - Errores Generales**: UnknownError, NetworkError
- **2000 - Productos**: ProductoNoExiste, GtinDuplicado, MarcaNoValida, NombreProductoDuplicado
- **3000 - Stock/Inventario**: StockInsuficiente, LoteVencido, DepositoNoEncontrado, SerieDuplicada, LoteNoEncontrado, ExistenciaNoEncontrada
- **4000 - Clientes**: ClienteNoExiste, GlnClienteDuplicado, CuitClienteDuplicado
- **5000 - Proveedores**: ProveedorNoExiste, GlnProveedorDuplicado, CuitProveedorDuplicado
- **6000 - Remitos**: RemitoNoExiste, NumeroRemitoDuplicado, RemitoSinDetalles, CantidadInvalida

### 2. Domain Layer - Excepciones

**Archivo**: `CigralBackend.Domain/Exceptions/NotFoundException.cs`

Excepcion especializada para entidades no encontradas:

```csharp
throw new NotFoundException(nameof(Producto), id);
// Mensaje: "La entidad Producto (5) no fue encontrada."
```

**Propiedades**:
- `EntityName`: Nombre de la entidad
- `Key`: Identificador de la entidad

**Archivo**: `CigralBackend.Domain/Exceptions/DomainException.cs`

Excepcion base para errores de dominio y reglas de negocio:

```csharp
throw new DomainException(
    DomainErrorCode.GtinDuplicado,
    "El GTIN ya existe en otro producto."
);
```

**Propiedades**:
- `Code`: Codigo de error de dominio (DomainErrorCode)

**Caracteristicas**:
- Mensajes por defecto basados en el codigo de error
- Soporte para mensajes personalizados
- Soporte para InnerException

## Refactorizacion de Servicios

### ProductoService

**Archivo**: `CigralBackend.Application/Services/ProductoService.cs`

#### Metodos Implementados:

1. **CreateProducto**
   - Valida GTIN duplicado -> `DomainException(GtinDuplicado)`
   - Valida nombre duplicado -> `DomainException(NombreProductoDuplicado)`
   - Valida existencia de Marca -> `DomainException(MarcaNoValida)`

2. **GetProductoById**
   - Valida existencia -> `NotFoundException(Producto, id)`

3. **UpdateProducto**
   - Valida existencia del producto -> `NotFoundException(Producto, id)`
   - Valida GTIN duplicado en otro producto -> `DomainException(GtinDuplicado)`
   - Valida nombre duplicado en otro producto -> `DomainException(NombreProductoDuplicado)`
   - Valida existencia de Marca -> `DomainException(MarcaNoValida)`

4. **DeleteProducto**
   - Valida existencia -> `NotFoundException(Producto, id)`

5. **GetAllProductos**
   - Incluye eager loading de Marca

6. **GetProductoFiltered**
   - Incluye eager loading de Marca
   - Filtra por Nombre y GTIN

#### Principios Aplicados:

- **NO se usa try-catch**: Las excepciones suben al middleware
- **Validaciones tempranas**: Fail-fast approach
- **Mensajes descriptivos**: Contexto completo del error
- **Codigos de error**: Facilita manejo en el cliente

## Actualizaciones en DTOs

### ProductoModelRequest

Se agrego el campo `MarcaId` opcional:

```csharp
public record ProductoModelRequest
(
    // ...campos existentes...
    int? MarcaId
);
```

## Actualizaciones en Controllers

### ProductsController

**Archivo**: `CigralBackend.Api/Controllers/ProductsController.cs`

#### Endpoints Implementados:

1. **POST /api/products** - Crear producto
2. **GET /api/products** - Listar con filtros y paginacion
3. **GET /api/products/{id}** - Obtener por ID
4. **PUT /api/products/{id}** - Actualizar producto
5. **DELETE /api/products/{id}** - Eliminar producto

#### Atributos de Documentacion:

- `[ProducesResponseType]`: Documenta codigos de respuesta
- Comentarios XML en cada endpoint

## Proximo Paso Recomendado: Middleware de Manejo de Excepciones

Para completar el sistema, se recomienda crear un middleware global que:

1. Capture `NotFoundException` -> Retorne 404
2. Capture `DomainException` -> Retorne 400 con codigo de error
3. Capture excepciones generales -> Retorne 500

### Ejemplo de Middleware:

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "NotFound",
                message = ex.Message,
                entityName = ex.EntityName,
                key = ex.Key
            });
        }
        catch (DomainException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "DomainError",
                code = ex.Code.ToString(),
                codeValue = (int)ex.Code,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "InternalServerError",
                message = "Ocurrio un error inesperado."
            });
        }
    }
}

// Registrar en Program.cs:
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

## Ventajas del Sistema Implementado

1. **Separacion de Responsabilidades**: Errores de dominio separados de errores de infraestructura
2. **Codigos de Error Tipados**: Enum previene errores de escritura
3. **Mensajes Consistentes**: Mensajes por defecto uniformes
4. **Facilita Testing**: Excepciones predecibles y testeables
5. **Mejor Experiencia del Cliente**: Errores claros y accionables
6. **Trazabilidad**: Codigos numericos facilitan logging y monitoreo

## Ejemplos de Uso en el Cliente

### Ejemplo 1: Producto No Encontrado

**Request**:
```http
GET /api/products/999
```

**Response** (404):
```json
{
  "error": "NotFound",
  "message": "La entidad Producto (999) no fue encontrada.",
  "entityName": "Producto",
  "key": 999
}
```

### Ejemplo 2: GTIN Duplicado

**Request**:
```http
POST /api/products
{
  "nombre": "Producto Test",
  "gtin": "1234567890123",  // Ya existe
  "esUnitario": true
}
```

**Response** (400):
```json
{
  "error": "DomainError",
  "code": "GtinDuplicado",
  "codeValue": 2001,
  "message": "El producto con GTIN 1234567890123 ya existe."
}
```

## Recomendaciones para Extender el Sistema

1. **Logging**: Integrar con Serilog para registrar todas las excepciones
2. **Metricas**: Contar ocurrencias de cada codigo de error
3. **Localizacion**: Soporte multi-idioma en mensajes
4. **Validaciones Complejas**: Crear FluentValidation para reglas complejas
5. **Retry Policies**: Para NetworkError usar Polly
6. **Circuit Breaker**: Proteccion contra fallos en cascada

## Testing

### Ejemplo de Test Unitario:

```csharp
[Fact]
public async Task CreateProducto_ConGtinDuplicado_DeberiaLanzarDomainException()
{
    // Arrange
    var mockRepo = new Mock<IRepository>();
    mockRepo.Setup(r => r.First<Producto>(It.IsAny<Expression<Func<Producto, bool>>>()))
            .ReturnsAsync(new Producto { GTIN = "1234567890123" });
    
    var service = new ProductoService(mockRepo.Object);
    var request = new ProductoModelRequest(
        "Test", "Desc", "1234567890123", true, 100, null
    );
    
    // Act & Assert
    var exception = await Assert.ThrowsAsync<DomainException>(
        () => service.CreateProducto(request)
    );
    
    Assert.Equal(DomainErrorCode.GtinDuplicado, exception.Code);
}
```

---

**Implementacion Completa**: Todos los archivos estan listos para compilar y usar.
**Proximo Paso**: Implementar el Middleware de manejo global de excepciones.

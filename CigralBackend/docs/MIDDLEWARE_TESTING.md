# Middleware de Manejo Global de Excepciones - Testing Guide

## Archivos Creados

### 1. ExceptionHandlingMiddleware.cs
**Ubicacion**: `CigralBackend.Api/Middleware/ExceptionHandlingMiddleware.cs`

Middleware que intercepta todas las excepciones y las convierte en respuestas HTTP estandarizadas.

**Caracteristicas**:
- Captura `NotFoundException` -> 404
- Captura `DomainException` -> 400
- Captura excepciones generales -> 500
- Logging automatico de errores
- Respuestas en formato JSON estandarizado
- Oculta detalles sensibles en produccion

### 2. MiddlewareExtensions.cs
**Ubicacion**: `CigralBackend.Api/Middleware/MiddlewareExtensions.cs`

Metodo de extension para registrar el middleware de forma fluida.

### 3. Program.cs (Actualizado)
Registra el middleware usando `app.UseGlobalExceptionHandler();`

## Ejemplos de Respuestas

### 1. NotFound (404) - Entidad No Encontrada

**Request**:
```http
GET /api/products/999
```

**Response** (404):
```json
{
  "error": "NotFound",
  "message": "La entidad Producto (999) no fue encontrada.",
  "statusCode": 404,
  "timestamp": "2025-01-19T10:30:00.000Z",
  "details": {
    "entityName": "Producto",
    "key": 999
  }
}
```

### 2. DomainError (400) - GTIN Duplicado

**Request**:
```http
POST /api/products
Content-Type: application/json

{
  "nombre": "Producto Test",
  "descripcion": "Descripcion",
  "gtin": "1234567890123",
  "esUnitario": true,
  "precio": 100
}
```

**Response** (400):
```json
{
  "error": "DomainError",
  "message": "El producto con GTIN 1234567890123 ya existe.",
  "statusCode": 400,
  "timestamp": "2025-01-19T10:30:00.000Z",
  "details": {
    "code": "GtinDuplicado",
    "codeValue": 2001
  }
}
```

### 3. DomainError (400) - Marca No Valida

**Request**:
```http
POST /api/products
Content-Type: application/json

{
  "nombre": "Producto Test",
  "descripcion": "Descripcion",
  "gtin": "9999999999999",
  "esUnitario": true,
  "precio": 100,
  "marcaId": 999
}
```

**Response** (400):
```json
{
  "error": "DomainError",
  "message": "La marca con ID 999 no existe.",
  "statusCode": 400,
  "timestamp": "2025-01-19T10:30:00.000Z",
  "details": {
    "code": "MarcaNoValida",
    "codeValue": 2002
  }
}
```

### 4. InternalServerError (500) - Error No Controlado

**Desarrollo** (muestra detalles):
```json
{
  "error": "InternalServerError",
  "message": "Object reference not set to an instance of an object.",
  "statusCode": 500,
  "timestamp": "2025-01-19T10:30:00.000Z",
  "details": {
    "stackTrace": "at CigralBackend.Services.ProductoService...",
    "type": "NullReferenceException"
  }
}
```

**Produccion** (oculta detalles):
```json
{
  "error": "InternalServerError",
  "message": "Ocurrio un error inesperado. Por favor, contacte al administrador.",
  "statusCode": 500,
  "timestamp": "2025-01-19T10:30:00.000Z"
}
```

## Como Probar

### Opcion 1: Usando Swagger

1. Ejecutar la aplicacion: `dotnet run`
2. Abrir `https://localhost:5001/swagger`
3. Probar cada endpoint con datos validos e invalidos

### Opcion 2: Usando cURL

#### Test 1: Producto No Encontrado
```bash
curl -X GET "https://localhost:5001/api/products/999" -H "accept: application/json" -k
```

#### Test 2: GTIN Duplicado
```bash
# Primero crear un producto
curl -X POST "https://localhost:5001/api/products" \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Test Product",
    "descripcion": "Test",
    "gtin": "1234567890123",
    "esUnitario": true,
    "precio": 100
  }' -k

# Luego intentar crear otro con el mismo GTIN
curl -X POST "https://localhost:5001/api/products" \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Test Product 2",
    "descripcion": "Test",
    "gtin": "1234567890123",
    "esUnitario": true,
    "precio": 100
  }' -k
```

### Opcion 3: Usando Postman

Importar la siguiente coleccion:

```json
{
  "info": {
    "name": "CigralBackend - Error Handling Tests",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Test NotFound",
      "request": {
        "method": "GET",
        "header": [],
        "url": {
          "raw": "https://localhost:5001/api/products/999",
          "protocol": "https",
          "host": ["localhost"],
          "port": "5001",
          "path": ["api", "products", "999"]
        }
      }
    },
    {
      "name": "Test GTIN Duplicado",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"nombre\": \"Test Product\",\n  \"descripcion\": \"Test\",\n  \"gtin\": \"1234567890123\",\n  \"esUnitario\": true,\n  \"precio\": 100\n}"
        },
        "url": {
          "raw": "https://localhost:5001/api/products",
          "protocol": "https",
          "host": ["localhost"],
          "port": "5001",
          "path": ["api", "products"]
        }
      }
    }
  ]
}
```

## Logging

El middleware registra automaticamente todos los errores usando `ILogger`:

### Nivel Warning (NotFoundException y DomainException):
```
[Warning] Entidad no encontrada: Producto con clave 999
[Warning] Error de dominio: GtinDuplicado - El producto con GTIN 1234567890123 ya existe.
```

### Nivel Error (Excepciones no controladas):
```
[Error] Error interno del servidor: Object reference not set to an instance of an object.
  at CigralBackend.Services.ProductoService.GetProductoById(Int32 id)
  at ...
```

## Verificar Logs

### En Desarrollo (Console):
Los logs apareceran en la consola al ejecutar `dotnet run`

### En Produccion:
Configurar un proveedor de logging como:
- Serilog
- Application Insights
- Elasticsearch
- File logging

Ejemplo con Serilog en `Program.cs`:
```csharp
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// ... resto del codigo
```

## Personalizacion del Middleware

### Agregar Mas Tipos de Excepciones

Editar `ExceptionHandlingMiddleware.cs`:

```csharp
var response = exception switch
{
    NotFoundException notFoundEx => CreateNotFoundResponse(context, notFoundEx),
    DomainException domainEx => CreateDomainErrorResponse(context, domainEx),
    UnauthorizedAccessException unauthorizedEx => CreateUnauthorizedResponse(context, unauthorizedEx),
    ValidationException validationEx => CreateValidationErrorResponse(context, validationEx),
    _ => CreateInternalServerErrorResponse(context, exception)
};
```

### Agregar Correlation ID

```csharp
public async Task InvokeAsync(HttpContext context)
{
    var correlationId = Guid.NewGuid().ToString();
    context.Response.Headers.Add("X-Correlation-ID", correlationId);
    
    try
    {
        await _next(context);
    }
    catch (Exception ex)
    {
        await HandleExceptionAsync(context, ex, correlationId);
    }
}
```

## Mejores Practicas

1. **No exponer stack traces en produccion**: El middleware ya lo hace
2. **Usar codigos de error consistentes**: DomainErrorCode enum
3. **Logging apropiado**: Warning para errores de negocio, Error para excepciones
4. **Mensajes descriptivos**: Ayudan al cliente a corregir el problema
5. **Timestamp en UTC**: Facilita debugging en multiples zonas horarias

## Testing Automatizado

### Ejemplo de Test de Integracion:

```csharp
[Fact]
public async Task GetProductoById_ProductoNoExiste_RetornaNotFound()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/products/999");
    var content = await response.Content.ReadAsStringAsync();
    var error = JsonSerializer.Deserialize<ErrorResponse>(content);
    
    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    Assert.Equal("NotFound", error.Error);
    Assert.Contains("Producto", error.Message);
    Assert.Contains("999", error.Message);
}

[Fact]
public async Task CreateProducto_GtinDuplicado_RetornaDomainError()
{
    // Arrange
    var client = _factory.CreateClient();
    var producto = new ProductoModelRequest("Test", "Desc", "1234567890123", true, 100, null);
    
    // Crear primer producto
    await client.PostAsJsonAsync("/api/products", producto);
    
    // Act - Intentar crear duplicado
    var response = await client.PostAsJsonAsync("/api/products", producto);
    var content = await response.Content.ReadAsStringAsync();
    var error = JsonSerializer.Deserialize<ErrorResponse>(content);
    
    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    Assert.Equal("DomainError", error.Error);
    Assert.Equal("GtinDuplicado", error.Details["code"]);
    Assert.Equal(2001, error.Details["codeValue"]);
}
```

## Monitoreo en Produccion

### Metricas Recomendadas:
- Numero de errores 404 por endpoint
- Numero de errores 400 por codigo de dominio
- Numero de errores 500 (requiere atencion inmediata)
- Tiempo promedio de respuesta

### Alertas Recomendadas:
- Mas de 10 errores 500 en 5 minutos
- Tasa de errores > 5%
- Errores de dominio especificos que indican problemas (ej: StockInsuficiente frecuente)

---

**Middleware Completo e Implementado** ?

El sistema de manejo de errores esta completamente funcional y listo para produccion.

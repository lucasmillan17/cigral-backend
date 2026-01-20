# Guía de Arquitectura - CigralBackend

## Principios de Diseño

El proyecto CigralBackend sigue los principios de **Clean Architecture** y **Domain-Driven Design (DDD)**, organizando el código en capas bien definidas con responsabilidades claras.

## Capas de la Aplicación

### 1. Domain (Dominio) ??

**Ubicación**: `CigralBackend.Domain`

**Responsabilidad**: Contiene las entidades del negocio y la lógica de dominio puro.

**Características**:
- ? No tiene dependencias externas
- ? Contiene solo entidades y reglas de negocio
- ? Es el núcleo de la aplicación
- ? Define las interfaces que necesita (pero no las implementa)

**Estructura**:
```
CigralBackend.Domain/
??? Bases/
?   ??? EntityBase.cs           # Clase base con Id
?   ??? RemitoBase.cs           # Base para remitos
??? Cliente.cs                  # Entidad Cliente
??? Proveedor.cs                # Entidad Proveedor
??? Producto.cs                 # Entidad Producto
??? Lote.cs                     # Entidad Lote
??? Deposito.cs                 # Entidad Depósito
??? Existencia.cs               # Entidad Existencia
??? DetalleRemito.cs            # Entidad DetalleRemito
??? RemitoCliente.cs            # Entidad RemitoCliente
??? RemitoProveedor.cs          # Entidad RemitoProveedor
```

**Ejemplo**:
```csharp
public class Producto : EntityBase
{
    public string Nombre { get; set; }
    public string GTIN { get; set; }
    public decimal? Precio { get; set; }
    public List<Lote>? Lotes { get; set; }
}
```

### 2. Application (Aplicación) ??

**Ubicación**: `CigralBackend.Application`

**Responsabilidad**: Orquesta la lógica de negocio, coordina el flujo de datos entre capas.

**Características**:
- ? Depende del Domain
- ? Define servicios de aplicación
- ? Contiene DTOs y modelos de validación
- ? Implementa casos de uso
- ? No depende de Infrastructure ni de detalles técnicos

**Estructura**:
```
CigralBackend.Application/
??? Dtos/
?   ??? ClienteDto.cs
?   ??? ClienteModel.cs          # Con validaciones
?   ??? ClienteRequests.cs       # Request/Response
?   ??? ...
??? Services/
    ??? Interfaces/
    ?   ??? IProductoService.cs
    ??? ProductoService.cs
```

**Ejemplo de Servicio**:
```csharp
public class ProductoService : IProductoService
{
    private readonly IRepository _repository;

    public ProductoService(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<Producto>> GetAllProductos(int page, int pageSize)
    {
        return await _repository.GetAll<Producto>(page, pageSize, "Lotes");
    }
}
```

**DTOs vs Models vs Requests**:

- **DTOs** (`*Dto.cs`): Objetos simples para transferencia de datos
- **Models** (`*Model.cs`): Records con validaciones (Data Annotations)
- **Requests** (`*Requests.cs`): Records para Create/Update/Response específicos

### 3. Infrastructure (Infraestructura) ??

**Ubicación**: `CigralBackend.Infrastructure`

**Responsabilidad**: Implementa detalles técnicos como acceso a datos, servicios externos, etc.

**Características**:
- ? Depende del Domain
- ? Implementa interfaces definidas en Application/Domain
- ? Contiene DbContext y repositorios
- ? Gestiona la persistencia de datos

**Estructura**:
```
CigralBackend.Infrastructure/
??? Database/
    ??? CigralBackendContext.cs    # DbContext de EF Core
    ??? EfRepository.cs            # Implementación del repositorio
    ??? Interfaces/
        ??? IRepository.cs         # Interfaz del repositorio
```

**DbContext**:
```csharp
public class CigralBackendContext : DbContext
{
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Producto> Productos { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuración Fluent API
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RazonSocial).HasMaxLength(200);
            // ...
        });
    }
}
```

### 4. API (Presentación) ??

**Ubicación**: `CigralBackend.Api`

**Responsabilidad**: Punto de entrada de la aplicación, expone endpoints HTTP.

**Características**:
- ? Depende de Application e Infrastructure
- ? Contiene Controllers
- ? Configura servicios e inyección de dependencias
- ? Gestiona autenticación y autorización
- ? Configura middleware

**Estructura**:
```
CigralBackend.Api/
??? Controllers/
?   ??? ProductController.cs
??? Program.cs                  # Configuración de la app
??? appsettings.json           # Configuración
??? appsettings.Development.json
```

**Ejemplo de Controller**:
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductoService _productoService;

    public ProductsController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<Producto>>> GetAll(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10)
    {
        var result = await _productoService.GetAllProductos(pageNumber, pageSize);
        return Ok(result);
    }
}
```

## Flujo de Datos

```
????????????????
?   Cliente    ?
?   (HTTP)     ?
????????????????
       ?
       ?
????????????????????????????????????????
?         API Layer                    ?
?  ??????????????                      ?
?  ? Controller ? ??? Validation       ?
?  ??????????????                      ?
????????????????????????????????????????
         ?
         ?
????????????????????????????????????????
?      Application Layer               ?
?  ???????????     ????????????        ?
?  ? Service ? ??? ?   DTOs   ?        ?
?  ???????????     ????????????        ?
????????????????????????????????????????
        ?
        ?
????????????????????????????????????????
?      Domain Layer                    ?
?  ????????????????                    ?
?  ?   Entities   ?                    ?
?  ? (Business    ?                    ?
?  ?   Rules)     ?                    ?
?  ????????????????                    ?
????????????????????????????????????????
        ?
        ?
????????????????????????????????????????
?    Infrastructure Layer              ?
?  ??????????????    ????????????      ?
?  ? Repository ? ???? DbContext?      ?
?  ??????????????    ????????????      ?
????????????????????????????????????????
                          ?
                          ?
                   ???????????????
                   ?  Database   ?
                   ? SQL Server  ?
                   ???????????????
```

## Patrones Implementados

### 1. Repository Pattern

Abstrae el acceso a datos y proporciona una interfaz uniforme:

```csharp
public interface IRepository
{
    Task<T> Add<T>(T entity) where T : EntityBase;
    Task<T> Update<T>(T entity) where T : EntityBase;
    Task<PagedResult<T>> GetAll<T>(int page, int size) where T : EntityBase;
}
```

**Beneficios**:
- ? Desacopla la lógica de negocio del acceso a datos
- ? Facilita el testing (mock del repositorio)
- ? Centraliza las consultas

### 2. Dependency Injection

Configurado en `Program.cs`:

```csharp
// DbContext
builder.Services.AddDbContext<CigralBackendContext>(options =>
    options.UseSqlServer(connectionString));

// Repositorio
builder.Services.AddScoped<IRepository, EfRepository>();

// Servicios
builder.Services.AddScoped<IProductoService, ProductoService>();
```

**Beneficios**:
- ? Bajo acoplamiento
- ? Mayor testabilidad
- ? Facilita el cambio de implementaciones

### 3. DTO Pattern

Separa las entidades del dominio de las representaciones externas:

```csharp
// Entidad del dominio
public class Producto : EntityBase
{
    public string Nombre { get; set; }
    public List<Lote> Lotes { get; set; }  // Relación compleja
}

// DTO para API
public class ProductoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    // Sin Lotes completos, solo información necesaria
}
```

**Beneficios**:
- ? Controla qué datos se exponen
- ? Evita sobre-serialización
- ? Optimiza el tamaño de las respuestas

## Principios SOLID Aplicados

### Single Responsibility (S)
Cada clase tiene una única responsabilidad:
- `ProductoService`: Solo gestiona operaciones de productos
- `EfRepository`: Solo gestiona acceso a datos

### Open/Closed (O)
Abierto para extensión, cerrado para modificación:
- `IRepository` permite nuevas implementaciones sin cambiar código existente

### Liskov Substitution (L)
Las implementaciones pueden sustituir a sus interfaces:
- `EfRepository` puede reemplazarse por cualquier implementación de `IRepository`

### Interface Segregation (I)
Interfaces específicas en lugar de genéricas:
- `IProductoService` en lugar de un `IService` genérico

### Dependency Inversion (D)
Depender de abstracciones, no de concreciones:
- Services dependen de `IRepository`, no de `EfRepository`

## Mejores Prácticas

### ? DO (Hacer)

1. **Mantener el Domain limpio**: Sin dependencias externas
2. **Usar DTOs**: Para transferencia de datos
3. **Validar en el límite**: Validaciones en la capa de API
4. **Usar async/await**: Para operaciones I/O
5. **Documentar con XML**: Comentarios en interfaces públicas

### ? DON'T (No hacer)

1. **No referenciar Infrastructure desde Domain**
2. **No poner lógica de negocio en Controllers**
3. **No exponer entidades directamente en la API**
4. **No hacer queries en Controllers**
5. **No ignorar la paginación en listas grandes**

## Testing Strategy

```
Tests/
??? Unit/
?   ??? Services/          # Tests de servicios (mockeando repository)
?   ??? Domain/            # Tests de entidades y lógica de negocio
??? Integration/
?   ??? Controllers/       # Tests de endpoints
?   ??? Repository/        # Tests con BD en memoria
??? E2E/
    ??? Scenarios/         # Tests end-to-end
```

## Referencias

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/tags/domain%20driven%20design.html)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)

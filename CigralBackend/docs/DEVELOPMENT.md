# Guia de Desarrollo - CigralBackend

## Configuracion del Entorno de Desarrollo

### Prerrequisitos

1. **.NET 8 SDK**
   - Descargar de [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Verificar instalacion: `dotnet --version`

2. **SQL Server**
   - SQL Server 2019+ o SQL Server Express
   - SQL Server Management Studio (SSMS) - opcional pero recomendado

3. **IDE (uno de los siguientes)**
   - Visual Studio 2022 Community/Professional/Enterprise
   - Visual Studio Code con extensiones:
     - C# Dev Kit
     - C#
     - .NET Extension Pack

4. **Git**
   - Para control de versiones

### Configuracion Inicial

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/lucasmillan17/cigral-backend.git
   cd cigral-backend
   ```

2. **Configurar la cadena de conexion**
   
   Editar `CigralBackend.Api/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=CigralBackendDB;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```

3. **Restaurar paquetes**
   ```bash
   dotnet restore
   ```

4. **Crear la base de datos**
   ```bash
   cd CigralBackend
   dotnet ef migrations add InitialCreate --project ..\CigralBackend.Infraestructure --startup-project .
   dotnet ef database update --project ..\CigralBackend.Infraestructure --startup-project .
   ```

5. **Ejecutar la aplicacion**
   ```bash
   dotnet run --project CigralBackend.Api
   ```

6. **Verificar**
   - Abrir navegador en `https://localhost:5001/swagger`
   - Deberas ver la documentacion de Swagger

## Estructura de Branches

### Main Branches

- **`main`**: Codigo en produccion, siempre estable
- **`development`**: Rama de desarrollo principal
- **`staging`**: Pre-produccion

### Feature Branches

Formato: `feature/nombre-descriptivo`

Ejemplo:
```bash
git checkout development
git checkout -b feature/agregar-autenticacion
# ... hacer cambios ...
git add .
git commit -m "feat: agregar autenticacion JWT"
git push origin feature/agregar-autenticacion
```

### Otros Branches

- **`bugfix/`**: Para correcciones de bugs
- **`hotfix/`**: Para correcciones urgentes en produccion
- **`refactor/`**: Para refactorizaciones
- **`docs/`**: Para documentacion

## Convenciones de Codigo

### Naming Conventions

#### Clases y Metodos
```csharp
// PascalCase para clases, metodos, propiedades
public class ProductoService
{
    public async Task<Producto> GetProductoById(Guid id)
    {
        // ...
    }
}
```

#### Variables y Parametros
```csharp
// camelCase para variables locales y parametros
public void ProcesarProducto(Producto producto)
{
    var nombreProducto = producto.Nombre;
    int cantidadTotal = 0;
}
```

#### Constantes
```csharp
// PascalCase con prefijo
public const int MaxProductosPorPagina = 100;
private const string DefaultConnectionString = "...";
```

#### Campos Privados
```csharp
// camelCase con guion bajo
private readonly IRepository _repository;
private int _contador;
```

### Organizacion de Archivos

#### Un archivo por clase
```
? Producto.cs
? ProductoService.cs
? Modelos.cs (con multiples clases)
```

#### Namespaces coinciden con carpetas
```csharp
// Archivo: CigralBackend.Application/Services/ProductoService.cs
namespace CigralBackend.Application.Services
{
    public class ProductoService
    {
        // ...
    }
}
```

### Comentarios y Documentacion

#### XML Documentation para APIs publicas
```csharp
/// <summary>
/// Obtiene un producto por su identificador unico.
/// </summary>
/// <param name="id">El identificador del producto</param>
/// <returns>El producto encontrado o null</returns>
/// <exception cref="ArgumentException">Si el id es vacio</exception>
public async Task<Producto?> GetProductoById(Guid id)
{
    if (id == Guid.Empty)
        throw new ArgumentException("El id no puede estar vacio", nameof(id));
        
    return await _repository.GetById<Producto>(id);
}
```

### Commits

Usar [Conventional Commits](https://www.conventionalcommits.org/):

```bash
# Formato
<type>(<scope>): <subject>

# Tipos
feat:     Nueva funcionalidad
fix:      Correccion de bug
docs:     Cambios en documentacion
style:    Cambios de formato (sin afectar codigo)
refactor: Refactorizacion
test:     Agregar o modificar tests
chore:    Cambios en build, CI, etc.

# Ejemplos
git commit -m "feat(producto): agregar validacion de GTIN"
git commit -m "fix(repository): corregir paginacion en GetAll"
git commit -m "docs: actualizar README con instrucciones de deploy"
```

## Workflow de Desarrollo

### 1. Crear Feature Branch

```bash
git checkout development
git pull origin development
git checkout -b feature/nueva-funcionalidad
```

### 2. Desarrollo

```bash
# Hacer cambios
# Ejecutar tests
dotnet test

# Verificar build
dotnet build

# Commit frecuentes
git add .
git commit -m "feat: descripcion del cambio"
```

### 3. Antes de Push

```bash
# Actualizar con development
git checkout development
git pull origin development
git checkout feature/nueva-funcionalidad
git merge development

# Resolver conflictos si existen
# Ejecutar tests nuevamente
dotnet test

# Push
git push origin feature/nueva-funcionalidad
```

## Agregar Nueva Funcionalidad

### Ejemplo: Agregar Categorias de Productos

#### 1. Crear Entidad (Domain)

```csharp
// CigralBackend.Domain/Categoria.cs
namespace CigralBackend.Domain
{
    public class Categoria : EntityBase
    {
        public Categoria() { }
        
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public List<Producto>? Productos { get; set; }
    }
}
```

#### 2. Actualizar DbContext (Infrastructure)

```csharp
// CigralBackend.Infrastructure/Database/CigralBackendContext.cs
public DbSet<Categoria> Categorias { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ... existing code ...
    
    modelBuilder.Entity<Categoria>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
        entity.Property(e => e.Descripcion).HasMaxLength(500);
    });
}
```

#### 3. Crear Migracion

```bash
cd CigralBackend
dotnet ef migrations add AgregarCategoria --project ..\CigralBackend.Infraestructure --startup-project .
dotnet ef database update --project ..\CigralBackend.Infraestructure --startup-project .
```

#### 4. Crear DTOs (Application)

```csharp
// CigralBackend.Application/Dtos/CategoriaModel.cs
using System.ComponentModel.DataAnnotations;

namespace CigralBackend.Application.Dtos
{
    public record CategoriaModelRequest
    (
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100)]
        string Nombre,
        
        [MaxLength(500)]
        string? Descripcion
    );
}
```

#### 5. Crear Servicio (Application)

```csharp
// CigralBackend.Application/Services/CategoriaService.cs
public class CategoriaService : ICategoriaService
{
    private readonly IRepository _repository;

    public CategoriaService(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<Categoria>> GetAllCategorias(int pageNumber, int pageSize)
    {
        return await _repository.GetAll<Categoria>(pageNumber, pageSize);
    }

    public async Task<Categoria> CreateCategoria(CategoriaModelRequest request)
    {
        var categoria = new Categoria
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Descripcion = request.Descripcion
        };

        return await _repository.Add(categoria);
    }
}
```

#### 6. Crear Controller (API)

```csharp
// CigralBackend.Api/Controllers/CategoriasController.cs
[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<Categoria>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _categoriaService.GetAllCategorias(pageNumber, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Categoria>> Create([FromBody] CategoriaModelRequest request)
    {
        var categoria = await _categoriaService.CreateCategoria(request);
        return CreatedAtAction(nameof(GetAll), new { id = categoria.Id }, categoria);
    }
}
```

#### 7. Registrar Servicio (API)

```csharp
// Program.cs
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
```

#### 8. Verificar

```bash
# Compilar
dotnet build

# Ejecutar
dotnet run --project CigralBackend.Api

# Probar en Swagger
# https://localhost:5001/swagger
```

## Debugging

### Visual Studio

1. Establecer breakpoints (F9)
2. Presionar F5 para iniciar debug
3. Usar F10 (Step Over), F11 (Step Into)

### Visual Studio Code

1. Configurar `launch.json`
2. Presionar F5

## Comandos Utiles

```bash
# Ver info del proyecto
dotnet --info

# Limpiar build
dotnet clean

# Restaurar + Build + Run
dotnet watch run --project CigralBackend.Api

# Listar paquetes instalados
dotnet list package

# Actualizar paquete
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.1
```

## Recursos

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [C# Coding Conventions](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)

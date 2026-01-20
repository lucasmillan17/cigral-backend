# Guía de Desarrollo - CigralBackend

## Configuración del Entorno de Desarrollo

### Prerrequisitos

1. **.NET 8 SDK**
   - Descargar de [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Verificar instalación: `dotnet --version`

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

### Configuración Inicial

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/lucasmillan17/cigral-backend.git
   cd cigral-backend
   ```

2. **Configurar la cadena de conexión**
   
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

5. **Ejecutar la aplicación**
   ```bash
   dotnet run --project CigralBackend.Api
   ```

6. **Verificar**
   - Abrir navegador en `https://localhost:5001/swagger`
   - Deberías ver la documentación de Swagger

## Estructura de Branches

### Main Branches

- **`main`**: Código en producción, siempre estable
- **`development`**: Rama de desarrollo principal
- **`staging`**: Pre-producción

### Feature Branches

Formato: `feature/nombre-descriptivo`

Ejemplo:
```bash
git checkout development
git checkout -b feature/agregar-autenticacion
# ... hacer cambios ...
git add .
git commit -m "feat: agregar autenticación JWT"
git push origin feature/agregar-autenticacion
```

### Otros Branches

- **`bugfix/`**: Para correcciones de bugs
- **`hotfix/`**: Para correcciones urgentes en producción
- **`refactor/`**: Para refactorizaciones
- **`docs/`**: Para documentación

## Convenciones de Código

### Naming Conventions

#### Clases y Métodos
```csharp
// PascalCase para clases, métodos, propiedades
public class ProductoService
{
    public async Task<Producto> GetProductoById(Guid id)
    {
        // ...
    }
}
```

#### Variables y Parámetros
```csharp
// camelCase para variables locales y parámetros
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

### Organización de Archivos

#### Un archivo por clase
```
? Producto.cs
? ProductoService.cs
? Modelos.cs (con múltiples clases)
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

### Comentarios y Documentación

#### XML Documentation para APIs públicas
```csharp
/// <summary>
/// Obtiene un producto por su identificador único.
/// </summary>
/// <param name="id">El identificador del producto</param>
/// <returns>El producto encontrado o null</returns>
/// <exception cref="ArgumentException">Si el id es vacío</exception>
public async Task<Producto?> GetProductoById(Guid id)
{
    if (id == Guid.Empty)
        throw new ArgumentException("El id no puede estar vacío", nameof(id));
        
    return await _repository.GetById<Producto>(id);
}
```

#### Comentarios TODO
```csharp
// TODO: Implementar caché para mejorar performance
// FIXME: Corregir validación de GTIN
// HACK: Solución temporal hasta refactorizar
```

### Commits

Usar [Conventional Commits](https://www.conventionalcommits.org/):

```bash
# Formato
<type>(<scope>): <subject>

# Tipos
feat:     Nueva funcionalidad
fix:      Corrección de bug
docs:     Cambios en documentación
style:    Cambios de formato (sin afectar código)
refactor: Refactorización
test:     Agregar o modificar tests
chore:    Cambios en build, CI, etc.

# Ejemplos
git commit -m "feat(producto): agregar validación de GTIN"
git commit -m "fix(repository): corregir paginación en GetAll"
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
git commit -m "feat: descripción del cambio"
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

### 4. Pull Request

1. Ir a GitHub
2. Crear Pull Request de `feature/nueva-funcionalidad` ? `development`
3. Completar template de PR
4. Solicitar revisión
5. Esperar aprobación y CI/CD verde

### 5. Code Review Checklist

- [ ] El código compila sin errores
- [ ] Los tests pasan
- [ ] Se agregaron tests para nueva funcionalidad
- [ ] La documentación está actualizada
- [ ] Se siguieron las convenciones de código
- [ ] No hay código comentado innecesario
- [ ] Las variables tienen nombres descriptivos
- [ ] Se agregó documentación XML a APIs públicas

## Agregar Nueva Funcionalidad

### Ejemplo: Agregar Categorías de Productos

#### 1. Crear Entidad (Domain)

```csharp
// CigralBackend.Domain/Categoria.cs
namespace CigralBackend.Domain
{
    /// <summary>
    /// Representa una categoría de productos.
    /// </summary>
    public class Categoria : EntityBase
    {
        public Categoria() { }
        
        /// <summary>
        /// Nombre de la categoría.
        /// </summary>
        public string Nombre { get; set; }
        
        /// <summary>
        /// Descripción de la categoría.
        /// </summary>
        public string? Descripcion { get; set; }
        
        /// <summary>
        /// Productos asociados a esta categoría.
        /// </summary>
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
        entity.Property(e => e.Nombre)
            .HasMaxLength(100)
            .IsRequired();
        entity.Property(e => e.Descripcion)
            .HasMaxLength(500);
        entity.HasMany(e => e.Productos)
            .WithOne(p => p.Categoria)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    });
}
```

#### 3. Crear Migración

```bash
cd CigralBackend
dotnet ef migrations add AgregarCategoria --project ..\CigralBackend.Infraestructure --startup-project .
dotnet ef database update --project ..\CigralBackend.Infraestructure --startup-project .
```

#### 4. Crear DTOs (Application)

```csharp
// CigralBackend.Application/Dtos/CategoriaDto.cs
namespace CigralBackend.Application.Dtos
{
    public class CategoriaDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
    }
}

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
// CigralBackend.Application/Services/Interfaces/ICategoriaService.cs
namespace CigralBackend.Application.Services.Interfaces
{
    public interface ICategoriaService
    {
        Task<PagedResult<Categoria>> GetAllCategorias(int pageNumber, int pageSize);
        Task<Categoria?> GetCategoriaById(Guid id);
        Task<Categoria> CreateCategoria(CategoriaModelRequest request);
        Task<Categoria> UpdateCategoria(Guid id, CategoriaModelRequest request);
        Task DeleteCategoria(Guid id);
    }
}

// CigralBackend.Application/Services/CategoriaService.cs
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Infrastructure.Database.Interfaces;

namespace CigralBackend.Application.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly IRepository _repository;

        public CategoriaService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResult<Categoria>> GetAllCategorias(int pageNumber, int pageSize)
        {
            return await _repository.GetAll<Categoria>(pageNumber, pageSize, "Productos");
        }

        public async Task<Categoria?> GetCategoriaById(Guid id)
        {
            return await _repository.GetById<Categoria>(id, "Productos");
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

        public async Task<Categoria> UpdateCategoria(Guid id, CategoriaModelRequest request)
        {
            var categoria = await _repository.GetById<Categoria>(id);
            if (categoria == null)
                throw new KeyNotFoundException($"Categoría con id {id} no encontrada");

            categoria.Nombre = request.Nombre;
            categoria.Descripcion = request.Descripcion;

            return await _repository.Update(categoria);
        }

        public async Task DeleteCategoria(Guid id)
        {
            var categoria = await _repository.GetById<Categoria>(id);
            if (categoria == null)
                throw new KeyNotFoundException($"Categoría con id {id} no encontrada");

            await _repository.Delete(categoria);
        }
    }
}
```

#### 6. Crear Controller (API)

```csharp
// CigralBackend.Api/Controllers/CategoriasController.cs
using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CigralBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriasController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        /// <summary>
        /// Obtiene todas las categorías con paginación.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<Categoria>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _categoriaService.GetAllCategorias(pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene una categoría por su ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Categoria>> GetById(Guid id)
        {
            var categoria = await _categoriaService.GetCategoriaById(id);
            if (categoria == null)
                return NotFound();

            return Ok(categoria);
        }

        /// <summary>
        /// Crea una nueva categoría.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Categoria>> Create(
            [FromBody] CategoriaModelRequest request)
        {
            var categoria = await _categoriaService.CreateCategoria(request);
            return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
        }

        /// <summary>
        /// Actualiza una categoría existente.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Categoria>> Update(
            Guid id,
            [FromBody] CategoriaModelRequest request)
        {
            try
            {
                var categoria = await _categoriaService.UpdateCategoria(id, request);
                return Ok(categoria);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Elimina una categoría.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _categoriaService.DeleteCategoria(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
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

1. Configurar `launch.json`:
```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Launch (web)",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/CigralBackend.Api/bin/Debug/net8.0/CigralBackend.Api.dll",
            "args": [],
            "cwd": "${workspaceFolder}/CigralBackend.Api",
            "stopAtEntry": false,
            "serverReadyAction": {
                "action": "openExternally",
                "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
            },
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            }
        }
    ]
}
```

2. Presionar F5

## Tips y Tricks

### Snippets Útiles

#### Crear una entidad rápidamente
```csharp
// Escribir "prop" + Tab + Tab
public string Nombre { get; set; }

// Escribir "ctor" + Tab + Tab para constructor
public MiClase()
{
}
```

### Comandos dotnet útiles

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

### SQL Server Tips

```sql
-- Ver todas las tablas
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES

-- Ver estructura de una tabla
sp_help 'Productos'

-- Ver datos
SELECT TOP 10 * FROM Productos
```

## Recursos

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [C# Coding Conventions](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)

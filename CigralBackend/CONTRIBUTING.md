# Contribuir a CigralBackend

¡Gracias por tu interés en contribuir a CigralBackend! ??

## Código de Conducta

Este proyecto y todos los participantes están sujetos a un código de conducta. Al participar, se espera que mantengas este código. Por favor reporta comportamientos inaceptables a [tu-email@ejemplo.com].

## ¿Cómo puedo contribuir?

### Reportar Bugs

Si encuentras un bug, por favor crea un issue con la siguiente información:

**Template de Bug Report:**

```markdown
**Descripción del Bug**
Una descripción clara y concisa del bug.

**Pasos para Reproducir**
1. Ir a '...'
2. Hacer click en '....'
3. Scrollear hasta '....'
4. Ver error

**Comportamiento Esperado**
Qué esperabas que sucediera.

**Comportamiento Actual**
Qué sucedió en realidad.

**Screenshots**
Si es aplicable, agrega screenshots.

**Entorno:**
 - OS: [e.g. Windows 11]
 - .NET Version: [e.g. 8.0]
 - SQL Server Version: [e.g. 2019]

**Información Adicional**
Cualquier otro contexto sobre el problema.
```

### Sugerir Mejoras

Las sugerencias de mejoras son bienvenidas. Crea un issue con:

**Template de Feature Request:**

```markdown
**¿Es tu feature request relacionada a un problema?**
Una descripción clara del problema. Ej. Siempre me frustra cuando [...]

**Describe la solución que te gustaría**
Una descripción clara y concisa de lo que quieres que suceda.

**Describe alternativas que hayas considerado**
Otras soluciones o features que hayas considerado.

**Contexto adicional**
Cualquier otro contexto o screenshots sobre el feature request.
```

### Pull Requests

1. **Fork** el repositorio
2. **Crea** tu feature branch (`git checkout -b feature/AmazingFeature`)
3. **Commit** tus cambios (`git commit -m 'feat: Add some AmazingFeature'`)
4. **Push** a la branch (`git push origin feature/AmazingFeature`)
5. **Abre** un Pull Request

## Guía de Estilo

### Commits

Usamos [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: Nueva funcionalidad
- `fix`: Corrección de bug
- `docs`: Cambios en documentación
- `style`: Cambios de formato (sin afectar el código)
- `refactor`: Refactorización de código
- `test`: Agregar o modificar tests
- `chore`: Cambios en el build, CI, etc.
- `perf`: Mejoras de performance

**Ejemplos:**

```bash
feat(producto): agregar validación de GTIN
fix(repository): corregir paginación cuando pageSize es 0
docs(readme): actualizar instrucciones de instalación
refactor(service): simplificar lógica de filtrado
test(producto): agregar tests para CreateProducto
```

### Código C#

#### Naming Conventions

```csharp
// ? Clases: PascalCase
public class ProductoService { }

// ? Interfaces: I + PascalCase
public interface IProductoService { }

// ? Métodos: PascalCase
public async Task<Producto> GetProductoById(Guid id) { }

// ? Propiedades: PascalCase
public string Nombre { get; set; }

// ? Parámetros y variables locales: camelCase
public void ProcesarProducto(Producto producto)
{
    var nombreProducto = producto.Nombre;
}

// ? Campos privados: _camelCase
private readonly IRepository _repository;

// ? Constantes: PascalCase
private const int MaxPageSize = 100;
```

#### Spacing y Formato

```csharp
// ? Usar espacios, no tabs (4 espacios)
// ? Abrir llaves en nueva línea
public class MiClase
{
    public void MiMetodo()
    {
        if (condicion)
        {
            // código
        }
    }
}

// ? Espacio después de palabras clave
if (condicion) { }
for (int i = 0; i < 10; i++) { }
while (true) { }

// ? Espacio alrededor de operadores
var total = precio + impuesto;
var resultado = a == b && c != d;
```

#### Documentación XML

```csharp
/// <summary>
/// Obtiene un producto por su identificador único.
/// </summary>
/// <param name="id">El identificador del producto</param>
/// <returns>El producto encontrado o null si no existe</returns>
/// <exception cref="ArgumentException">Si el id es Guid.Empty</exception>
public async Task<Producto?> GetProductoById(Guid id)
{
    // implementación
}
```

### SQL y Entity Framework

#### Configuración de Entidades

```csharp
// ? Usar Fluent API en OnModelCreating
modelBuilder.Entity<Producto>(entity =>
{
    entity.HasKey(e => e.Id);
    
    entity.Property(e => e.Nombre)
        .HasMaxLength(100)
        .IsRequired();
    
    entity.HasMany(e => e.Lotes)
        .WithOne(l => l.Producto)
        .HasForeignKey(l => l.ProductoId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

#### Migraciones

```bash
# Nombres descriptivos en inglés
dotnet ef migrations add AddProductoCategoriaRelation

# No
dotnet ef migrations add Migration1
```

## Proceso de Review

### Checklist del Autor

Antes de crear el PR, verifica:

- [ ] El código compila sin warnings
- [ ] Todos los tests pasan
- [ ] Se agregaron tests para nueva funcionalidad
- [ ] La documentación está actualizada
- [ ] Se siguieron las convenciones de código
- [ ] No hay código comentado o console.logs
- [ ] Las variables tienen nombres descriptivos
- [ ] Se agregó documentación XML a APIs públicas
- [ ] No hay cambios no relacionados (whitespace, formateo)
- [ ] El commit message sigue Conventional Commits

### Checklist del Reviewer

Al revisar un PR:

- [ ] El código es claro y fácil de entender
- [ ] No introduce vulnerabilidades de seguridad
- [ ] Sigue las convenciones del proyecto
- [ ] La lógica de negocio está en el lugar correcto
- [ ] No hay duplicación de código
- [ ] Las validaciones son adecuadas
- [ ] El manejo de errores es apropiado
- [ ] Los tests cubren casos edge
- [ ] La documentación es clara
- [ ] No hay impacto negativo en performance

## Estructura de PR

```markdown
## Descripción
Descripción breve de los cambios

## Tipo de cambio
- [ ] Bug fix (cambio que corrige un issue)
- [ ] Nueva funcionalidad (cambio que agrega funcionalidad)
- [ ] Breaking change (fix o feature que causaría que funcionalidad existente no funcione como se espera)
- [ ] Cambio de documentación

## ¿Cómo se ha probado?
Describe las pruebas que ejecutaste para verificar tus cambios.

## Checklist:
- [ ] Mi código sigue el estilo del proyecto
- [ ] He realizado una auto-revisión de mi código
- [ ] He comentado mi código, particularmente en áreas difíciles de entender
- [ ] He hecho los cambios correspondientes a la documentación
- [ ] Mis cambios no generan nuevos warnings
- [ ] He agregado tests que prueban que mi fix es efectivo o que mi feature funciona
- [ ] Tests unitarios nuevos y existentes pasan localmente con mis cambios

## Screenshots (si aplica)
Agrega screenshots para ayudar a explicar tu cambio.

## Issues relacionados
Fixes #(issue number)
```

## Configuración del Entorno de Desarrollo

### Fork y Clone

```bash
# Fork el repositorio en GitHub
# Luego clona tu fork
git clone https://github.com/TU-USUARIO/cigral-backend.git
cd cigral-backend

# Agrega el repositorio original como remote
git remote add upstream https://github.com/lucasmillan17/cigral-backend.git

# Verifica los remotes
git remote -v
```

### Mantener tu Fork Actualizado

```bash
# Obtener cambios del upstream
git fetch upstream

# Mergear en tu rama local
git checkout development
git merge upstream/development

# Push a tu fork
git push origin development
```

### Crear Feature Branch

```bash
# Desde development actualizado
git checkout development
git pull upstream development

# Crear nueva branch
git checkout -b feature/mi-nueva-funcionalidad
```

### Hacer Commits

```bash
# Agregar archivos
git add .

# Commit con mensaje descriptivo
git commit -m "feat(producto): agregar campo categoria"

# Push a tu fork
git push origin feature/mi-nueva-funcionalidad
```

## Testing

### Ejecutar Tests

```bash
# Todos los tests
dotnet test

# Tests de un proyecto específico
dotnet test CigralBackend.Tests

# Con coverage
dotnet test /p:CollectCoverage=true
```

### Escribir Tests

```csharp
using Xunit;
using Moq;

namespace CigralBackend.Tests.Services
{
    public class ProductoServiceTests
    {
        [Fact]
        public async Task GetProductoById_WithValidId_ReturnsProducto()
        {
            // Arrange
            var mockRepo = new Mock<IRepository>();
            var expectedProducto = new Producto { Id = Guid.NewGuid(), Nombre = "Test" };
            mockRepo.Setup(r => r.GetById<Producto>(It.IsAny<Guid>()))
                    .ReturnsAsync(expectedProducto);
            
            var service = new ProductoService(mockRepo.Object);
            
            // Act
            var result = await service.GetProductoById(expectedProducto.Id);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProducto.Nombre, result.Nombre);
        }
    }
}
```

## Documentación

### Actualizar README

Si agregas una nueva funcionalidad importante, actualiza el README.md con:
- Breve descripción de la feature
- Ejemplo de uso si aplica

### Crear Documentación Detallada

Para features complejas, crea documentación en `docs/`:

```markdown
# Nombre de la Feature

## Descripción
...

## Uso
...

## Ejemplos
...

## API Reference
...
```

## Recursos

- [Guía de Arquitectura](docs/ARCHITECTURE.md)
- [Guía de Desarrollo](docs/DEVELOPMENT.md)
- [Configuración de Base de Datos](DATABASE_SETUP.md)

## Preguntas

¿Tienes preguntas? Abre un issue con la etiqueta `question`.

## Licencia

Al contribuir a CigralBackend, aceptas que tus contribuciones serán licenciadas bajo la MIT License.

---

**¡Gracias por contribuir a CigralBackend!** ??

# CigralBackend - Entity Framework Core Setup

## Configuración de la Base de Datos

Este proyecto utiliza Entity Framework Core con SQL Server para la persistencia de datos.

### Cadena de Conexión

La cadena de conexión se encuentra en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CigralBackendDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Nota:** Modifica la cadena de conexión según tu configuración de SQL Server.

### Crear y Aplicar Migraciones

#### 1. Crear la primera migración

Desde el directorio raíz de la solución, ejecuta:

```bash
dotnet ef migrations add InitialCreate --project CigralBackend.Infraestructure --startup-project CigralBackend
```

#### 2. Aplicar la migración a la base de datos

```bash
dotnet ef database update --project CigralBackend.Infraestructure --startup-project CigralBackend
```

#### 3. Agregar nuevas migraciones (cuando cambies el modelo)

```bash
dotnet ef migrations add NombreDeLaMigracion --project CigralBackend.Infraestructure --startup-project CigralBackend
dotnet ef database update --project CigralBackend.Infraestructure --startup-project CigralBackend
```

#### 4. Revertir una migración

```bash
dotnet ef database update NombreMigracionAnterior --project CigralBackend.Infraestructure --startup-project CigralBackend
```

#### 5. Eliminar la última migración

```bash
dotnet ef migrations remove --project CigralBackend.Infraestructure --startup-project CigralBackend
```

## Características Implementadas

### 1. DbContext Configurado

- **CigralBackendContext**: Contexto de Entity Framework con todas las entidades del dominio
- Configuración completa de relaciones entre entidades
- Restricciones de longitud en campos de texto
- Configuración de tipos decimales para precios

### 2. Repositorio Genérico con Paginación

El `EfRepository` implementa:

- **Add**: Agregar nuevas entidades
- **Update**: Actualizar entidades existentes
- **Delete**: Eliminar entidades
- **GetById**: Obtener por ID con includes opcionales
- **First**: Obtener la primera coincidencia de un filtro
- **GetAll**: Obtener todos los registros con **paginación**
- **GetFiltered**: Obtener registros filtrados con **paginación**

#### Ejemplo de uso de paginación:

```csharp
// Obtener la página 2 con 20 elementos por página
var result = await _repository.GetAll<Producto>(pageNumber: 2, pageSize: 20, "Lotes");

Console.WriteLine($"Total de productos: {result.TotalCount}");
Console.WriteLine($"Página {result.PageNumber} de {result.TotalPages}");
Console.WriteLine($"Tiene página anterior: {result.HasPreviousPage}");
Console.WriteLine($"Tiene página siguiente: {result.HasNextPage}");

foreach (var producto in result.Items)
{
    Console.WriteLine(producto.Nombre);
}
```

### 3. Entidades del Dominio

Todas las entidades heredan de `EntityBase` y tienen `Id` de tipo `Guid`:

- **Cliente**: Información de clientes
- **Proveedor**: Información de proveedores
- **Producto**: Catálogo de productos
- **Lote**: Lotes de productos con vencimiento
- **Deposito**: Almacenes o depósitos
- **Existencia**: Stock de productos por depósito
- **DetalleRemito**: Líneas de items en remitos
- **RemitoCliente**: Remitos de salida a clientes
- **RemitoProveedor**: Remitos de entrada de proveedores

### 4. DTOs y Modelos

- **DTOs**: Representación de datos para transferencia
- **Request Models**: Modelos con validaciones para crear/actualizar entidades
- **Response Models**: Modelos para respuestas de la API

## Inyección de Dependencias

El `Program.cs` ya está configurado con:

```csharp
// DbContext
builder.Services.AddDbContext<CigralBackendContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositorio
builder.Services.AddScoped<IRepository, EfRepository>();
```

## Próximos Pasos

1. Crear las migraciones con los comandos anteriores
2. Implementar servicios de aplicación que usen el repositorio
3. Crear controladores que consuman los servicios
4. Agregar AutoMapper para mapeo entre entidades y DTOs

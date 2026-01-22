# DATABASE SETUP - CigralBackend

Guia completa de configuracion de Entity Framework Core y migraciones.

## Configuracion de la Base de Datos

Este proyecto utiliza Entity Framework Core con SQL Server para la persistencia de datos.

### Cadena de Conexion

La cadena de conexion se encuentra en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CigralBackendDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Nota:** Modifica la cadena de conexion segun tu configuracion de SQL Server.

### Crear y Aplicar Migraciones

#### 1. Crear la primera migracion

Desde el directorio raiz de la solucion, ejecuta:

```bash
dotnet ef migrations add InitialCreate --project CigralBackend.Infraestructure --startup-project CigralBackend
```

#### 2. Aplicar la migracion a la base de datos

```bash
dotnet ef database update --project CigralBackend.Infraestructure --startup-project CigralBackend
```

#### 3. Agregar nuevas migraciones (cuando cambies el modelo)

```bash
dotnet ef migrations add NombreDeLaMigracion --project CigralBackend.Infraestructure --startup-project CigralBackend
dotnet ef database update --project CigralBackend.Infraestructure --startup-project CigralBackend
```

#### 4. Revertir una migracion

```bash
dotnet ef database update NombreMigracionAnterior --project CigralBackend.Infraestructure --startup-project CigralBackend
```

#### 5. Eliminar la ultima migracion

```bash
dotnet ef migrations remove --project CigralBackend.Infraestructure --startup-project CigralBackend
```

## Caracteristicas Implementadas

### 1. DbContext Configurado

- **CigralBackendContext**: Contexto de Entity Framework con todas las entidades del dominio
- Configuracion completa de relaciones entre entidades
- Restricciones de longitud en campos de texto
- Configuracion de tipos decimales para precios

### 2. Repositorio Generico con Paginacion

El `EfRepository` implementa:

- **Add**: Agregar nuevas entidades
- **Update**: Actualizar entidades existentes
- **Delete**: Eliminar entidades
- **GetById**: Obtener por ID con includes opcionales
- **First**: Obtener la primera coincidencia de un filtro
- **GetAll**: Obtener todos los registros con **paginacion**
- **GetFiltered**: Obtener registros filtrados con **paginacion**

#### Ejemplo de uso de paginacion:

```csharp
// Obtener la pagina 2 con 20 elementos por pagina
var result = await _repository.GetAll<Producto>(pageNumber: 2, pageSize: 20, "Lotes");

Console.WriteLine($"Total de productos: {result.TotalCount}");
Console.WriteLine($"Pagina {result.PageNumber} de {result.TotalPages}");
Console.WriteLine($"Tiene pagina anterior: {result.HasPreviousPage}");
Console.WriteLine($"Tiene pagina siguiente: {result.HasNextPage}");

foreach (var producto in result.Items)
{
    Console.WriteLine(producto.Nombre);
}
```

### 3. Entidades del Dominio

Todas las entidades heredan de `EntityBase` y tienen `Id` de tipo `Guid`:

- **Cliente**: Informacion de clientes
- **Proveedor**: Informacion de proveedores
- **Producto**: Catalogo de productos
- **Lote**: Lotes de productos con vencimiento
- **Deposito**: Almacenes o depositos
- **Existencia**: Stock de productos por deposito
- **DetalleRemito**: Lineas de items en remitos
- **RemitoCliente**: Remitos de salida a clientes
- **RemitoProveedor**: Remitos de entrada de proveedores

### 4. DTOs y Modelos

- **DTOs**: Representacion de datos para transferencia
- **Request Models**: Modelos con validaciones para crear/actualizar entidades
- **Response Models**: Modelos para respuestas de la API

## Inyeccion de Dependencias

El `Program.cs` ya esta configurado con:

```csharp
// DbContext
builder.Services.AddDbContext<CigralBackendContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositorio
builder.Services.AddScoped<IRepository, EfRepository>();
```

## Proximos Pasos

1. Crear las migraciones con los comandos anteriores
2. Implementar servicios de aplicacion que usen el repositorio
3. Crear controladores que consuman los servicios
4. Agregar AutoMapper para mapeo entre entidades y DTOs

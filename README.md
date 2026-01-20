# CigralBackend - Sistema de Gestión de Inventario y Remitos

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![C#](https://img.shields.io/badge/C%23-12.0-green)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-8.0-orange)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-red)
![Private](https://img.shields.io/badge/Repository-Private-red)

Sistema backend para la gestión de inventario, productos, clientes, proveedores y remitos, desarrollado con arquitectura en capas siguiendo principios de Clean Architecture.

## ?? Tabla de Contenidos

- [Características](#-características)
- [Arquitectura](#-arquitectura)
- [Tecnologías](#-tecnologías)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Configuración Inicial](#?-configuración-inicial)
- [Migraciones de Base de Datos](#?-migraciones-de-base-de-datos)
- [Uso del API](#-uso-del-api)
- [Modelos de Dominio](#-modelos-de-dominio)
- [Paginación](#-paginación)
- [Documentación](#-documentación)

## ?? Características

- ? **Gestión de Productos**: CRUD completo con soporte de lotes y GTIN
- ? **Control de Inventario**: Seguimiento de existencias por depósito
- ? **Gestión de Clientes y Proveedores**: Con GLN (Global Location Number)
- ? **Remitos**: Entrada (proveedores) y salida (clientes)
- ? **Paginación**: Soporte integrado en todas las consultas
- ? **Validaciones**: Data Annotations en todos los modelos
- ? **API RESTful**: Endpoints documentados con Swagger
- ? **Entity Framework Core**: ORM con SQL Server
- ? **Patrón Repository**: Abstracción de acceso a datos

## ??? Arquitectura

El proyecto sigue una arquitectura en capas basada en Clean Architecture:

```
???????????????????????????????????????????
?         CigralBackend.Api               ?  ? Capa de Presentación (Controllers)
???????????????????????????????????????????
?     CigralBackend.Application           ?  ? Capa de Aplicación (Services, DTOs)
???????????????????????????????????????????
?       CigralBackend.Domain              ?  ? Capa de Dominio (Entidades)
???????????????????????????????????????????
?   CigralBackend.Infrastructure          ?  ? Capa de Infraestructura (EF, DB)
???????????????????????????????????????????
```

### Responsabilidades por Capa

- **API**: Controladores, configuración de servicios, middleware
- **Application**: Servicios de negocio, DTOs, modelos de validación
- **Domain**: Entidades del dominio, reglas de negocio
- **Infrastructure**: Implementación de repositorios, DbContext, acceso a datos

**Más información**: Ver [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)

## ??? Tecnologías

- **Framework**: .NET 8.0
- **Lenguaje**: C# 12.0
- **ORM**: Entity Framework Core 8.0
- **Base de Datos**: SQL Server 2019+
- **API Documentation**: Swagger/OpenAPI
- **Validación**: Data Annotations
- **Inyección de Dependencias**: Built-in DI Container

## ?? Estructura del Proyecto

```
CigralBackend/
??? CigralBackend.Api/                 # Web API
?   ??? Controllers/                   # Controladores REST
?   ??? Program.cs                     # Configuración de la aplicación
?   ??? appsettings.json              # Configuración (conexión DB)
?
??? CigralBackend.Application/         # Lógica de aplicación
?   ??? Dtos/                         # Data Transfer Objects
?   ?   ??? *Dto.cs                   # DTOs básicos
?   ?   ??? *Model.cs                 # Modelos con validaciones
?   ?   ??? *Requests.cs              # Request/Response records
?   ??? Services/                     # Servicios de negocio
?       ??? Interfaces/
?       ??? *.Service.cs
?
??? CigralBackend.Domain/              # Dominio
?   ??? Bases/                        # Clases base
?   ?   ??? EntityBase.cs             # Entidad base con Id
?   ?   ??? RemitoBase.cs             # Base para remitos
?   ??? Cliente.cs
?   ??? Proveedor.cs
?   ??? Producto.cs
?   ??? Lote.cs
?   ??? Deposito.cs
?   ??? Existencia.cs
?   ??? DetalleRemito.cs
?   ??? RemitoCliente.cs
?   ??? RemitoProveedor.cs
?
??? CigralBackend.Infrastructure/      # Infraestructura
    ??? Database/
        ??? CigralBackendContext.cs   # DbContext
        ??? EfRepository.cs           # Implementación del repositorio
        ??? Interfaces/
            ??? IRepository.cs        # Interfaz del repositorio
```

## ?? Configuración Inicial

### Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server 2019+](https://www.microsoft.com/sql-server) o SQL Server Express
- IDE recomendado: Visual Studio 2022 o VS Code

### ?? Inicio Rápido

Ver [docs/QUICK_START.md](docs/QUICK_START.md) para una guía de 5 minutos.

### Instalación Completa

1. **Clonar el Repositorio**

```bash
git clone https://github.com/lucasmillan17/cigral-backend.git
cd cigral-backend/CigralBackend
```

2. **Configurar Cadena de Conexión**

Edita `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=CigralBackendDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

3. **Restaurar Paquetes**

```bash
dotnet restore
```

4. **Crear Base de Datos**

```bash
dotnet ef migrations add InitialCreate --project ..\CigralBackend.Infraestructure --startup-project .
dotnet ef database update --project ..\CigralBackend.Infraestructure --startup-project .
```

5. **Ejecutar**

```bash
dotnet run
```

Abre: `https://localhost:5001/swagger`

## ??? Migraciones de Base de Datos

Ver [DATABASE_SETUP.md](DATABASE_SETUP.md) para la guía completa de base de datos.

### Comandos Principales

```bash
# Crear migración
dotnet ef migrations add NombreMigracion --project ..\CigralBackend.Infraestructure --startup-project .

# Aplicar migraciones
dotnet ef database update --project ..\CigralBackend.Infraestructure --startup-project .

# Listar migraciones
dotnet ef migrations list --project ..\CigralBackend.Infraestructure --startup-project .
```

## ?? Uso del API

### Swagger UI

Accede a `https://localhost:5001/swagger` para la documentación interactiva.

### Ejemplo: Crear un Producto

```http
POST /api/products
Content-Type: application/json

{
  "nombre": "Producto Ejemplo",
  "descripcion": "Descripción del producto",
  "gtin": "7891234567890",
  "esUnitario": true,
  "precio": 1500.50
}
```

### Ejemplo: Paginación

```http
GET /api/products?pageNumber=1&pageSize=20
```

Respuesta:
```json
{
  "items": [...],
  "totalCount": 150,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 8,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

## ?? Modelos de Dominio

### Entidades Principales

| Entidad | Descripción |
|---------|-------------|
| **Cliente** | Información de clientes con GLN |
| **Proveedor** | Información de proveedores con GLN |
| **Producto** | Catálogo de productos con GTIN |
| **Lote** | Lotes de productos con vencimiento |
| **Deposito** | Almacenes o depósitos |
| **Existencia** | Stock de productos por depósito |
| **DetalleRemito** | Líneas de items en remitos |
| **RemitoCliente** | Remitos de salida a clientes |
| **RemitoProveedor** | Remitos de entrada de proveedores |

Ver estructura completa en [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)

## ?? Paginación

Todas las consultas soportan paginación:

```csharp
var result = await _repository.GetAll<Producto>(pageNumber: 1, pageSize: 20);

// PagedResult incluye:
result.Items              // Lista de elementos
result.TotalCount         // Total de registros
result.TotalPages         // Total de páginas
result.HasPreviousPage    // ¿Tiene anterior?
result.HasNextPage        // ¿Tiene siguiente?
```

## ?? Documentación

### Documentación Completa

| Documento | Descripción |
|-----------|-------------|
| [QUICK_START.md](docs/QUICK_START.md) | Guía de inicio rápido (5 min) |
| [ARCHITECTURE.md](docs/ARCHITECTURE.md) | Arquitectura del sistema |
| [DEVELOPMENT.md](docs/DEVELOPMENT.md) | Guía de desarrollo |
| [DATABASE_SETUP.md](DATABASE_SETUP.md) | Configuración de BD |
| [INDEX.md](docs/INDEX.md) | Índice de documentación |

### Comentarios XML

Todas las interfaces y clases públicas incluyen documentación XML:

```csharp
/// <summary>
/// Obtiene un producto por su identificador único.
/// </summary>
/// <param name="id">El identificador del producto</param>
/// <returns>El producto encontrado o null</returns>
public async Task<Producto?> GetProductoById(Guid id)
```

## ?? Inyección de Dependencias

```csharp
// Program.cs
builder.Services.AddDbContext<CigralBackendContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IRepository, EfRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>();
```

## ?? Roadmap

- [ ] Implementar AutoMapper para mapeo de DTOs
- [ ] Agregar autenticación y autorización (JWT)
- [ ] Implementar logging con Serilog
- [ ] Agregar validaciones de negocio personalizadas
- [ ] Tests unitarios y de integración
- [ ] Caché con Redis
- [ ] Dockerización
- [ ] CI/CD Pipeline

## ?? Equipo de Desarrollo

- **Lucas Millan** - Desarrollador Principal

## ?? Contacto

Para preguntas o consultas sobre el proyecto, contactar al equipo de desarrollo.

## ?? Notas del Proyecto

Este es un **repositorio privado** para uso interno del equipo de desarrollo.

### Convenciones de Código

- Seguir las convenciones de C# y .NET
- Usar nombres descriptivos en español para el dominio
- Documentar métodos públicos con comentarios XML
- Mantener la separación de responsabilidades por capa
- Usar Conventional Commits para mensajes

### Branches

- `main` - Producción
- `development` - Desarrollo principal
- `feature/*` - Nuevas funcionalidades
- `bugfix/*` - Correcciones de bugs
- `hotfix/*` - Correcciones urgentes

---

**CigralBackend** © 2025 - Sistema de Gestión de Inventario y Remitos

# CigralBackend - Sistema de Gestion de Inventario y Remitos

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![C#](https://img.shields.io/badge/C%23-12.0-green)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-8.0-orange)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-red)
![Private](https://img.shields.io/badge/Repository-Private-red)

Sistema backend para la gestion de inventario, productos, clientes, proveedores y remitos, desarrollado con arquitectura en capas siguiendo principios de Clean Architecture.

## Tabla de Contenidos

- [Caracteristicas](#caracteristicas)
- [Arquitectura](#arquitectura)
- [Tecnologias](#tecnologias)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Configuracion Inicial](#configuracion-inicial)
- [Migraciones de Base de Datos](#migraciones-de-base-de-datos)
- [Uso del API](#uso-del-api)
- [Modelos de Dominio](#modelos-de-dominio)
- [Paginacion](#paginacion)
- [Documentacion](#documentacion)
- [Estadisticas del Proyecto](#estadisticas-del-proyecto)

## Caracteristicas

- **Gestion de Productos**: CRUD completo con soporte de lotes y GTIN
- **Gestion de Marcas**: CRUD completo con validaciones
- **Control de Inventario**: Seguimiento de existencias por deposito con validaciones avanzadas
- **Gestion de Clientes y Proveedores**: Con GLN (Global Location Number)
- **Remitos**: Entrada (proveedores) y salida (clientes)
- **Sistema de Excepciones**: Manejo robusto de errores con códigos tipados
- **Validaciones de Negocio**: Control de lotes vencidos, números de serie únicos, productos unitarios
- **Paginacion**: Soporte integrado en todas las consultas
- **Validaciones**: Data Annotations en todos los modelos
- **API RESTful**: Endpoints documentados con Swagger
- **Entity Framework Core**: ORM con SQL Server
- **Patron Repository**: Abstraccion de acceso a datos
- **Tests Unitarios**: 75 tests con 100% de cobertura en servicios críticos

## Arquitectura

El proyecto sigue una arquitectura en capas basada en Clean Architecture:

```
+------------------------------------------+
|         CigralBackend.Api                |  <- Capa de Presentacion (Controllers)
+------------------------------------------+
|     CigralBackend.Application            |  <- Capa de Aplicacion (Services, DTOs)
+------------------------------------------+
|       CigralBackend.Domain               |  <- Capa de Dominio (Entidades)
+------------------------------------------+
|   CigralBackend.Infrastructure           |  <- Capa de Infraestructura (EF, DB)
+------------------------------------------+
```

### Responsabilidades por Capa

- **API**: Controladores, configuracion de servicios, middleware
- **Application**: Servicios de negocio, DTOs, modelos de validacion
- **Domain**: Entidades del dominio, reglas de negocio
- **Infrastructure**: Implementacion de repositorios, DbContext, acceso a datos

**Mas informacion**: Ver [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)

## Tecnologias

- **Framework**: .NET 8.0
- **Lenguaje**: C# 12.0
- **ORM**: Entity Framework Core 8.0
- **Base de Datos**: SQL Server 2019+
- **API Documentation**: Swagger/OpenAPI
- **Validacion**: Data Annotations
- **Inyeccion de Dependencias**: Built-in DI Container

## Estructura del Proyecto

```
CigralBackend/
|-- CigralBackend.Api/                 # Web API
|   |-- Controllers/                   # Controladores REST
|   |-- Program.cs                     # Configuracion de la aplicacion
|   +-- appsettings.json              # Configuracion (conexion DB)
|
|-- CigralBackend.Application/         # Logica de aplicacion
|   |-- Dtos/                         # Data Transfer Objects
|   |   |-- *Dto.cs                   # DTOs basicos
|   |   |-- *Model.cs                 # Modelos con validaciones
|   |   +-- *Requests.cs              # Request/Response records
|   +-- Services/                     # Servicios de negocio
|       |-- Interfaces/
|       +-- *.Service.cs
|
|-- CigralBackend.Domain/              # Dominio
|   |-- Bases/                        # Clases base
|   |   |-- EntityBase.cs             # Entidad base con Id
|   |   +-- RemitoBase.cs             # Base para remitos
|   |-- Cliente.cs
|   |-- Proveedor.cs
|   |-- Producto.cs
|   |-- Lote.cs
|   |-- Deposito.cs
|   |-- Existencia.cs
|   |-- DetalleRemito.cs
|   |-- RemitoCliente.cs
|   +-- RemitoProveedor.cs
|
+-- CigralBackend.Infrastructure/      # Infraestructura
    +-- Database/
        |-- CigralBackendContext.cs   # DbContext
        |-- EfRepository.cs           # Implementacion del repositorio
        +-- Interfaces/
            +-- IRepository.cs        # Interfaz del repositorio
```

## Configuracion Inicial

### Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server 2019+](https://www.microsoft.com/sql-server) o SQL Server Express
- IDE recomendado: Visual Studio 2022 o VS Code

### Inicio Rapido

Ver [docs/QUICK_START.md](docs/QUICK_START.md) para una guia de 5 minutos.

### Instalacion Completa

1. **Clonar el Repositorio**

```bash
git clone https://github.com/lucasmillan17/cigral-backend.git
cd cigral-backend/CigralBackend
```

2. **Configurar Cadena de Conexion**

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

## Migraciones de Base de Datos

Ver [DATABASE_SETUP.md](DATABASE_SETUP.md) para la guia completa de base de datos.

### Comandos Principales

```bash
# Crear migracion
dotnet ef migrations add NombreMigracion --project ..\CigralBackend.Infraestructure --startup-project .

# Aplicar migraciones
dotnet ef database update --project ..\CigralBackend.Infraestructure --startup-project .

# Listar migraciones
dotnet ef migrations list --project ..\CigralBackend.Infraestructure --startup-project .
```

## Uso del API

### Swagger UI

Accede a `https://localhost:5001/swagger` para la documentacion interactiva.

### Ejemplo: Crear un Producto

```http
POST /api/products
Content-Type: application/json

{
  "nombre": "Producto Ejemplo",
  "descripcion": "Descripcion del producto",
  "gtin": "7891234567890",
  "esUnitario": true,
  "precio": 1500.50
}
```

### Ejemplo: Paginacion

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

## Modelos de Dominio

### Entidades Principales

| Entidad | Descripcion | Estado |
|---------|-------------|--------|
| **Cliente** | Informacion de clientes con GLN | ? Implementado |
| **Proveedor** | Informacion de proveedores con GLN | ? Implementado |
| **Producto** | Catalogo de productos con GTIN | ? CRUD + Tests |
| **Marca** | Marcas de productos | ? CRUD + Tests |
| **Lote** | Lotes de productos con vencimiento | ? Implementado |
| **Deposito** | Almacenes o depositos | ? Implementado |
| **Existencia** | Stock de productos por deposito | ? CRUD + Tests |
| **DetalleRemito** | Lineas de items en remitos | ? Implementado |
| **RemitoCliente** | Remitos de salida a clientes | ? Implementado |
| **RemitoProveedor** | Remitos de entrada de proveedores | ? Implementado |

Ver estructura completa en [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)

## Paginacion

Todas las consultas soportan paginacion:

```csharp
var result = await _repository.GetAll<Producto>(pageNumber: 1, pageSize: 20);

// PagedResult incluye:
result.Items              // Lista de elementos
result.TotalCount         // Total de registros
result.TotalPages         // Total de paginas
result.HasPreviousPage    // Tiene anterior?
result.HasNextPage        // Tiene siguiente?
```

## Documentacion

### Documentacion Completa

| Documento | Descripcion |
|-----------|-------------|
| [QUICK_START.md](docs/QUICK_START.md) | Guia de inicio rapido (5 min) |
| [ARCHITECTURE.md](docs/ARCHITECTURE.md) | Arquitectura del sistema |
| [DEVELOPMENT.md](docs/DEVELOPMENT.md) | Guia de desarrollo |
| [DATABASE_SETUP.md](DATABASE_SETUP.md) | Configuracion de BD |
| [INDEX.md](docs/INDEX.md) | Indice de documentacion |

### Comentarios XML

Todas las interfaces y clases publicas incluyen documentacion XML:

```csharp
/// <summary>
/// Obtiene un producto por su identificador unico.
/// </summary>
/// <param name="id">El identificador del producto</param>
/// <returns>El producto encontrado o null</returns>
public async Task<Producto?> GetProductoById(Guid id)
```

## Inyeccion de Dependencias

```csharp
// Program.cs
builder.Services.AddDbContext<CigralBackendContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IRepository, EfRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>();
```

## Estadisticas del Proyecto

### Código de Producción
- **Servicios**: 4 (BarCodeParser, ProductoService, MarcaService, ExistenciaService)
- **Controladores**: 3 (ProductsController, MarcasController, ExistenciasController)
- **Excepciones**: 2 (NotFoundException, DomainException)
- **Middlewares**: 1 (ExceptionHandlingMiddleware)
- **Códigos de error**: 29
- **Endpoints REST**: 18 (6 productos + 6 marcas + 6 existencias)

### Documentación
- **Archivos**: 7 documentos completos
- **Páginas**: ~40 páginas de documentación
- **Ejemplos de código**: 80+

## Roadmap

### Completado ?
- [x] Sistema robusto de manejo de errores
- [x] ProductoService CRUD completo con tests
- [x] MarcaService CRUD completo con tests
- [x] ExistenciaService CRUD completo con tests
- [x] BarCodeParser con tests completos
- [x] Middleware de excepciones global
- [x] 75 tests unitarios (100% cobertura en servicios críticos)
- [x] Documentación completa

### En Progreso ??
- [ ] Implementar AutoMapper para mapeo de DTOs
- [ ] Tests de integración con base de datos real
- [ ] Tests de controladores

### Planificado ??
- [ ] ClienteService CRUD completo
- [ ] ProveedorService CRUD completo
- [ ] LoteService CRUD completo
- [ ] DepositoService CRUD completo
- [ ] Agregar autenticacion y autorizacion (JWT)
- [ ] Implementar logging con Serilog
- [ ] Agregar validaciones de negocio personalizadas con FluentValidation
- [ ] Tests E2E
- [ ] Cache con Redis
- [ ] Dockerizacion
- [ ] CI/CD Pipeline

## Equipo de Desarrollo

- **Lucas Millan** - Desarrollador Principal

## Contacto

Para preguntas o consultas sobre el proyecto, contactar al equipo de desarrollo.

## Notas del Proyecto

Este es un **repositorio privado** para uso interno del equipo de desarrollo.

### Convenciones de Codigo

- Seguir las convenciones de C# y .NET
- Usar nombres descriptivos en espanol para el dominio
- Documentar metodos publicos con comentarios XML
- Mantener la separacion de responsabilidades por capa
- Usar Conventional Commits para mensajes

### Branches

- `main` - Produccion
- `development` - Desarrollo principal
- `feature/*` - Nuevas funcionalidades
- `bugfix/*` - Correcciones de bugs
- `hotfix/*` - Correcciones urgentes

---

**CigralBackend** (c) 2025 - Sistema de Gestion de Inventario y Remitos

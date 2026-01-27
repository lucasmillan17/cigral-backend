# ?? Documentación CigralBackend v1.0

> Sistema de Gestión de Inventario con .NET 8 - Clean Architecture

---

## ?? Índice

1. [Inicio Rápido](#-inicio-rápido)
2. [Arquitectura](#-arquitectura)
3. [Funcionalidades Principales](#-funcionalidades-principales)
4. [API Endpoints](#-api-endpoints)
5. [Manejo de Errores](#-manejo-de-errores)
6. [Autenticación](#-autenticación)
7. [Despliegue](#-despliegue)

---

## ?? Inicio Rápido

### Requisitos Previos
- .NET 8 SDK
- SQL Server 2019+
- Visual Studio 2022 o VS Code

### Configuración Inicial

```bash
# 1. Clonar repositorio
git clone https://github.com/lucasmillan17/cigral-backend.git
cd cigral-backend

# 2. Configurar connection string
# Editar: CigralBackend/appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CigralDB;Trusted_Connection=True;"
  }
}

# 3. Crear base de datos
cd CigralBackend
dotnet ef database update --project ..\CigralBackend.Infraestructure

# 4. Ejecutar
dotnet run

# 5. Abrir Swagger
# https://localhost:5001/swagger
```

### Crear Usuario Admin

```bash
POST /api/auth/register
{
  "nombreCompleto": "Admin",
  "username": "admin",
  "email": "admin@cigral.com",
  "password": "Admin123!",
  "esAdmin": true
}
```

---

## ??? Arquitectura

### Clean Architecture - Capas

```
???????????????????????????????????????????
?          API Layer (Controllers)        ?
?  - ProductosController                  ?
?  - MarcasController                     ?
?  - ExistenciasController                ?
?  - RemitosController                    ?
?  - AuthController                       ?
???????????????????????????????????????????
                  ?
                  ?
???????????????????????????????????????????
?      Application Layer (Services)       ?
?  - ProductoService                      ?
?  - MarcaService                         ?
?  - ExistenciaService                    ?
?  - RemitoService                        ?
?  - AuthService                          ?
???????????????????????????????????????????
                  ?
                  ?
???????????????????????????????????????????
?        Domain Layer (Entities)          ?
?  - Producto, Marca, Cliente             ?
?  - Existencia, Lote                     ?
?  - RemitoIngreso, RemitoEgreso          ?
?  - Exceptions, DomainErrorCode          ?
???????????????????????????????????????????
                  ?
                  ?
???????????????????????????????????????????
?   Infrastructure (Database, Services)   ?
?  - EfRepository (EF Core)               ?
?  - PdfService (QuestPDF)                ?
?  - BarCodeParser (GS1)                  ?
???????????????????????????????????????????
```

### Principios Aplicados
- ? **SOLID** - Todos los principios
- ? **DRY** - Sin código duplicado
- ? **KISS** - Código simple y directo
- ? **Fail-Fast** - Validaciones tempranas
- ? **Clean Code** - Nombres descriptivos

---

## ?? Funcionalidades Principales

### 1. Gestión de Productos
- ? CRUD completo
- ? Validación de GTIN (código de barras)
- ? Productos unitarios vs. fraccionables
- ? Asignación de marcas
- ? Búsqueda y filtrado

### 2. Control de Inventario (Existencias)
- ? Stock por producto/depósito/lote
- ? Números de serie
- ? Fechas de vencimiento
- ? Aumentar/Disminuir stock
- ? Auditoría de movimientos
- ? **Dashboard de vencimientos** ??

### 3. Remitos
- ? Remitos de Ingreso (compras)
- ? Remitos de Egreso (ventas)
- ? Movimientos automáticos de stock
- ? **Generación de PDF** ??
- ? Asociación con clientes/proveedores

### 4. Sistema de Vencimientos
- ? Dashboard con rangos (0-30, 31-60, 61-90 días)
- ? Filtros por depósito/producto
- ? Estadísticas agrupadas
- ? Alertas de productos críticos

### 5. Generación de PDFs
- ? PDFs profesionales de remitos
- ? Plantilla A4 con QuestPDF
- ? Información completa (cliente, productos, totales)
- ? Secciones de firmas

### 6. Parser de Códigos GS1
- ? Parseo de códigos de barras GS1
- ? Extracción de GTIN, lote, vencimiento, serie
- ? Validación de formato

---

## ?? API Endpoints

### Autenticación

```http
POST   /api/auth/register         # Registrar usuario
POST   /api/auth/login            # Login (retorna JWT)
```

### Productos

```http
GET    /api/products              # Listar (paginado)
GET    /api/products/{id}         # Obtener por ID
POST   /api/products              # Crear
PUT    /api/products/{id}         # Actualizar
DELETE /api/products/{id}         # Eliminar
```

**Ejemplo Request:**
```json
POST /api/products
{
  "nombre": "Paracetamol 500mg",
  "descripcion": "Analgésico",
  "gtin": "7790123456789",
  "esUnitario": false,
  "marcaId": 1
}
```

### Existencias

```http
GET    /api/existencias                    # Listar con filtros
GET    /api/existencias/{id}               # Obtener por ID
POST   /api/existencias/aumentar           # Aumentar stock
POST   /api/existencias/disminuir          # Disminuir stock
DELETE /api/existencias/{id}               # Eliminar
```

**Filtros disponibles:**
- `depositoId` - Por depósito
- `productoId` - Por producto
- `loteId` - Por lote
- `diasParaVencer` - Productos que vencen en X días
- `fechaVencimientoDesde/Hasta` - Rango de fechas
- `soloConVencimiento` - true/false

**Ejemplo - Productos que vencen en 30 días:**
```http
GET /api/existencias?diasParaVencer=30
```

### Dashboard de Vencimientos

```http
GET    /api/existencias/dashboard/vencimientos    # Dashboard con rangos
GET    /api/existencias/proximos-vencer           # Query personalizada
```

**Response Dashboard:**
```json
{
  "fechaConsulta": "2025-01-27",
  "totalProductosProximosVencer": 156,
  "totalLotesProximosVencer": 45,
  "cantidadTotalProximaVencer": 2340,
  "rangos": [
    {
      "rango": "0-30 días",
      "totalProductos": 12,
      "totalLotes": 8,
      "cantidadTotal": 345,
      "items": [...]
    }
  ]
}
```

### Remitos

```http
POST   /api/remitos/ingreso                # Crear remito de ingreso
POST   /api/remitos/egreso                 # Crear remito de egreso
PUT    /api/remitos/ingreso/{id}           # Actualizar
PUT    /api/remitos/egreso/{id}            # Actualizar
GET    /api/remitos/ingreso/{id}/pdf       # ?? Generar PDF
GET    /api/remitos/egreso/{id}/pdf        # ?? Generar PDF
```

**Ejemplo - Crear Remito:**
```json
POST /api/remitos/ingreso
{
  "depositoId": 1,
  "entidadId": 5,
  "numeroRemito": "RI-001",
  "observaciones": "Ingreso enero 2025",
  "detalles": [
    {
      "productoId": 10,
      "loteId": 3,
      "cantidad": 100
    }
  ]
}
```

---

## ?? Manejo de Errores

### Sistema de Excepciones Tipadas

**3 Tipos de Excepciones:**

1. **NotFoundException** (404)
   ```json
   {
     "error": "NotFound",
     "message": "La entidad Producto (999) no fue encontrada.",
     "statusCode": 404,
     "timestamp": "2025-01-27T10:30:00Z",
     "details": {
       "entityName": "Producto",
       "key": 999
     }
   }
   ```

2. **DomainException** (400)
   ```json
   {
     "error": "DomainError",
     "message": "El producto con GTIN 7790123456789 ya existe.",
     "statusCode": 400,
     "timestamp": "2025-01-27T10:30:00Z",
     "details": {
       "code": "GtinDuplicado",
       "codeValue": 2001
     }
   }
   ```

3. **Exception** (500)
   ```json
   {
     "error": "InternalServerError",
     "message": "Ocurrió un error inesperado.",
     "statusCode": 500,
     "timestamp": "2025-01-27T10:30:00Z"
   }
   ```

### Códigos de Error

| Rango | Categoría | Códigos |
|-------|-----------|---------|
| 1000-1999 | Generales | UnknownError, NetworkError |
| 2000-2999 | Productos | GtinDuplicado, MarcaNoValida |
| 3000-3999 | Inventario | StockInsuficiente, LoteVencido |
| 4000-4999 | Clientes | ClienteNoExiste, CuitDuplicado |
| 5000-5999 | Proveedores | ProveedorNoExiste, GlnDuplicado |
| 6000-6999 | Remitos | NumeroRemitoDuplicado, RemitoSinDetalles |

**Ver lista completa:** `Domain/Enums/DomainErrorCode.cs`

---

## ?? Autenticación

### JWT (JSON Web Tokens)

**1. Login:**
```http
POST /api/auth/login
{
  "username": "admin",
  "password": "Admin123!"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiration": "2025-01-28T10:00:00Z",
  "user": {
    "id": 1,
    "username": "admin",
    "nombreCompleto": "Admin",
    "esAdmin": true
  }
}
```

**2. Usar Token:**
```http
GET /api/products
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

**Configuración JWT:**
```json
// appsettings.json
{
  "Jwt": {
    "Key": "tu-clave-super-secreta-de-al-menos-32-caracteres",
    "Issuer": "CigralBackend",
    "Audience": "CigralBackend",
    "ExpirationMinutes": 60
  }
}
```

---

## ?? Despliegue

### Producción

**1. Configurar appsettings.Production.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=CigralDB;User=sa;Password=***;"
  },
  "Jwt": {
    "Key": "production-secret-key-change-this",
    "ExpirationMinutes": 60
  }
}
```

**2. Publicar:**
```bash
dotnet publish -c Release -o ./publish
```

**3. Migrar Base de Datos:**
```bash
dotnet ef database update --project CigralBackend.Infraestructure
```

**4. Ejecutar:**
```bash
cd publish
dotnet CigralBackend.Api.dll
```

### Docker (Opcional)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "CigralBackend.Api.dll"]
```

---

## ?? Estadísticas v1.0

### Código
- **Proyectos**: 4 (Api, Application, Domain, Infrastructure)
- **Servicios**: 8 (Producto, Marca, Existencia, Remito, etc.)
- **Controladores**: 5
- **Endpoints**: 25+
- **Entidades**: 12

### Funcionalidades
- ? CRUD de Productos/Marcas
- ? Control de Inventario completo
- ? Remitos de Ingreso/Egreso
- ? Dashboard de Vencimientos
- ? Generación de PDFs
- ? Autenticación JWT
- ? Auditoría de Movimientos
- ? Parser GS1

---

## ?? Próximos Pasos (Roadmap v2.0)

### Funcionalidades Planeadas
- [ ] Reportes avanzados
- [ ] Notificaciones por email
- [ ] Exportación a Excel
- [ ] Gestión de usuarios y roles
- [ ] Dashboard analítico
- [ ] App móvil (Flutter)

---

## ?? Soporte

### Documentación Adicional

- **Arquitectura Detallada:** `docs/ARCHITECTURE.md`
- **Desarrollo:** `docs/DEVELOPMENT.md`
- **Vencimientos:** `docs/DASHBOARD_VENCIMIENTOS.md`
- **PDFs:** `docs/PDF_GENERATION.md`

### Contacto

- **Repository:** https://github.com/lucasmillan17/cigral-backend
- **Issues:** https://github.com/lucasmillan17/cigral-backend/issues

---

**CigralBackend v1.0** - Sistema de Gestión de Inventario
**Última actualización:** 27 de Enero, 2025

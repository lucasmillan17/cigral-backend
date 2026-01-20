# ?? Quick Start Guide - CigralBackend

¿Quieres poner el proyecto en marcha rápidamente? Sigue esta guía de 5 minutos.

## ? Setup Rápido

### 1. Prerrequisitos (2 minutos)

Verifica que tengas instalado:

```bash
# .NET 8 SDK
dotnet --version
# Debe mostrar 8.0.x

# SQL Server (cualquier versión funciona)
# Windows: Abre SQL Server Management Studio
# O verifica que el servicio esté corriendo
```

### 2. Clonar y Configurar (1 minuto)

```bash
# Clonar
git clone https://github.com/lucasmillan17/cigral-backend.git
cd cigral-backend/CigralBackend

# Configurar connection string
# Edita appsettings.json y cambia "localhost" por tu servidor SQL
```

**Editar `appsettings.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=CigralBackendDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Ejemplos de connection strings:
- Local: `Server=localhost`
- SQL Express: `Server=.\\SQLEXPRESS`
- Con credenciales: `Server=localhost;Database=CigralBackendDB;User Id=sa;Password=tuPassword;TrustServerCertificate=True`

### 3. Crear Base de Datos (1 minuto)

```bash
# Crear migración inicial
dotnet ef migrations add InitialCreate --project ..\CigralBackend.Infraestructure --startup-project .

# Crear base de datos
dotnet ef database update --project ..\CigralBackend.Infraestructure --startup-project .
```

### 4. ¡Ejecutar! (30 segundos)

```bash
dotnet run
```

Abre tu navegador en: **https://localhost:5001/swagger**

?? **¡Listo! Ya tienes el proyecto corriendo.**

---

## ?? ¿Y ahora qué?

### Explorar el API

En Swagger UI puedes:

1. **Ver todos los endpoints** disponibles
2. **Probar** las operaciones directamente
3. **Ver** los modelos de datos

### Primer Request

**Crear un producto:**

1. En Swagger, busca `POST /api/products`
2. Click en "Try it out"
3. Modifica el JSON:

```json
{
  "nombre": "Mi Primer Producto",
  "descripcion": "Producto de prueba",
  "gtin": "1234567890123",
  "esUnitario": true,
  "precio": 100.50
}
```

4. Click en "Execute"
5. ¡Verás tu producto creado! ?

### Ver los Datos

**Opción 1 - Desde Swagger:**
- Usa `GET /api/products?pageNumber=1&pageSize=10`

**Opción 2 - SQL Server:**
```sql
USE CigralBackendDB
SELECT * FROM Productos
```

---

## ?? Siguiente Nivel

Ya tienes el proyecto funcionando. Ahora puedes:

### Aprender más sobre el proyecto:
- ?? [README.md](../README.md) - Vista general completa
- ??? [docs/ARCHITECTURE.md](ARCHITECTURE.md) - Entender la arquitectura
- ?? [docs/DEVELOPMENT.md](DEVELOPMENT.md) - Guía de desarrollo

### Explorar el código:
```
CigralBackend.Domain/        ? Entidades de negocio
CigralBackend.Application/   ? Servicios y DTOs
CigralBackend.Infrastructure/ ? Base de datos
CigralBackend.Api/           ? Controllers
```

### Agregar tu primera feature:
- Sigue la guía en [DEVELOPMENT.md - Agregar Nueva Funcionalidad](DEVELOPMENT.md#agregar-nueva-funcionalidad)

---

## ?? ¿Problemas?

### Error: "Cannot open database"

**Solución:**
1. Verifica que SQL Server esté corriendo
2. Verifica el connection string en `appsettings.json`
3. Intenta con un usuario y contraseña en lugar de Trusted_Connection

### Error: "dotnet ef not found"

**Solución:**
```bash
dotnet tool install --global dotnet-ef
```

### Error: Compilación fallida

**Solución:**
```bash
# Restaurar paquetes
dotnet restore

# Limpiar y recompilar
dotnet clean
dotnet build
```

### Otros problemas:

- Revisa [docs/INDEX.md](INDEX.md) - FAQ
- Abre un [Issue en GitHub](https://github.com/lucasmillan17/cigral-backend/issues)

---

## ?? Entidades Disponibles

El proyecto incluye estas entidades listas para usar:

| Entidad | Descripción | Endpoint |
|---------|-------------|----------|
| **Productos** | Catálogo de productos | `/api/products` |
| **Clientes** | Gestión de clientes | `/api/clientes` |
| **Proveedores** | Gestión de proveedores | `/api/proveedores` |
| **Lotes** | Lotes de productos | `/api/lotes` |
| **Depósitos** | Almacenes | `/api/depositos` |
| **Existencias** | Stock por depósito | `/api/existencias` |
| **Remitos** | Remitos de entrada/salida | `/api/remitos` |

*(Nota: Algunos endpoints pueden estar en desarrollo)*

---

## ?? Configuración Avanzada

### Cambiar Puerto

Edita `Properties/launchSettings.json`:

```json
"applicationUrl": "https://localhost:TU_PUERTO;http://localhost:5000"
```

### Habilitar CORS

En `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Después de app.Build()
app.UseCors();
```

### Seed Data (Datos Iniciales)

Crea un archivo `DataSeeder.cs` en Infrastructure:

```csharp
public static class DataSeeder
{
    public static async Task SeedAsync(CigralBackendContext context)
    {
        if (!context.Productos.Any())
        {
            context.Productos.Add(new Producto
            {
                Id = Guid.NewGuid(),
                Nombre = "Producto Demo",
                GTIN = "1234567890123",
                Precio = 100
            });
            await context.SaveChangesAsync();
        }
    }
}
```

Llámalo en `Program.cs`:

```csharp
// Antes de app.Run()
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CigralBackendContext>();
    await DataSeeder.SeedAsync(context);
}
```

---

## ?? Comandos Útiles

```bash
# Ver logs detallados
dotnet run --verbosity detailed

# Watch mode (recompila automáticamente)
dotnet watch run

# Ejecutar en producción
dotnet run --configuration Release

# Ver info del proyecto
dotnet --info

# Listar migraciones
dotnet ef migrations list --project ..\CigralBackend.Infraestructure --startup-project .

# Generar script SQL de migración
dotnet ef migrations script --project ..\CigralBackend.Infraestructure --startup-project .
```

---

## ?? Probar con Postman/Insomnia

### Import de Collection

Puedes exportar desde Swagger:

1. Ve a Swagger UI
2. Click en el link `/swagger/v1/swagger.json`
3. Copia el JSON
4. Importa en Postman/Insomnia

### Ejemplos de Requests

**GET Productos (con paginación):**
```
GET https://localhost:5001/api/products?pageNumber=1&pageSize=10
```

**POST Crear Producto:**
```
POST https://localhost:5001/api/products
Content-Type: application/json

{
  "nombre": "Nuevo Producto",
  "descripcion": "Descripción",
  "gtin": "1234567890123",
  "esUnitario": true,
  "precio": 150.00
}
```

---

## ?? Próximos Pasos (Opcional)

### Agregar Autenticación

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### Agregar Logging

```bash
dotnet add package Serilog.AspNetCore
```

### Agregar AutoMapper

```bash
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

---

## ? Checklist de Setup

- [ ] .NET 8 SDK instalado
- [ ] SQL Server instalado y corriendo
- [ ] Proyecto clonado
- [ ] Connection string configurado
- [ ] Migraciones ejecutadas
- [ ] Aplicación corriendo
- [ ] Swagger abierto en navegador
- [ ] Primer request exitoso

---

## ?? ¡Felicitaciones!

Ya tienes CigralBackend funcionando. Ahora estás listo para:

- ? Desarrollar nuevas features
- ? Explorar el código
- ? Contribuir al proyecto
- ? Aprender Clean Architecture

**Happy Coding!** ??

---

*¿Necesitas más ayuda? Revisa la [documentación completa](INDEX.md) o abre un [issue](https://github.com/lucasmillan17/cigral-backend/issues).*

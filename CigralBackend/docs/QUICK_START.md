# Quick Start Guide - CigralBackend

Pon el proyecto en marcha en 5 minutos.

## Setup Rapido

### 1. Prerrequisitos (2 minutos)

Verifica que tengas instalado:

```bash
# .NET 8 SDK
dotnet --version
# Debe mostrar 8.0.x

# SQL Server
# Verifica que el servicio este corriendo
```

### 2. Clonar y Configurar (1 minuto)

```bash
# Clonar
git clone https://github.com/lucasmillan17/cigral-backend.git
cd cigral-backend/CigralBackend

# Configurar connection string en appsettings.json
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
# Crear migracion inicial
dotnet ef migrations add InitialCreate --project ..\CigralBackend.Infraestructure --startup-project .

# Crear base de datos
dotnet ef database update --project ..\CigralBackend.Infraestructure --startup-project .
```

### 4. Ejecutar (30 segundos)

```bash
dotnet run
```

Abre tu navegador en: **https://localhost:5001/swagger**

**Listo!** Ya tienes el proyecto corriendo.

---

## Y ahora que?

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
5. Veras tu producto creado!

### Ver los Datos

**Opcion 1 - Desde Swagger:**
- Usa `GET /api/products?pageNumber=1&pageSize=10`

**Opcion 2 - SQL Server:**
```sql
USE CigralBackendDB
SELECT * FROM Productos
```

---

## Siguiente Nivel

Ya tienes el proyecto funcionando. Ahora puedes:

### Aprender mas sobre el proyecto:
- [README.md](../README.md) - Vista general completa
- [docs/ARCHITECTURE.md](ARCHITECTURE.md) - Entender la arquitectura
- [docs/DEVELOPMENT.md](DEVELOPMENT.md) - Guia de desarrollo

### Explorar el codigo:
```
CigralBackend.Domain/        <- Entidades de negocio
CigralBackend.Application/   <- Servicios y DTOs
CigralBackend.Infrastructure/ <- Base de datos
CigralBackend.Api/           <- Controllers
```

---

## Problemas?

### Error: "Cannot open database"

**Solucion:**
1. Verifica que SQL Server este corriendo
2. Verifica el connection string en `appsettings.json`
3. Intenta con un usuario y contrasena en lugar de Trusted_Connection

### Error: "dotnet ef not found"

**Solucion:**
```bash
dotnet tool install --global dotnet-ef
```

### Error: Compilacion fallida

**Solucion:**
```bash
# Restaurar paquetes
dotnet restore

# Limpiar y recompilar
dotnet clean
dotnet build
```

---

## Comandos Utiles

```bash
# Ver logs detallados
dotnet run --verbosity detailed

# Watch mode (recompila automaticamente)
dotnet watch run

# Ejecutar en produccion
dotnet run --configuration Release

# Listar migraciones
dotnet ef migrations list --project ..\CigralBackend.Infraestructure --startup-project .
```

---

**Happy Coding!**

*Necesitas mas ayuda? Revisa la [documentacion completa](INDEX.md)*

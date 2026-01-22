# Indice de Documentacion - CigralBackend

Bienvenido a la documentacion de CigralBackend. Esta guia te ayudara a encontrar rapidamente la informacion que necesitas.

## Documentacion Disponible

### Para Empezar

| Documento | Descripcion | Audiencia |
|-----------|-------------|-----------|
| [README.md](../README.md) | Vista general del proyecto, instalacion rapida y caracteristicas principales | Todos |
| [DATABASE_SETUP.md](../DATABASE_SETUP.md) | Configuracion de Entity Framework, migraciones y base de datos | Desarrolladores |

### Guias de Desarrollo

| Documento | Descripcion | Audiencia |
|-----------|-------------|-----------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | Explicacion detallada de la arquitectura en capas, patrones y principios SOLID | Arquitectos, Desarrolladores Senior |
| [DEVELOPMENT.md](DEVELOPMENT.md) | Guia completa para configurar el entorno y desarrollar nuevas features | Desarrolladores |
| [QUICK_START.md](QUICK_START.md) | Inicio rapido en 5 minutos | Nuevos Desarrolladores |

## Rutas Rapidas por Rol

### Nuevo Desarrollador

1. Lee el [README.md](../README.md) para entender el proyecto
2. Sigue [QUICK_START.md](QUICK_START.md) para setup rapido
3. Revisa [DEVELOPMENT.md](DEVELOPMENT.md) seccion "Configuracion del Entorno"
4. Explora [ARCHITECTURE.md](ARCHITECTURE.md) para entender la estructura

### Arquitecto / Tech Lead

1. [ARCHITECTURE.md](ARCHITECTURE.md) - Entender decisiones arquitectonicas
2. [README.md](../README.md) - Vision general de caracteristicas
3. [DEVELOPMENT.md](DEVELOPMENT.md) - Proceso de desarrollo

## Contenido por Tema

### Arquitectura

- **Capas del Sistema**: [ARCHITECTURE.md - Capas de la Aplicacion](ARCHITECTURE.md#capas-de-la-aplicacion)
- **Patrones de Diseno**: [ARCHITECTURE.md - Patrones Implementados](ARCHITECTURE.md#patrones-implementados)
- **Flujo de Datos**: [ARCHITECTURE.md - Flujo de Datos](ARCHITECTURE.md#flujo-de-datos)
- **Principios SOLID**: [ARCHITECTURE.md - Principios SOLID](ARCHITECTURE.md#principios-solid-aplicados)

### Base de Datos

- **Configuracion Inicial**: [DATABASE_SETUP.md](../DATABASE_SETUP.md)
- **Migraciones**: [DATABASE_SETUP.md - Crear y Aplicar Migraciones](../DATABASE_SETUP.md)
- **DbContext**: [ARCHITECTURE.md - Infrastructure Layer](ARCHITECTURE.md#3-infrastructure-infraestructura)
- **Entities**: [README.md - Modelos de Dominio](../README.md)

### Desarrollo

- **Setup Inicial**: [DEVELOPMENT.md - Configuracion Inicial](DEVELOPMENT.md#configuracion-inicial)
- **Convenciones de Codigo**: [DEVELOPMENT.md - Convenciones](DEVELOPMENT.md#convenciones-de-codigo)
- **Workflow Git**: [DEVELOPMENT.md - Workflow de Desarrollo](DEVELOPMENT.md#workflow-de-desarrollo)
- **Agregar Features**: [DEVELOPMENT.md - Agregar Nueva Funcionalidad](DEVELOPMENT.md#agregar-nueva-funcionalidad)

### API

- **Endpoints**: [README.md - Uso del API](../README.md#uso-del-api)
- **Paginacion**: [README.md - Paginacion](../README.md#paginacion)
- **Swagger**: README.md (ver seccion de Uso)

## Busqueda Rapida

### "Como hago para...?"

| Pregunta | Respuesta en |
|----------|--------------|
| ...instalar el proyecto? | [README.md](../README.md) o [QUICK_START.md](QUICK_START.md) |
| ...crear la base de datos? | [DATABASE_SETUP.md](../DATABASE_SETUP.md) |
| ...agregar una nueva entidad? | [DEVELOPMENT.md - Agregar Nueva Funcionalidad](DEVELOPMENT.md#agregar-nueva-funcionalidad) |
| ...crear una migracion? | [DATABASE_SETUP.md](../DATABASE_SETUP.md) |
| ...usar paginacion? | [DATABASE_SETUP.md](../DATABASE_SETUP.md) |
| ...entender la arquitectura? | [ARCHITECTURE.md](ARCHITECTURE.md) |

### "Donde esta...?"

| Elemento | Ubicacion |
|----------|-----------|
| Entidades del dominio | `CigralBackend.Domain/` |
| DTOs y validaciones | `CigralBackend.Application/Dtos/` |
| Servicios de negocio | `CigralBackend.Application/Services/` |
| Repositorio | `CigralBackend.Infrastructure/Database/` |
| Controllers | `CigralBackend.Api/Controllers/` |
| DbContext | `CigralBackend.Infrastructure/Database/CigralBackendContext.cs` |
| Configuracion de DI | `CigralBackend.Api/Program.cs` |
| Connection string | `CigralBackend.Api/appsettings.json` |

## Checklist de Documentacion

### Para Nuevos Features

Cuando agregas un nuevo feature, actualiza:

- [ ] README.md - Si es una caracteristica importante
- [ ] ARCHITECTURE.md - Si cambia la estructura
- [ ] API Documentation - Swagger comments en controllers
- [ ] XML Documentation - En clases y metodos publicos
- [ ] Ejemplos de uso - Si es complejo

## Recursos de Aprendizaje

### .NET y C#

- [Microsoft .NET Documentation](https://docs.microsoft.com/dotnet/)
- [C# Programming Guide](https://docs.microsoft.com/dotnet/csharp/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)

### Entity Framework Core

- [EF Core Documentation](https://docs.microsoft.com/ef/core/)
- [EF Core Migrations](https://docs.microsoft.com/ef/core/managing-schemas/migrations/)

### Arquitectura y Patrones

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/tags/domain%20driven%20design.html)
- [Repository Pattern](https://docs.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)

### Git y Colaboracion

- [Conventional Commits](https://www.conventionalcommits.org/)
- [Git Flow](https://nvie.com/posts/a-successful-git-branching-model/)

## Soporte

### Necesitas Ayuda?

1. **Revisa la documentacion** - Probablemente la respuesta este aqui
2. **Busca en el codigo** - Usa ejemplos existentes como guia
3. **Contacta al equipo** - Ver README para contactos

### Issues Comunes

| Problema | Solucion |
|----------|----------|
| Error de conexion a BD | Verificar connection string en appsettings.json |
| Migraciones no se aplican | Verificar que el proyecto de startup sea correcto |
| NuGet package no se restaura | Ejecutar `dotnet restore` |
| Tests fallan | Verificar que la BD de test este configurada |

## Mantenimiento de Documentacion

La documentacion debe:

- Mantenerse actualizada con cada cambio significativo
- Ser clara y concisa
- Incluir ejemplos cuando sea posible
- Estar en espanol para el dominio del negocio
- Usar markdown para formato consistente

### Responsabilidades

- **Desarrolladores**: Actualizar documentacion tecnica con cada PR
- **Tech Lead**: Revisar que la documentacion este completa

---

**Ultima actualizacion**: Enero 2025

**Version de la documentacion**: 1.0

**Mantenedores**: Lucas Millan

# Índice de Documentación - CigralBackend

Bienvenido a la documentación de CigralBackend. Esta guía te ayudará a encontrar rápidamente la información que necesitas.

## ?? Documentación Disponible

### Para Empezar

| Documento | Descripción | Audiencia |
|-----------|-------------|-----------|
| [README.md](../README.md) | Vista general del proyecto, instalación rápida y características principales | Todos |
| [DATABASE_SETUP.md](../DATABASE_SETUP.md) | Configuración de Entity Framework, migraciones y base de datos | Desarrolladores |

### Guías de Desarrollo

| Documento | Descripción | Audiencia |
|-----------|-------------|-----------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | Explicación detallada de la arquitectura en capas, patrones y principios SOLID | Arquitectos, Desarrolladores Senior |
| [DEVELOPMENT.md](DEVELOPMENT.md) | Guía completa para configurar el entorno y desarrollar nuevas features | Desarrolladores |
| [CONTRIBUTING.md](../CONTRIBUTING.md) | Cómo contribuir al proyecto, convenciones y proceso de PR | Contribuidores |

## ?? Rutas Rápidas por Rol

### Nuevo Desarrollador

1. Lee el [README.md](../README.md) para entender el proyecto
2. Sigue [DATABASE_SETUP.md](../DATABASE_SETUP.md) para configurar la BD
3. Revisa [DEVELOPMENT.md](DEVELOPMENT.md) sección "Configuración del Entorno"
4. Explora [ARCHITECTURE.md](ARCHITECTURE.md) para entender la estructura

### Arquitecto / Tech Lead

1. [ARCHITECTURE.md](ARCHITECTURE.md) - Entender decisiones arquitectónicas
2. [README.md](../README.md) - Visión general de características
3. [DEVELOPMENT.md](DEVELOPMENT.md) - Proceso de desarrollo

### Contribuidor Externo

1. [CONTRIBUTING.md](../CONTRIBUTING.md) - Guía de contribución
2. [README.md](../README.md) - Visión general
3. [DEVELOPMENT.md](DEVELOPMENT.md) - Setup y convenciones

## ?? Contenido por Tema

### Arquitectura

- **Capas del Sistema**: [ARCHITECTURE.md - Capas de la Aplicación](ARCHITECTURE.md#capas-de-la-aplicación)
- **Patrones de Diseño**: [ARCHITECTURE.md - Patrones Implementados](ARCHITECTURE.md#patrones-implementados)
- **Flujo de Datos**: [ARCHITECTURE.md - Flujo de Datos](ARCHITECTURE.md#flujo-de-datos)
- **Principios SOLID**: [ARCHITECTURE.md - Principios SOLID](ARCHITECTURE.md#principios-solid-aplicados)

### Base de Datos

- **Configuración Inicial**: [DATABASE_SETUP.md - Configuración](../DATABASE_SETUP.md#configuración-de-la-base-de-datos)
- **Migraciones**: [DATABASE_SETUP.md - Migraciones](../DATABASE_SETUP.md#crear-y-aplicar-migraciones)
- **DbContext**: [ARCHITECTURE.md - Infrastructure Layer](ARCHITECTURE.md#3-infrastructure-infraestructura-)
- **Entities**: [README.md - Modelos de Dominio](../README.md#-modelos-de-dominio)

### Desarrollo

- **Setup Inicial**: [DEVELOPMENT.md - Configuración Inicial](DEVELOPMENT.md#configuración-inicial)
- **Convenciones de Código**: [DEVELOPMENT.md - Convenciones](DEVELOPMENT.md#convenciones-de-código)
- **Workflow Git**: [DEVELOPMENT.md - Workflow de Desarrollo](DEVELOPMENT.md#workflow-de-desarrollo)
- **Agregar Features**: [DEVELOPMENT.md - Agregar Nueva Funcionalidad](DEVELOPMENT.md#agregar-nueva-funcionalidad)

### API

- **Endpoints**: [README.md - Uso del API](../README.md#-uso-del-api)
- **Paginación**: [README.md - Paginación](../README.md#-paginación)
- **Swagger**: README.md (ver sección de Uso)

### Contribución

- **Cómo Contribuir**: [CONTRIBUTING.md](../CONTRIBUTING.md)
- **Reportar Bugs**: [CONTRIBUTING.md - Reportar Bugs](../CONTRIBUTING.md#reportar-bugs)
- **Pull Requests**: [CONTRIBUTING.md - Pull Requests](../CONTRIBUTING.md#pull-requests)
- **Code Review**: [CONTRIBUTING.md - Proceso de Review](../CONTRIBUTING.md#proceso-de-review)

## ?? Búsqueda Rápida

### "¿Cómo hago para...?"

| Pregunta | Respuesta en |
|----------|--------------|
| ...instalar el proyecto? | [README.md - Configuración Inicial](../README.md#-configuración-inicial) |
| ...crear la base de datos? | [DATABASE_SETUP.md](../DATABASE_SETUP.md) |
| ...agregar una nueva entidad? | [DEVELOPMENT.md - Agregar Nueva Funcionalidad](DEVELOPMENT.md#agregar-nueva-funcionalidad) |
| ...crear una migración? | [DATABASE_SETUP.md - Migraciones](../DATABASE_SETUP.md#crear-y-aplicar-migraciones) |
| ...usar paginación? | [DATABASE_SETUP.md - Paginación](../DATABASE_SETUP.md#características-implementadas) |
| ...hacer un PR? | [CONTRIBUTING.md - Pull Requests](../CONTRIBUTING.md#pull-requests) |
| ...ejecutar tests? | [CONTRIBUTING.md - Testing](../CONTRIBUTING.md#testing) |
| ...entender la arquitectura? | [ARCHITECTURE.md](ARCHITECTURE.md) |

### "¿Dónde está...?"

| Elemento | Ubicación |
|----------|-----------|
| Entidades del dominio | `CigralBackend.Domain/` |
| DTOs y validaciones | `CigralBackend.Application/Dtos/` |
| Servicios de negocio | `CigralBackend.Application/Services/` |
| Repositorio | `CigralBackend.Infrastructure/Database/` |
| Controllers | `CigralBackend.Api/Controllers/` |
| DbContext | `CigralBackend.Infrastructure/Database/CigralBackendContext.cs` |
| Configuración de DI | `CigralBackend.Api/Program.cs` |
| Connection string | `CigralBackend.Api/appsettings.json` |

## ?? Checklist de Documentación

### Para Nuevos Features

Cuando agregas un nuevo feature, actualiza:

- [ ] README.md - Si es una característica importante
- [ ] ARCHITECTURE.md - Si cambia la estructura
- [ ] API Documentation - Swagger comments en controllers
- [ ] XML Documentation - En clases y métodos públicos
- [ ] Ejemplos de uso - Si es complejo

### Para Cambios Arquitectónicos

- [ ] ARCHITECTURE.md - Documentar el cambio
- [ ] README.md - Actualizar diagrama si aplica
- [ ] DEVELOPMENT.md - Actualizar guías afectadas
- [ ] Crear ADR (Architecture Decision Record) si es significativo

## ?? Recursos de Aprendizaje

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

### Git y Colaboración

- [Conventional Commits](https://www.conventionalcommits.org/)
- [Git Flow](https://nvie.com/posts/a-successful-git-branching-model/)

## ?? Plantillas

### Issue de Bug

```markdown
**Descripción del Bug**
[Descripción clara y concisa]

**Pasos para Reproducir**
1. 
2. 
3. 

**Comportamiento Esperado**
[Qué debería pasar]

**Comportamiento Actual**
[Qué pasa actualmente]

**Entorno**
- OS: 
- .NET Version: 
- SQL Server: 
```

### Issue de Feature

```markdown
**Feature Request**
[Descripción de la funcionalidad deseada]

**Problema que Resuelve**
[Qué problema soluciona]

**Solución Propuesta**
[Cómo lo implementarías]

**Alternativas**
[Otras opciones consideradas]
```

### Template de PR

```markdown
## Descripción
[Descripción de los cambios]

## Tipo de cambio
- [ ] Bug fix
- [ ] Nueva funcionalidad
- [ ] Breaking change
- [ ] Documentación

## Checklist
- [ ] Tests pasan
- [ ] Código sigue convenciones
- [ ] Documentación actualizada
- [ ] Sin warnings de compilación
```

## ?? Soporte

### ¿Necesitas Ayuda?

1. **Revisa la documentación** - Probablemente la respuesta esté aquí
2. **Busca en Issues** - Alguien más pudo haber tenido la misma pregunta
3. **Abre un Issue** - Con la etiqueta `question`
4. **Contacta al equipo** - Ver README para contactos

### Issues Comunes

| Problema | Solución |
|----------|----------|
| Error de conexión a BD | Verificar connection string en appsettings.json |
| Migraciones no se aplican | Verificar que el proyecto de startup sea correcto |
| NuGet package no se restaura | Ejecutar `dotnet restore` |
| Tests fallan | Verificar que la BD de test esté configurada |

## ?? Mantenimiento de Documentación

La documentación debe:

- ? Mantenerse actualizada con cada cambio significativo
- ? Ser clara y concisa
- ? Incluir ejemplos cuando sea posible
- ? Estar en español para el dominio del negocio
- ? Usar markdown para formato consistente

### Responsabilidades

- **Desarrolladores**: Actualizar documentación técnica con cada PR
- **Tech Lead**: Revisar que la documentación esté completa
- **Product Owner**: Mantener documentación de negocio actualizada

---

**Última actualización**: Enero 2025

**Versión de la documentación**: 1.0

**Mantenedores**: Lucas Millan

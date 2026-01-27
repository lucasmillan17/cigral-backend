# Script para crear usuario administrador inicial con Identity
# Ejecutar este script después de crear la base de datos

## ?? IMPORTANTE: ASP.NET Core Identity está implementado

El sistema ahora usa **ASP.NET Core Identity** para gestión de usuarios. La creación del usuario admin debe hacerse mediante código o manualmente con el hash correcto de Identity.

---

## Opción 1: Crear Admin mediante Endpoint (RECOMENDADO)

### Paso 1: Crear usuario temporal con permisos admin

Después de crear las migraciones de Identity, ejecuta este código una sola vez en `Program.cs` (antes de `app.Run()`):

```csharp
// SOLO PARA DESARROLLO - Crear usuario admin inicial
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    
    // Verificar si ya existe el admin
    var adminUser = await userManager.FindByNameAsync("admin");
    
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@cigral.com",
            EmailConfirmed = true, // Activo
            EsAdmin = true,
            NombreCompleto = "Administrador del Sistema",
            FechaCreacion = DateTime.Now
        };
        
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        
        if (result.Succeeded)
        {
            Console.WriteLine("Usuario admin creado exitosamente");
        }
    }
}
```

**Después de ejecutar una vez, ELIMINA este código.**

---

## Opción 2: SQL Directo (Avanzado)

Si prefieres SQL directo, necesitas generar el hash de contraseña compatible con Identity:

```sql
-- Insertar en AspNetUsers (tabla de Identity)
INSERT INTO AspNetUsers (
    UserName, 
    NormalizedUserName, 
    Email, 
    NormalizedEmail, 
    EmailConfirmed, 
    PasswordHash, 
    SecurityStamp, 
    ConcurrencyStamp,
    PhoneNumberConfirmed,
    TwoFactorEnabled,
    LockoutEnabled,
    AccessFailedCount,
    NombreCompleto,
    EsAdmin,
    FechaCreacion
)
VALUES (
    'admin',
    'ADMIN',
    'admin@cigral.com',
    'ADMIN@CIGRAL.COM',
    1, -- EmailConfirmed = true (equivale a Activo)
    'AQAAAAIAAYagAAAAEKxP8C9xOY8W9qZ1pW6Z8fN2qB7xL9qK3mP5nR8tS2vU4wX7yA1', -- Hash de "Admin123!"
    NEWID(), -- SecurityStamp
    NEWID(), -- ConcurrencyStamp
    0,
    0,
    1,
    0,
    'Administrador del Sistema',
    1, -- EsAdmin = true
    GETDATE()
);
```

?? **NOTA:** El hash de arriba es de ejemplo. Para generar el hash correcto, usa la Opción 1.

---

## Credenciales por defecto:
- **Username:** admin
- **Password:** Admin123!

### Requisitos de contraseña (configurados en Identity):
- ? Mínimo 6 caracteres
- ? Al menos 1 dígito
- ? Al menos 1 mayúscula
- ? Al menos 1 minúscula
- ? Al menos 1 carácter especial

?? **IMPORTANTE:** Cambia esta contraseña inmediatamente después del primer login por seguridad.

---

## Cómo usar:

### 1. Crear la migración de Identity:
```bash
cd CigralBackend
dotnet ef migrations add AgregarIdentity --project ..\CigralBackend.Infraestructure --startup-project .
dotnet ef database update --project ..\CigralBackend.Infraestructure --startup-project .
```

Esto creará las siguientes tablas de Identity:
- `AspNetUsers` - Usuarios
- `AspNetRoles` - Roles
- `AspNetUserRoles` - Relación usuarios-roles
- `AspNetUserClaims` - Claims de usuarios
- `AspNetUserLogins` - Logins externos
- `AspNetUserTokens` - Tokens de usuario

### 2. Crear usuario admin (elegir una opción)

**Opción A:** Agregar código temporal en `Program.cs` (recomendado)

**Opción B:** Ejecutar SQL directo (avanzado)

### 3. Probar el login:
```http
POST /api/auth/login
{
  "username": "admin",
  "password": "Admin123!"
}
```

### 4. Usar el token en Swagger:
- Copiar el token de la respuesta
- Click en "Authorize" en Swagger
- Escribir: `Bearer {tu-token-aqui}`
- Click "Authorize"

Ahora podrás acceder a todos los endpoints protegidos.

---

## Ventajas de Identity

? **Seguridad mejorada**
- Hash de contraseñas más robusto
- Soporte para lockout automático
- Validación de contraseñas configurable

? **Funcionalidades listas**
- Confirmación de email
- Recuperación de contraseña
- Two-Factor Authentication (2FA)
- External logins (Google, Facebook, etc.)

? **Gestión profesional**
- UserManager para operaciones de usuarios
- SignInManager para autenticación
- RoleManager para roles (futuro)

---

## Diferencias con implementación manual anterior

| Característica | Manual | Identity |
|----------------|--------|----------|
| **Tabla** | `Usuarios` | `AspNetUsers` |
| **Hash** | BCrypt | Identity (PBKDF2) |
| **Validación** | Manual | Automática |
| **Lockout** | No | Sí (5 intentos, 5 min) |
| **2FA** | No | Sí (listo para usar) |
| **External Logins** | No | Sí |
| **Email Confirmation** | No | Sí |
| **Password Reset** | No | Sí |

---

## Migrar usuarios existentes (si los hay)

Si ya tenías usuarios con BCrypt, necesitas:

1. Exportar datos de `Usuarios`
2. Crear usuarios con Identity usando `UserManager.CreateAsync()`
3. Eliminar tabla `Usuarios` antigua

?? **Las contraseñas BCrypt NO son compatibles** con Identity. Los usuarios deberán resetear sus contraseñas.

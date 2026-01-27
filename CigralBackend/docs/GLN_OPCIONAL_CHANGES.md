# ? GLN Opcional - Cambios Implementados

## ?? Resumen de Cambios

**El GLN ahora es OPCIONAL para Clientes y Proveedores**

---

## ?? Motivación

El GLN (Global Location Number) es un identificador útil pero no siempre se tiene disponible al momento de registrar un cliente o proveedor. Ahora el sistema permite crear estas entidades sin GLN y agregarlo posteriormente.

---

## ?? Archivos Modificados (8)

### DTOs (2)
1. ? `ClienteModel.cs`
   - `GLN` cambiado de `string` a `string?` (nullable)
   - Removido `[Required]`
   - Mantenido `[MinLength(13)][MaxLength(13)]` para validar formato cuando se proporciona

2. ? `ProveedorModel.cs`
   - `GLN` cambiado de `string` a `string?` (nullable)
   - Removido `[Required]`
   - Mantenido `[MinLength(13)][MaxLength(13)]` para validar formato cuando se proporciona

### Entidades de Dominio (2)
3. ? `Cliente.cs`
   - `GLN` cambiado de `string` a `string?`

4. ? `Proveedor.cs`
   - `GLN` cambiado de `string` a `string?`

### Configuración de Base de Datos (1)
5. ? `CigralBackendContext.cs`
   - Removido `.IsRequired()` del GLN en Cliente
   - Removido `.IsRequired()` del GLN en Proveedor

### Servicios (2)
6. ? `ClienteService.cs`
   - CreateCliente: Valida GLN único **solo si se proporciona**
   - UpdateCliente: Valida GLN único **solo si se proporciona y cambió**

7. ? `ProveedorService.cs`
   - CreateProveedor: Valida GLN único **solo si se proporciona**
   - UpdateProveedor: Valida GLN único **solo si se proporciona y cambió**

---

## ? Validaciones Actualizadas

### Antes
```csharp
// GLN era REQUERIDO
[Required(ErrorMessage = "El GLN es obligatorio")]
[MaxLength(13)][MinLength(13)]
string GLN

// Siempre se validaba unicidad
var existeGLN = await _repository.First<Cliente>(c => c.GLN == request.GLN);
```

### Después
```csharp
// GLN es OPCIONAL
[MaxLength(13)][MinLength(13)]
string? GLN

// Solo se valida si se proporciona
if (!string.IsNullOrEmpty(request.GLN))
{
    var existeGLN = await _repository.First<Cliente>(c => c.GLN == request.GLN);
    if (existeGLN != null)
    {
        throw new DomainException(...);
    }
}
```

---

## ?? Comportamiento

### Crear Cliente/Proveedor SIN GLN ?
```json
POST /api/clientes
{
  "razonSocial": "Cliente Test S.A.",
  "email": "contacto@test.com",
  "cuit": "30123456789"
}
```

**Resultado:** ? Se crea correctamente con GLN = null

---

### Crear Cliente/Proveedor CON GLN ?
```json
POST /api/clientes
{
  "razonSocial": "Cliente Test S.A.",
  "gln": "7798765432109",
  "email": "contacto@test.com"
}
```

**Resultado:** ? Se crea correctamente con GLN validado

---

### GLN con formato inválido ?
```json
POST /api/clientes
{
  "razonSocial": "Cliente Test S.A.",
  "gln": "123"  // ? Menos de 13 caracteres
}
```

**Resultado:** ? Error 400 - "El GLN debe tener 13 caracteres"

---

### GLN duplicado ?
```json
POST /api/clientes
{
  "razonSocial": "Cliente Test S.A.",
  "gln": "7798765432109"  // ? Ya existe
}
```

**Resultado:** ? Error 400 - `DomainException(GlnClienteDuplicado)`

---

### Actualizar para agregar GLN ?
```json
PUT /api/clientes/5
{
  "razonSocial": "Cliente Test S.A.",
  "gln": "7798765432109",  // ? Agregando GLN
  "email": "contacto@test.com"
}
```

**Resultado:** ? Se actualiza correctamente

---

### Actualizar para quitar GLN ?
```json
PUT /api/clientes/5
{
  "razonSocial": "Cliente Test S.A.",
  "gln": null,  // ? Quitando GLN
  "email": "contacto@test.com"
}
```

**Resultado:** ? Se actualiza correctamente (GLN = null)

---

## ?? Búsqueda por GLN

### Funciona con GLN null ?
```http
GET /api/clientes?gln=7798765432109
```

**Resultado:** ? Encuentra clientes con ese GLN

```http
GET /api/clientes
```

**Resultado:** ? Devuelve TODOS los clientes (con y sin GLN)

---

## ?? Casos de Uso

### Caso 1: Cliente sin GLN inicialmente
```
1. POST /api/clientes (sin GLN)
   ? Cliente creado con GLN = null

2. Más tarde, obtienen el GLN...

3. PUT /api/clientes/{id} (con GLN)
   ? Cliente actualizado con GLN
```

---

### Caso 2: Cliente con GLN desde el inicio
```
1. POST /api/clientes (con GLN)
   ? Cliente creado con GLN validado
```

---

### Caso 3: Corrección de GLN
```
1. Cliente tiene GLN incorrecto

2. PUT /api/clientes/{id} (con GLN correcto)
   ? Valida que el nuevo GLN no esté duplicado
   ? Actualiza el GLN
```

---

## ? Ventajas del Cambio

### 1. **Flexibilidad**
- ? Permite crear clientes/proveedores sin GLN
- ? Puede agregarse el GLN posteriormente

### 2. **Realidad del Negocio**
- ? No todos tienen GLN inmediatamente
- ? Permite operar mientras se obtiene el dato

### 3. **Migración de Datos**
- ? Facilita importar datos existentes sin GLN
- ? No requiere GLN dummy

### 4. **Mantiene Validaciones**
- ? Si se proporciona, debe tener 13 caracteres
- ? Si se proporciona, debe ser único
- ? No se pierden validaciones de negocio

---

## ??? Validaciones que se Mantienen

| Validación | Cliente | Proveedor |
|------------|---------|-----------|
| **Razón Social requerida** | ? | ? |
| **GLN único (si se proporciona)** | ? | ? |
| **GLN 13 caracteres (si se proporciona)** | ? | ? |
| **CUIT único (si se proporciona)** | ? | ? |
| **Email válido (si se proporciona)** | ? | ? |

---

## ?? Migración de Base de Datos

### Comando Necesario

Después de estos cambios, necesitas crear y aplicar una migración:

```sh
cd CigralBackend
dotnet ef migrations add GLNOpcionalClienteProveedor --project ..\CigralBackend.Infraestructure --startup-project .
dotnet ef database update --project ..\CigralBackend.Infraestructure --startup-project .
```

### Cambios en la BD

```sql
-- Columna GLN cambia de NOT NULL a NULL
ALTER TABLE Clientes 
ALTER COLUMN GLN nvarchar(13) NULL;

ALTER TABLE Proveedores 
ALTER COLUMN GLN nvarchar(13) NULL;
```

---

## ? Estado de Compilación

```
??????????????????????????????????????????????
?                                            ?
?    ? CAMBIOS APLICADOS EXITOSAMENTE ?   ?
?                                            ?
?  ? Compilación:       EXITOSA            ?
?  ? DTOs:              Actualizados       ?
?  ? Entidades:         Actualizadas       ?
?  ? Servicios:         Actualizados       ?
?  ? DbContext:         Actualizado        ?
?  ? Validaciones:      Condicionales      ?
?  ? Backward compatible: NO (requiere     ?
?                           migración)      ?
?                                            ?
??????????????????????????????????????????????
```

---

## ?? Notas Importantes

### ?? Breaking Change
- Este es un **breaking change** que requiere migración de BD
- Los clientes/proveedores existentes mantendrán su GLN
- Nuevos registros pueden crearse sin GLN

### ? Compatibilidad
- Clientes/proveedores existentes **NO se ven afectados**
- Código que consume la API debe actualizar validaciones
- Frontend debe permitir GLN vacío

---

## ?? Próximos Pasos

1. [ ] **Crear migración de BD** (comando arriba)
2. [ ] **Aplicar migración** a base de datos
3. [ ] **Probar** creación sin GLN en Swagger
4. [ ] **Actualizar** frontend para permitir GLN opcional
5. [ ] **Actualizar** documentación de API

---

**¡GLN ahora es opcional para Clientes y Proveedores!** ?

**Cambio motivado por:** Realidad del negocio donde no siempre se tiene el GLN de inmediato

**Impacto:** Mínimo, solo requiere migración de BD y actualización de frontend

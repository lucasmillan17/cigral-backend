# Sistema de Auditoría de Movimientos de Stock

## ? IMPLEMENTADO COMPLETAMENTE

**Sistema completo de tracking y auditoría para todos los movimientos de stock**

---

## ?? Resumen de Implementación

### Nuevo Sistema
1. ? **Tabla MovimientoStock** - Registro completo de auditoría
2. ? **ExistenciaService** - Registra automáticamente movimientos
3. ? **RemitoService** - Usa solo IRepository, sin DbContext
4. ? **UpdateRemito** - Actualización de datos no críticos
5. ? **Endpoints PUT** - 2 nuevos endpoints de actualización

---

## ??? Tabla de Auditoría: MovimientoStock

### Campos

| Campo | Tipo | Descripción |
|-------|------|-------------|
| **Id** | int | ID único del movimiento |
| **Tipo** | TipoMovimiento | Tipo de movimiento (enum) |
| **FechaMovimiento** | DateTime | Fecha y hora exacta |
| **ProductoId** | int | Producto afectado |
| **DepositoId** | int | Depósito donde ocurrió |
| **LoteId** | int? | Lote (opcional) |
| **NumeroSerie** | string? | Número de serie (opcional) |
| **Cantidad** | int | Cantidad movida (+/- según tipo) |
| **StockAnterior** | int | Stock ANTES del movimiento |
| **StockNuevo** | int | Stock DESPUÉS del movimiento |
| **RemitoIngresoId** | int? | Remito de ingreso (si aplica) |
| **RemitoEgresoId** | int? | Remito de egreso (si aplica) |
| **Usuario** | string? | Usuario que realizó la acción |
| **Observaciones** | string? | Notas adicionales |

### Tipos de Movimiento (Enum)

```csharp
public enum TipoMovimiento
{
    Ingreso = 1,           // Remito de proveedor
    Egreso = 2,            // Remito de cliente
    AjustePositivo = 3,    // Ajuste manual (aumento)
    AjusteNegativo = 4,    // Ajuste manual (disminución)
    Transferencia = 5      // Entre depósitos (futuro)
}
```

---

## ?? Registro Automático de Movimientos

### Cuándo se Registra

#### 1. Remito de Ingreso
```
POST /api/remitos/ingreso
? RemitoService.RegistrarIngreso()
? ExistenciaService.AumentarStock(remitoIngresoId: X)
? RegistrarMovimiento(Tipo: Ingreso, RemitoIngresoId: X)
```

**Registro en MovimientoStock:**
```json
{
  "tipo": "Ingreso",
  "productoId": 10,
  "depositoId": 1,
  "cantidad": 100,
  "stockAnterior": 50,
  "stockNuevo": 150,
  "remitoIngresoId": 5,
  "observaciones": "Remito de ingreso REM-001"
}
```

#### 2. Remito de Egreso
```
POST /api/remitos/egreso
? RemitoService.RegistrarEgreso()
? ExistenciaService.DisminuirStock(remitoEgresoId: Y)
? RegistrarMovimiento(Tipo: Egreso, RemitoEgresoId: Y)
```

**Registro en MovimientoStock:**
```json
{
  "tipo": "Egreso",
  "productoId": 10,
  "depositoId": 1,
  "cantidad": -50,
  "stockAnterior": 150,
  "stockNuevo": 100,
  "remitoEgresoId": 3,
  "observaciones": "Remito de egreso REM-SAL-001"
}
```

#### 3. Ajuste Manual (Aumento)
```
POST /api/existencias/aumentar
? ExistenciaService.AumentarStock(remitoIngresoId: null)
? RegistrarMovimiento(Tipo: AjustePositivo)
```

**Registro en MovimientoStock:**
```json
{
  "tipo": "AjustePositivo",
  "productoId": 10,
  "depositoId": 1,
  "cantidad": 10,
  "stockAnterior": 100,
  "stockNuevo": 110,
  "observaciones": "Ajuste manual de inventario"
}
```

#### 4. Ajuste Manual (Disminución)
```
POST /api/existencias/disminuir
? ExistenciaService.DisminuirStock(remitoEgresoId: null)
? RegistrarMovimiento(Tipo: AjusteNegativo)
```

**Registro en MovimientoStock:**
```json
{
  "tipo": "AjusteNegativo",
  "productoId": 10,
  "depositoId": 1,
  "cantidad": -5,
  "stockAnterior": 110,
  "stockNuevo": 105,
  "observaciones": "Corrección de inventario - producto dañado"
}
```

---

## ?? Refactorización Realizada

### RemitoService - Ahora usa solo IRepository

**Antes:**
```csharp
private readonly CigralBackendContext _context;
private readonly IExistenciaService _existenciaService;

using var transaction = await _context.Database.BeginTransactionAsync();
// ...
```

**Después:**
```csharp
private readonly IRepository _repository;
private readonly IExistenciaService _existenciaService;

// SIN transacciones manuales
// Todo se maneja en capa de repositorio
```

### ExistenciaService - Registra Auditoría

**Método privado agregado:**
```csharp
private async Task RegistrarMovimiento(
    TipoMovimiento tipo,
    int productoId,
    int depositoId,
    int? loteId,
    string? numeroSerie,
    int cantidad,
    int stockAnterior,
    int stockNuevo,
    int? remitoIngresoId = null,
    int? remitoEgresoId = null,
    string? observaciones = null,
    string? usuario = null)
{
    var movimiento = new MovimientoStock { /* ... */ };
    await _repository.Add(movimiento);
}
```

**Llamado desde AumentarStock:**
```csharp
await RegistrarMovimiento(
    tipo: remitoIngresoId.HasValue 
        ? TipoMovimiento.Ingreso 
        : TipoMovimiento.AjustePositivo,
    productoId: r.ProductoId,
    // ...
    remitoIngresoId: remitoIngresoId
);
```

---

## ?? Actualización de Remitos (Solo Datos No Críticos)

### Nuevo Endpoint: PUT Ingreso

```http
PUT /api/remitos/ingreso/{id}
{
  "numeroRemito": "REM-ING-001-MODIFICADO",
  "observaciones": "Observaciones actualizadas"
}
```

**Qué SE PUEDE actualizar:**
- ? NumeroRemito
- ? Observaciones

**Qué NO se puede actualizar:**
- ? DepositoId
- ? ProveedorId/ClienteId
- ? Fecha
- ? Detalles (productos, cantidades)
- ? Stock

**Validaciones:**
- Número de remito único (si se cambia)
- Remito debe existir

### Nuevo Endpoint: PUT Egreso

```http
PUT /api/remitos/egreso/{id}
{
  "numeroRemito": "REM-EGR-001-MODIFICADO",
  "observaciones": "Cliente confirmó recepción"
}
```

**Mismas validaciones que ingreso**

---

## ?? Rastreabilidad Completa

### Por Remito
```sql
SELECT * FROM MovimientosStock 
WHERE RemitoIngresoId = 5
  OR RemitoEgresoId = 3
ORDER BY FechaMovimiento;
```

**Resultado:** Todos los movimientos de stock asociados a ese remito.

### Por Producto
```sql
SELECT * FROM MovimientosStock 
WHERE ProductoId = 10
ORDER BY FechaMovimiento DESC;
```

**Resultado:** Historial completo de movimientos del producto.

### Por Depósito
```sql
SELECT * FROM MovimientosStock 
WHERE DepositoId = 1
  AND FechaMovimiento >= '2025-01-01'
ORDER BY FechaMovimiento DESC;
```

**Resultado:** Todos los movimientos en el depósito en el período.

### Stock en Fecha Específica
```sql
-- Stock del producto 10 en depósito 1 al 15/01/2025
SELECT 
    StockNuevo 
FROM MovimientosStock
WHERE ProductoId = 10 
  AND DepositoId = 1
  AND FechaMovimiento <= '2025-01-15 23:59:59'
ORDER BY FechaMovimiento DESC
LIMIT 1;
```

**Resultado:** Stock exacto en esa fecha.

---

## ?? Casos de Uso del Sistema de Auditoría

### Caso 1: Investigar Diferencia de Inventario
```
Problema: Stock físico (120) ? Stock sistema (100)

Consulta:
1. Ver todos los movimientos del producto
2. Filtrar por depósito
3. Revisar remitos asociados
4. Identificar ajustes manuales

Resultado: Se detectó ajuste negativo de -20 sin justificación
```

### Caso 2: Auditoría de Remito
```
Pregunta: ¿Qué productos se ingresaron en el remito REM-001?

Consulta:
SELECT p.Nombre, m.Cantidad, m.StockAnterior, m.StockNuevo
FROM MovimientosStock m
JOIN Productos p ON m.ProductoId = p.Id
WHERE m.RemitoIngresoId = 5;

Resultado: Lista completa con cambios de stock
```

### Caso 3: Historial de Usuario
```
Pregunta: ¿Qué movimientos hizo el usuario "jperez" hoy?

Consulta:
SELECT * FROM MovimientosStock
WHERE Usuario = 'jperez'
  AND FechaMovimiento >= CAST(GETDATE() AS DATE);

Resultado: Todas las acciones del usuario en el día
```

### Caso 4: Productos con Más Movimientos
```
Pregunta: ¿Cuáles son los 10 productos más movidos este mes?

Consulta:
SELECT 
    ProductoId, 
    COUNT(*) as CantMovimientos,
    SUM(ABS(Cantidad)) as TotalUnidades
FROM MovimientosStock
WHERE FechaMovimiento >= '2025-01-01'
GROUP BY ProductoId
ORDER BY CantMovimientos DESC
LIMIT 10;
```

---

## ?? Ventajas del Sistema

### 1. **Rastreabilidad Total**
- ? Cada movimiento queda registrado
- ? Se puede reconstruir el historial completo
- ? Identificar origen de discrepancias

### 2. **Auditoría Automática**
- ? No requiere intervención manual
- ? Registro en tiempo real
- ? No se puede omitir

### 3. **Integración con Remitos**
- ? Cada movimiento tiene su remito asociado
- ? Fácil identificar responsables
- ? Trazabilidad documento-stock

### 4. **Stock Histórico**
- ? Ver stock en cualquier fecha pasada
- ? Analizar tendencias
- ? Reportes históricos

### 5. **Detección de Problemas**
- ? Identificar ajustes anormales
- ? Detectar patrones sospechosos
- ? Alertas de stock negativo

---

## ?? Índices de Base de Datos

Para optimizar consultas:

```csharp
entity.HasIndex(e => e.FechaMovimiento);
entity.HasIndex(e => new { e.ProductoId, e.DepositoId });
entity.HasIndex(e => e.Tipo);
```

**Consultas optimizadas:**
- Por fecha
- Por producto + depósito
- Por tipo de movimiento

---

## ?? Endpoints Actualizados

| Método | Endpoint | Descripción | Auditoría |
|--------|----------|-------------|-----------|
| POST | `/api/remitos/ingreso` | Crear ingreso | ? Tipo: Ingreso |
| POST | `/api/remitos/egreso` | Crear egreso | ? Tipo: Egreso |
| PUT | `/api/remitos/ingreso/{id}` | Actualizar ingreso | ? No afecta stock |
| PUT | `/api/remitos/egreso/{id}` | Actualizar egreso | ? No afecta stock |
| POST | `/api/existencias/aumentar` | Ajuste positivo | ? Tipo: AjustePositivo |
| POST | `/api/existencias/disminuir` | Ajuste negativo | ? Tipo: AjusteNegativo |

---

## ? Estado de Implementación

```
??????????????????????????????????????????
?                                        ?
?  ? SISTEMA DE AUDITORÍA COMPLETO ?  ?
?                                        ?
?  ? Tabla MovimientoStock              ?
?  ? Registro Automático                ?
?  ? Rastreabilidad Total               ?
?  ? UpdateRemito (sin afectar stock)   ?
?  ? Solo IRepository (sin DbContext)   ?
?  ? Compilación: EXITOSA               ?
?                                        ?
??????????????????????????????????????????
```

---

## ?? Próximas Mejoras Sugeridas

### Reportes de Auditoría
- [ ] Endpoint: Movimientos por producto
- [ ] Endpoint: Movimientos por depósito
- [ ] Endpoint: Movimientos por período
- [ ] Endpoint: Movimientos por usuario
- [ ] Endpoint: Stock en fecha histórica

### Dashboard
- [ ] Total movimientos por tipo
- [ ] Productos con más movimientos
- [ ] Depósitos con más actividad
- [ ] Gráficos de tendencias

### Alertas
- [ ] Ajustes manuales excesivos
- [ ] Stock negativo
- [ ] Movimientos fuera de horario
- [ ] Productos sin movimiento

---

**¡Sistema de auditoría completo y funcionando!** ??

**Beneficio clave:** Ahora tienes trazabilidad completa de cada cambio en el stock, con remitos asociados y capacidad de reconstruir el historial completo.

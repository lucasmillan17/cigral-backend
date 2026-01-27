# Sistema de Remitos - Implementación Completa

## ? Estado: COMPLETADO

**Sistema completo de remitos de ingreso y egreso con integración automática de stock**

---

## ?? Archivos Creados/Modificados

### Entidades de Dominio (4 modificadas)
1. ? `RemitoProveedor.cs` ? **RemitoIngreso.cs**
2. ? `RemitoCliente.cs` ? **RemitoEgreso.cs**
3. ? `DetalleRemito.cs` - Agregado NumeroSerie y relaciones
4. ? `RemitoBase.cs` - Agregado DepositoId

### DTOs (1 nuevo)
5. ? `RemitoModel.cs` - RemitoRequest, RemitoDetalleRequest, RemitoResponse

### Servicios (2 nuevos)
6. ? `IRemitoService.cs` - Interfaz
7. ? `RemitoService.cs` - Implementación con transacciones

### Controlador (1 nuevo)
8. ? `RemitosController.cs` - 2 endpoints REST

### Configuración (2 modificados)
9. ? `CigralBackendContext.cs` - Actualizado para RemitoIngreso/Egreso
10. ? `Program.cs` - Registro de IRemitoService

---

## ?? Funcionalidades Implementadas

### 1. Remitos de Ingreso (Proveedores)
**Endpoint:** `POST /api/remitos/ingreso`

**Funcionalidad:**
- Registra entrada de mercadería de proveedores
- Crea remito con detalles
- **Aumenta automáticamente el stock** usando `ExistenciaService.AumentarStock()`
- Transacción de base de datos (commit/rollback automático)

**Validaciones:**
- ? Proveedor debe existir
- ? Depósito debe existir
- ? Debe tener al menos un detalle
- ? Número de remito único (opcional)
- ? Validaciones de stock del ExistenciaService

---

### 2. Remitos de Egreso (Clientes)
**Endpoint:** `POST /api/remitos/egreso`

**Funcionalidad:**
- Registra salida de mercadería a clientes
- Crea remito con detalles
- **Disminuye automáticamente el stock** usando `ExistenciaService.DisminuirStock()`
- Transacción de base de datos (commit/rollback automático)

**Validaciones:**
- ? Cliente debe existir
- ? Depósito debe existir
- ? Debe tener al menos un detalle
- ? Número de remito único (opcional)
- ? Stock suficiente (validado por ExistenciaService)

---

## ?? API REST

### POST /api/remitos/ingreso
Registra entrada de mercadería de proveedor.

**Request:**
```json
{
  "depositoId": 1,
  "entidadId": 5,
  "numeroRemito": "REM-ING-001",
  "observaciones": "Ingreso de mercadería mensual",
  "detalles": [
    {
      "productoId": 10,
      "loteId": 3,
      "numeroSerie": null,
      "cantidad": 100
    },
    {
      "productoId": 15,
      "loteId": null,
      "numeroSerie": "SN12345",
      "cantidad": 1
    }
  ]
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "numeroRemito": "REM-ING-001",
  "fecha": "2025-01-23T14:30:00",
  "depositoId": 1,
  "entidadId": 5,
  "observaciones": "Ingreso de mercadería mensual",
  "cantidadDetalles": 2,
  "cantidadTotal": 101
}
```

**Efectos:**
- Crea RemitoIngreso con detalles
- Aumenta stock en existencias (llama a `AumentarStock` por cada detalle)
- Si falla algo, hace rollback completo

---

### POST /api/remitos/egreso
Registra salida de mercadería a cliente.

**Request:**
```json
{
  "depositoId": 1,
  "entidadId": 8,
  "numeroRemito": "REM-EGR-001",
  "observaciones": "Venta a cliente",
  "detalles": [
    {
      "productoId": 10,
      "loteId": 3,
      "numeroSerie": null,
      "cantidad": 50
    }
  ]
}
```

**Response (201 Created):**
```json
{
  "id": 2,
  "numeroRemito": "REM-EGR-001",
  "fecha": "2025-01-23T14:35:00",
  "depositoId": 1,
  "entidadId": 8,
  "observaciones": "Venta a cliente",
  "cantidadDetalles": 1,
  "cantidadTotal": 50
}
```

**Efectos:**
- Crea RemitoEgreso con detalles
- Disminuye stock en existencias (llama a `DisminuirStock` por cada detalle)
- Si no hay stock suficiente, lanza DomainException y hace rollback

---

## ?? Flujo de Transacción

### Remito de Ingreso
```
1. Validar proveedor existe
2. Validar depósito existe
3. Validar número de remito único
4. COMENZAR TRANSACCIÓN
5. Crear RemitoIngreso
6. Por cada detalle:
   6.1. Crear DetalleRemito
   6.2. Llamar ExistenciaService.AumentarStock()
       - Si lote vencido ? Exception ? ROLLBACK
       - Si serie duplicada ? Exception ? ROLLBACK
       - Si producto unitario inválido ? Exception ? ROLLBACK
7. COMMIT TRANSACCIÓN
8. Retornar RemitoResponse
```

### Remito de Egreso
```
1. Validar cliente existe
2. Validar depósito existe
3. Validar número de remito único
4. COMENZAR TRANSACCIÓN
5. Crear RemitoEgreso
6. Por cada detalle:
   6.1. Crear DetalleRemito
   6.2. Llamar ExistenciaService.DisminuirStock()
       - Si stock insuficiente ? Exception ? ROLLBACK
       - Si existencia no existe ? Exception ? ROLLBACK
7. COMMIT TRANSACCIÓN
8. Retornar RemitoResponse
```

---

## ??? Validaciones y Manejo de Errores

### Validaciones Comunes
- ? Depósito debe existir ? `NotFoundException`
- ? Debe haber al menos un detalle ? `DomainException(RemitoSinDetalles)`
- ? Número de remito único ? `DomainException(NumeroRemitoDuplicado)`

### Validaciones de Ingreso
- ? Proveedor debe existir ? `NotFoundException`
- ? Validaciones de ExistenciaService:
  - Lote no vencido
  - Número de serie único
  - Producto unitario cantidad = 1

### Validaciones de Egreso
- ? Cliente debe existir ? `NotFoundException`
- ? Validaciones de ExistenciaService:
  - Stock suficiente
  - Existencia debe existir

### Transacciones
- ? Si **cualquier** operación falla ? **ROLLBACK COMPLETO**
- ? Garantiza consistencia de datos
- ? No quedan remitos sin stock actualizado

---

## ?? Modelos de Datos

### RemitoRequest
| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| DepositoId | int | ? | ID del depósito |
| EntidadId | int | ? | ProveedorId (ingreso) o ClienteId (egreso) |
| NumeroRemito | string? | ? | Número del remito (opcional, único) |
| Observaciones | string? | ? | Observaciones |
| Detalles | List | ? | Lista de detalles (min 1) |

### RemitoDetalleRequest
| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| ProductoId | int | ? | ID del producto |
| LoteId | int? | ? | ID del lote (opcional) |
| NumeroSerie | string? | ? | Número de serie (max 100 chars) |
| Cantidad | int | ? | Cantidad (min 1) |

### RemitoResponse
| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | int | ID del remito creado |
| NumeroRemito | string? | Número del remito |
| Fecha | DateTime | Fecha de creación |
| DepositoId | int | ID del depósito |
| EntidadId | int | ID de proveedor o cliente |
| Observaciones | string? | Observaciones |
| CantidadDetalles | int | Cantidad de líneas |
| CantidadTotal | int | Suma total de unidades |

---

## ?? Integración con ExistenciaService

### Mapeo de Datos
```csharp
// De RemitoDetalleRequest a ExistenciaModelRequest
var existenciaRequest = new ExistenciaModelRequest(
    DepositoId: request.DepositoId,        // Del remito
    ProductoId: detalle.ProductoId,        // Del detalle
    NumSerie: detalle.NumeroSerie,         // Del detalle
    LoteId: detalle.LoteId,                // Del detalle
    FechaVencimiento: null,                // Se toma del lote
    Cantidad: detalle.Cantidad             // Del detalle
);

// Ingreso
await _existenciaService.AumentarStock(existenciaRequest);

// Egreso
await _existenciaService.DisminuirStock(existenciaRequest);
```

---

## ?? Casos de Uso

### Caso 1: Recepción de Mercadería
```
Usuario: Recepcionista de almacén
Acción: Registrar ingreso de mercadería
Endpoint: POST /api/remitos/ingreso

Flujo:
1. Escanea código de proveedor
2. Selecciona depósito
3. Escanea códigos de productos
4. Sistema aumenta stock automáticamente
5. Imprime remito (fuera del alcance de esta implementación)
```

### Caso 2: Venta a Cliente
```
Usuario: Vendedor
Acción: Registrar salida de mercadería
Endpoint: POST /api/remitos/egreso

Flujo:
1. Selecciona cliente
2. Selecciona productos del pedido
3. Sistema valida stock disponible
4. Sistema disminuye stock automáticamente
5. Genera remito de egreso
```

### Caso 3: Error de Stock Insuficiente
```
Request: Egreso de 100 unidades
Stock actual: 50 unidades

Resultado:
? DomainException(StockInsuficiente)
? Rollback de transacción
? No se crea el remito
? Stock NO se modifica

Mensaje: "Stock insuficiente. Disponible: 50, Solicitado: 100"
```

---

## ?? Próximas Mejoras Sugeridas

### Funcionalidades Adicionales
- [ ] Consulta de remitos (GET por ID, lista paginada)
- [ ] Anulación de remitos (con reversión de stock)
- [ ] Modificación de remitos (antes de confirmar)
- [ ] Generación de PDF del remito
- [ ] Envío por email
- [ ] Tracking de estados (Borrador, Confirmado, Anulado)

### Validaciones Adicionales
- [ ] Validar GLN de proveedor/cliente
- [ ] Validar capacidad de depósito
- [ ] Alertas de stock mínimo
- [ ] Alertas de productos próximos a vencer

### Reportes
- [ ] Remitos por período
- [ ] Remitos por proveedor/cliente
- [ ] Movimientos de stock
- [ ] Productos más vendidos/comprados

---

## ? Checklist de Implementación

### Código
- [x] Entidades de dominio actualizadas
- [x] DTOs creados
- [x] Interfaz del servicio
- [x] Servicio implementado con transacciones
- [x] Controlador REST
- [x] DbContext actualizado
- [x] Registro en DI

### Validaciones
- [x] Validación de entidades relacionadas
- [x] Validación de número de remito único
- [x] Validación de detalles no vacíos
- [x] Integración con validaciones de ExistenciaService

### Transacciones
- [x] Begin transaction
- [x] Commit on success
- [x] Rollback on error
- [x] Manejo de excepciones

### Compilación
- [x] Build exitoso
- [x] Sin warnings críticos

---

## ?? Notas Importantes

### ?? No Implementado (Por Diseño)
- ? Generación de PDF
- ? Impresión
- ? Envío por email
- ? Consultas (GET)
- ? Modificación
- ? Anulación

### ? Implementado
- ? Registro de ingreso (POST)
- ? Registro de egreso (POST)
- ? Integración automática con stock
- ? Transacciones de BD
- ? Manejo de errores robusto
- ? Validaciones completas

---

**¡Sistema de remitos completo y listo para usar!** ??

**Total de endpoints:** 20 (6 productos + 6 marcas + 4 existencias + 2 remitos + 2 remitos)

**Estado:** ? Compilación exitosa, listo para testing

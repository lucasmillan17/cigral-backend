# ? Dashboard de Vencimientos - Implementación Completa

## ?? SISTEMA DE ALERTAS DE VENCIMIENTO IMPLEMENTADO

**Sistema completo para monitorear productos próximos a vencer con dashboard tipo semáforo**

---

## ?? Funcionalidades Implementadas

### 1. **Dashboard de Vencimientos** ??
Endpoint especializado para mostrar un semáforo visual en el frontend.

### 2. **Filtros de Vencimiento en Existencias** ??
Ampliación del endpoint de existencias con filtros de fecha de vencimiento.

### 3. **Query Personalizada** ??
Endpoint para consultas específicas de productos próximos a vencer.

---

## ?? Endpoints Nuevos

### 1. GET `/api/existencias/dashboard/vencimientos`

**Dashboard principal con estadísticas agrupadas por rangos.**

**Response:**
```json
{
  "fechaConsulta": "2025-01-24",
  "totalProductosProximosVencer": 156,
  "totalLotesProximosVencer": 45,
  "cantidadTotalProximaVencer": 2340,
  "rangos": [
    {
      "rango": "0-30 días",
      "diasMinimo": 0,
      "diasMaximo": 30,
      "totalProductos": 12,
      "totalLotes": 8,
      "cantidadTotal": 345,
      "items": [
        {
          "existenciaId": 101,
          "productoId": 5,
          "productoNombre": "Paracetamol 500mg",
          "productoGtin": "7790123456789",
          "depositoId": 1,
          "depositoNombre": "Depósito Central",
          "loteId": 23,
          "codigoLote": "LOTE-2024-A",
          "numeroSerie": null,
          "fechaVencimiento": "2025-02-15",
          "diasParaVencer": 22,
          "cantidad": 50
        }
      ]
    },
    {
      "rango": "31-60 días",
      "diasMinimo": 31,
      "diasMaximo": 60,
      "totalProductos": 25,
      "totalLotes": 12,
      "cantidadTotal": 580,
      "items": [...]
    },
    {
      "rango": "61-90 días",
      "diasMinimo": 61,
      "diasMaximo": 90,
      "totalProductos": 35,
      "totalLotes": 15,
      "cantidadTotal": 820,
      "items": [...]
    },
    {
      "rango": "91-120 días",
      "diasMinimo": 91,
      "diasMaximo": 120,
      "totalProductos": 42,
      "totalLotes": 18,
      "cantidadTotal": 1020,
      "items": [...]
    }
  ]
}
```

**Uso en Frontend (Ejemplo React):**
```jsx
const DashboardVencimientos = () => {
  const { data } = useDashboard();
  
  return (
    <div className="dashboard">
      <h2>Productos Próximos a Vencer</h2>
      <div className="stats">
        <div>Total Productos: {data.totalProductosProximosVencer}</div>
        <div>Total Lotes: {data.totalLotesProximosVencer}</div>
        <div>Cantidad Total: {data.cantidadTotalProximaVencer}</div>
      </div>
      
      {data.rangos.map(rango => (
        <div 
          key={rango.rango} 
          className={getSemaforoColor(rango.diasMaximo)}
        >
          <h3>{rango.rango}</h3>
          <p>Productos: {rango.totalProductos}</p>
          <p>Lotes: {rango.totalLotes}</p>
          <p>Cantidad: {rango.cantidadTotal}</p>
        </div>
      ))}
    </div>
  );
};

function getSemaforoColor(dias) {
  if (dias <= 30) return 'rojo'; // Crítico
  if (dias <= 90) return 'amarillo'; // Alerta
  return 'verde'; // OK
}
```

---

### 2. GET `/api/existencias/proximos-vencer`

**Query personalizada de productos próximos a vencer.**

**Parámetros:**
- `diasDesde` (int, opcional): Días desde hoy (ej: 0 = hoy)
- `diasHasta` (int, opcional): Días hasta (ej: 90 = 3 meses)
- `depositoId` (int, opcional): Filtrar por depósito
- `productoId` (int, opcional): Filtrar por producto
- `incluirVencidos` (bool, default: false): Incluir productos ya vencidos

**Ejemplos:**

#### Productos críticos (próximos 30 días)
```http
GET /api/existencias/proximos-vencer?diasDesde=0&diasHasta=30
```

**Response:**
```json
[
  {
    "existenciaId": 101,
    "productoId": 5,
    "productoNombre": "Paracetamol 500mg",
    "productoGtin": "7790123456789",
    "depositoId": 1,
    "depositoNombre": "Depósito Central",
    "loteId": 23,
    "codigoLote": "LOTE-2024-A",
    "numeroSerie": null,
    "fechaVencimiento": "2025-02-15",
    "diasParaVencer": 22,
    "cantidad": 50
  }
]
```

#### Productos que vencen en 1-3 meses
```http
GET /api/existencias/proximos-vencer?diasDesde=30&diasHasta=90
```

#### Productos vencidos
```http
GET /api/existencias/proximos-vencer?diasDesde=-365&diasHasta=-1&incluirVencidos=true
```

#### Por depósito específico
```http
GET /api/existencias/proximos-vencer?diasDesde=0&diasHasta=60&depositoId=1
```

---

### 3. GET `/api/existencias` (Actualizado)

**Endpoint existente con NUEVOS filtros de vencimiento.**

**Nuevos Parámetros:**
- `fechaVencimientoDesde` (DateTime, opcional)
- `fechaVencimientoHasta` (DateTime, opcional)
- `diasParaVencer` (int, opcional): Productos que vencen en X días
- `soloConVencimiento` (bool, opcional): true = solo con vencimiento, false = solo sin vencimiento

**Ejemplos:**

#### Productos que vencen en los próximos 90 días
```http
GET /api/existencias?diasParaVencer=90
```

#### Productos que vencen entre fechas específicas
```http
GET /api/existencias?fechaVencimientoDesde=2025-01-01&fechaVencimientoHasta=2025-03-31
```

#### Solo productos CON fecha de vencimiento
```http
GET /api/existencias?soloConVencimiento=true
```

#### Solo productos SIN fecha de vencimiento
```http
GET /api/existencias?soloConVencimiento=false
```

---

## ?? DTOs Creados

### ProductoProximoVencerDto
```csharp
public record ProductoProximoVencerDto
(
    int ExistenciaId,
    int ProductoId,
    string ProductoNombre,
    string ProductoGtin,
    int DepositoId,
    string DepositoNombre,
    int? LoteId,
    string? CodigoLote,
    string? NumeroSerie,
    DateTime FechaVencimiento,
    int DiasParaVencer,      // Calculado automáticamente
    int Cantidad
);
```

### VencimientoStats
```csharp
public record VencimientoStats
(
    string Rango,           // "0-30 días", "31-90 días", etc.
    int DiasMinimo,
    int DiasMaximo,
    int TotalProductos,     // Cantidad de productos ÚNICOS
    int TotalLotes,         // Cantidad de lotes ÚNICOS
    int CantidadTotal,      // SUMA de todas las cantidades
    List<ProductoProximoVencerDto> Items
);
```

### DashboardVencimientosResponse
```csharp
public record DashboardVencimientosResponse
(
    DateTime FechaConsulta,
    int TotalProductosProximosVencer,
    int TotalLotesProximosVencer,
    int CantidadTotalProximaVencer,
    List<VencimientoStats> Rangos
);
```

### VencimientoFilters
```csharp
public record VencimientoFilters
(
    int? DiasDesde,         // Ej: 0 (hoy)
    int? DiasHasta,         // Ej: 90 (3 meses)
    int? DepositoId,
    int? ProductoId,
    bool IncluirVencidos = false
);
```

---

## ?? Rangos del Dashboard

| Rango | Días | Color Sugerido | Prioridad |
|-------|------|----------------|-----------|
| **Vencidos** | < 0 | ?? Rojo oscuro | CRÍTICO |
| **0-30 días** | 0-30 | ?? Rojo | URGENTE |
| **31-60 días** | 31-60 | ?? Naranja | ALTA |
| **61-90 días** | 61-90 | ?? Amarillo | MEDIA |
| **91-120 días** | 91-120 | ?? Azul | BAJA |
| **121-180 días** | 121-180 | ?? Verde | NORMAL |

---

## ?? Casos de Uso

### Caso 1: Dashboard Principal

**Frontend muestra:**
```
???????????????????????????????????????????
?  ?? PRODUCTOS PRÓXIMOS A VENCER         ?
???????????????????????????????????????????
?  Total: 156 productos | 45 lotes        ?
?  Cantidad: 2,340 unidades               ?
???????????????????????????????????????????
?                                         ?
?  ?? 0-30 DÍAS        12 prod   345 un  ?
?  ?? 31-60 DÍAS       25 prod   580 un  ?
?  ?? 61-90 DÍAS       35 prod   820 un  ?
?  ?? 91-120 DÍAS      42 prod 1,020 un  ?
?  ?? 121-180 DÍAS     42 prod   595 un  ?
?                                         ?
?  [Ver Detalles] [Exportar PDF]         ?
???????????????????????????????????????????
```

**Request:**
```javascript
fetch('/api/existencias/dashboard/vencimientos')
  .then(res => res.json())
  .then(data => {
    // Renderizar semáforo
  });
```

---

### Caso 2: Alerta Automática

**Backend envía emails automáticos:**
```csharp
// Tarea programada diaria
var productosUrgentes = await _existenciaService.GetProductosProximosVencer(
    new VencimientoFilters(DiasDesde: 0, DiasHasta: 30)
);

if (productosUrgentes.Count > 0)
{
    await _emailService.EnviarAlerta(
        destinatario: "admin@cigral.com",
        asunto: $"ALERTA: {productosUrgentes.Count} productos vencen en 30 días",
        productos: productosUrgentes
    );
}
```

---

### Caso 3: Reporte de Vencidos

**Obtener productos ya vencidos:**
```http
GET /api/existencias/proximos-vencer?diasDesde=-365&diasHasta=-1&incluirVencidos=true
```

**Frontend muestra:**
```
???????????????????????????????????????????
?  ?? PRODUCTOS VENCIDOS                  ?
???????????????????????????????????????????
?  - Paracetamol 500mg (LOTE-A)          ?
?    Vencido hace: 15 días                ?
?    Cantidad: 50 unidades                ?
?    Depósito: Central                    ?
?                                         ?
?  - Ibuprofeno 400mg (LOTE-B)           ?
?    Vencido hace: 3 días                 ?
?    Cantidad: 30 unidades                ?
?    Depósito: Sucursal 1                 ?
???????????????????????????????????????????
```

---

### Caso 4: Filtros Combinados

**Productos del depósito 1 que vencen en 60 días:**
```http
GET /api/existencias?depositoId=1&diasParaVencer=60
```

**Productos específicos próximos a vencer:**
```http
GET /api/existencias/proximos-vencer?productoId=5&diasDesde=0&diasHasta=90
```

---

## ?? Lógica de Cálculo

### Días para Vencer
```csharp
var hoy = DateTime.Now.Date;
var fechaVencimiento = lote.FechaVencimiento;
var diasParaVencer = (int)(fechaVencimiento.Date - hoy).TotalDays;

// Ejemplos:
// Hoy: 2025-01-24
// Vence: 2025-02-24 ? diasParaVencer = 31
// Vence: 2025-01-20 ? diasParaVencer = -4 (vencido)
```

### Agrupación por Rangos
```csharp
var rangos = new[]
{
    new { Nombre = "0-30 días", Min = 0, Max = 30 },
    new { Nombre = "31-60 días", Min = 31, Max = 60 },
    // ...
};

// Filtrar items en cada rango
var itemsEnRango = productosProximosVencer
    .Where(p => p.DiasParaVencer >= rango.Min && p.DiasParaVencer <= rango.Max)
    .ToList();
```

---

## ?? Priorización de Vencimiento

### Sistema de Prioridades

```csharp
public enum PrioridadVencimiento
{
    Vencido,    // < 0 días
    Critica,    // 0-30 días
    Alta,       // 31-60 días
    Media,      // 61-90 días
    Baja,       // 91-120 días
    Normal      // > 120 días
}

// Calcular prioridad
PrioridadVencimiento GetPrioridad(int diasParaVencer)
{
    if (diasParaVencer < 0) return PrioridadVencimiento.Vencido;
    if (diasParaVencer <= 30) return PrioridadVencimiento.Critica;
    if (diasParaVencer <= 60) return PrioridadVencimiento.Alta;
    if (diasParaVencer <= 90) return PrioridadVencimiento.Media;
    if (diasParaVencer <= 120) return PrioridadVencimiento.Baja;
    return PrioridadVencimiento.Normal;
}
```

---

## ?? Ejemplos Frontend

### Dashboard con Cards
```jsx
const DashboardVencimientos = () => {
  const { data, loading } = useFetch('/api/existencias/dashboard/vencimientos');
  
  if (loading) return <Spinner />;
  
  return (
    <div className="dashboard-vencimientos">
      <div className="header">
        <h2>Control de Vencimientos</h2>
        <span>Fecha: {data.fechaConsulta}</span>
      </div>
      
      <div className="totales">
        <Card title="Productos" value={data.totalProductosProximosVencer} />
        <Card title="Lotes" value={data.totalLotesProximosVencer} />
        <Card title="Unidades" value={data.cantidadTotalProximaVencer} />
      </div>
      
      <div className="rangos">
        {data.rangos.map(rango => (
          <RangoCard key={rango.rango} rango={rango} />
        ))}
      </div>
    </div>
  );
};

const RangoCard = ({ rango }) => {
  const colorClass = getColorClass(rango.diasMaximo);
  
  return (
    <div className={`rango-card ${colorClass}`}>
      <h3>{rango.rango}</h3>
      <div className="stats">
        <div>{rango.totalProductos} productos</div>
        <div>{rango.totalLotes} lotes</div>
        <div>{rango.cantidadTotal} unidades</div>
      </div>
      <button onClick={() => verDetalles(rango.items)}>
        Ver Detalles ({rango.items.length})
      </button>
    </div>
  );
};
```

### Notificaciones
```jsx
const NotificacionesVencimiento = () => {
  const { data } = useFetch('/api/existencias/proximos-vencer?diasDesde=0&diasHasta=30');
  
  return (
    <div className="notificaciones">
      <h3>?? Productos Críticos ({data.length})</h3>
      {data.map(producto => (
        <div key={producto.existenciaId} className="alerta">
          <strong>{producto.productoNombre}</strong>
          <span>Vence en {producto.diasParaVencer} días</span>
          <span>Cantidad: {producto.cantidad}</span>
        </div>
      ))}
    </div>
  );
};
```

---

## ? Ventajas del Sistema

### Para el Negocio
? **Prevenir pérdidas** - Detectar productos antes del vencimiento
? **Optimizar rotación** - Vender primero lo que vence antes
? **Reducir desperdicio** - Planificar promociones o descuentos
? **Cumplir normativas** - No vender productos vencidos

### Para el Usuario
? **Dashboard visual** - Semáforo intuitivo
? **Alertas automáticas** - Notificaciones proactivas
? **Filtros flexibles** - Consultas personalizadas
? **Datos completos** - Producto, lote, depósito, cantidad

### Técnico
? **Performance** - Cálculo en servidor, no en frontend
? **Escalable** - Funciona con miles de productos
? **Flexible** - Múltiples endpoints para diferentes casos
? **Documentado** - Swagger con ejemplos

---

## ?? Próximas Mejoras (Opcionales)

### Funcionalidades Futuras
- [ ] **Exportar a Excel/PDF** - Reportes de vencimientos
- [ ] **Emails automáticos** - Alertas programadas
- [ ] **Gráficos** - Visualización con Chart.js
- [ ] **Historial** - Tracking de productos vencidos
- [ ] **Configuración** - Rangos personalizables por usuario
- [ ] **Push Notifications** - Alertas en tiempo real

---

## ? Estado Final

```
??????????????????????????????????????????????
?                                            ?
?   ?? DASHBOARD DE VENCIMIENTOS ?         ?
?                                            ?
?  ? Dashboard:         Implementado       ?
?  ? Filtros:           3 tipos            ?
?  ? DTOs:              4 nuevos           ?
?  ? Endpoints:         3 (1 nuevo, 2 mod) ?
?  ? Rangos:            6 rangos           ?
?  ? Compilación:       EXITOSA            ?
?  ? Documentación:     Completa           ?
?                                            ?
??????????????????????????????????????????????
```

---

**¡Sistema de control de vencimientos listo para producción!** ??

**Ideal para:** Farmacias, alimentos, productos perecederos, cualquier negocio con productos que vencen.

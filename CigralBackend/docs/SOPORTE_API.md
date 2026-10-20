# API de soporte

Endpoints de tickets y de las herramientas de corrección. La guía de uso desde la app está en el repo del front: `docs/SOPORTE.md`.
Se actualiza con cada herramienta nueva.

## Roles
- Los roles son de ASP.NET Identity: `Administrador` y `Soporte` (`CigralBackend.Domain.Roles`). Viajan en el JWT como `ClaimTypes.Role`.
- Al arrancar, la API crea los roles si no existen y le asigna `Administrador` a todo usuario sin rol.
- `Soporte` se asigna a mano: SQL en `DEPLOY_DOCKER.md`, o `POST api/Auth/usuarios/{id}/roles` (solo Soporte).

## Tickets — `TicketsController`

| Método | Ruta | Quién | Qué hace |
|---|---|---|---|
| POST | `api/Tickets` | Autenticado | Multipart: `datos` (JSON de `TicketCreateRequest`) + `fotos` (hasta 5 imágenes de 3 MB). Manda el mail a soporte en segundo plano. |
| GET | `api/Tickets` | Autenticado | Lista. Soporte ve todos; el resto, los propios. Filtros: `estado`, `tipo`, `texto`, paginación. |
| GET | `api/Tickets/pendientes` | Autenticado | Cantidad de abiertos y en revisión (contador del menú). |
| GET | `api/Tickets/{id}` | Creador o Soporte | Detalle con comentarios y adjuntos. Un ticket ajeno da 404. |
| GET | `api/Tickets/{id}/adjuntos/{adjuntoId}` | Creador o Soporte | La imagen. |
| POST | `api/Tickets/{id}/comentarios` | Creador o Soporte | Agrega un comentario. |
| PUT | `api/Tickets/{id}/estado` | Soporte | Cambia el estado. |

**Mail:** variables `Smtp__Host`, `Smtp__Port`, `Smtp__Usuario`, `Smtp__Password`, `Smtp__Remitente` y `Soporte__EmailDestino` (ver `DEPLOY_DOCKER.md`). Si faltan, el ticket se guarda igual.

## Reporte de problemas de la interfaz (`tipo: "Interfaz"`)
Un ticket de tipo `Interfaz` (`TipoTicket.Interfaz = 2`) se crea con el mismo `POST api/Tickets`. No lleva `codigoCrudo` ni datos del parser; la captura de pantalla viaja como una de las `fotos`.

Campo nuevo de `TicketCreateRequest` (al final, opcional): `contextoJson` (texto, máximo 20000 caracteres; más da 400). Se guarda en `Tickets.ContextoJson` y vuelve en `TicketDetalleResponse.contextoJson`. La API no lo valida: es el JSON que arma la app y puede cambiar sin migrar. Forma actual:

```json
{
  "version": "1.0.0",
  "pantalla": { "ruta": "/ingresos", "titulo": "Ingresos" },
  "usuario": "Antonella",
  "ventana": { "ancho": 1280, "alto": 720 },
  "monitor": { "ancho": 1920, "alto": 1080 },
  "zoom": 1.25,
  "tema": "claro",
  "userAgent": "...",
  "errorReportado": "mensaje del error desde el que se abrió el reporte (opcional)",
  "eventos": [
    { "hora": "10:00:05", "tipo": "api", "detalle": { "metodo": "POST", "ruta": "/api/Remitos/ingreso", "estado": 400, "mensaje": "..." } },
    { "hora": "10:00:09", "tipo": "js", "detalle": { "mensaje": "..." } },
    { "hora": "10:00:10", "tipo": "navegacion", "detalle": { "ruta": "/egresos" } }
  ]
}
```

- `eventos` son los últimos 25, del más viejo al más nuevo. `tipo` es `api`, `js`, `promesa` o `navegacion`. De la API se guarda solo método, ruta sin la query, estado y mensaje: nunca bodies, headers ni tokens.
- `zoom` es `devicePixelRatio`: sirve para reproducir problemas de monitores con escala de Windows.
- **Mail:** el asunto es `[Soporte #N · Interfaz] título`. El cuerpo trae título, usuario, fecha, pantalla y versión, la descripción, una tabla con resolución, zoom, tema y pantalla, los últimos 5 errores (los eventos que no son de navegación) y "Captura adjunta". Todo es opcional: un `contextoJson` inválido o con otra forma no impide el envío (se muestra lo que se pueda leer).
- `GET api/Tickets?tipo=Interfaz` filtra la bandeja por tipo.

## Reglas comunes de las correcciones
- Todas son `[Authorize(Roles = Roles.Soporte)]`.
- Piden `ticketId` o `motivo` (máximo 500 caracteres). Si faltan los dos: 400 `CorreccionSinMotivo`. Si no hay cambios: 400 `CorreccionSinCambios`.
- Corren en **una transacción**: o se aplica todo o nada.
- Los movimientos de stock son `AjustePositivo` o `AjusteNegativo`, con la observación `Corrección de soporte · [Remito N ·] [Ticket #T ·] motivo`.
- La auditoría automática (`CigralBackendContext.SaveChangesAsync`) registra los cambios campo por campo, y también los **borrados** con los valores de la fila. Un campo que queda vacío se registra como "(vacío)".
- Con ticket, se agrega un `TicketComentario` de soporte con el antes y el después (`CorreccionSoporte.Comentar`).
- Las piezas comunes están en `CigralBack.Api/Services/CorreccionSoporte.cs`.

## Corregir existencia
`PUT api/Existencias/{id}/correccion` — `ExistenciaCorreccionRequest`:
```json
{ "codigoLote": "L123", "numSerie": null, "fechaVencimiento": "2027-05-31", "depositoId": 1, "cantidad": 8, "ticketId": 3, "motivo": "conteo físico" }
```
- Se manda el **estado final completo**.
- Si la nueva combinación (producto + depósito + lote + serie) ya existe en otra existencia, **se suman**: las consignaciones pasan a la que queda y la original se elimina. La respuesta es la existencia resultante.
- Errores:
  - `StockEnConsignacion`: la cantidad quedaría por debajo de lo consignado.
  - `ProductoUnitarioCantidadInvalida`: más de 1 unidad de un producto unitario.
  - `SerieYCodigoLoteNoEspecificados`: sin serie ni lote, y el producto no tiene código genérico.
- Servicio: `ExistenciaService.CorregirExistencia`. La parte de mover o fusionar una existencia a otra clave es `ExistenciaService.MoverExistencia` (la reutiliza la corrección de lote).

## Corregir remitos — `RemitosController`
`{tipo}` = `ingreso` | `egreso`. Las tres devuelven el remito corregido (`RemitoResponseGet`).

| Método | Ruta | Body |
|---|---|---|
| PUT | `api/Remitos/{tipo}/{id}/detalles/{detalleId}/correccion` | `CorreccionLineaRequest`: `codigoLote`, `numeroSerie`, `fechaVencimiento`, `cantidad` (0 = quitar), `ajustarStock` (por defecto true), `ticketId`, `motivo` |
| POST | `api/Remitos/{tipo}/{id}/detalles/correccion` | `AgregarLineaRequest`: lo mismo, más `productoId` |
| PUT | `api/Remitos/{tipo}/{id}/entidad/correccion` | `CorreccionEntidadRequest`: `entidadId` (cliente o proveedor), `ticketId`, `motivo`. No mueve stock. |

**Stock con `ajustarStock`** (`CorreccionRemitoService.AjustarStock`). Se pasa del efecto viejo de la línea al nuevo:
- **Ingreso:**
  - se resta lo viejo de la existencia exacta en el depósito del remito (`ExistenciaService.AjustarExistenciaExacta`);
  - después se suma lo nuevo;
  - si lo que hay que restar ya no está disponible (salió o está consignado): 400 `CorreccionStockYaMovido`, y nada cambia. La app ofrece reintentar con `ajustarStock: false`.
- **Egreso:**
  - se devuelve lo viejo al depósito del remito;
  - lo nuevo se descuenta con `DisminuirStock` (cualquier depósito, respetando las consignaciones);
  - si no alcanza: 400 `StockInsuficiente`.
- **Misma clave** (solo cambió la cantidad o el vencimiento): se ajusta solo la diferencia.

Otros errores: quitar la única línea da 400 `RemitoSinDetalles`, y un tipo inválido da 400.

`GET api/Remitos/ingreso|egreso` devuelve ahora, en cada línea, `id` y `productoNombre`.

## Corregir lote completo — `LotesController`
Corrige **de una vez, para un producto**, un lote mal leído: renombra el código (si el código nuevo ya existe para ese producto, los lotes se fusionan) y/o cambia el vencimiento, en todas sus existencias y líneas de remito. `Lote` es solo un código compartido entre productos (índice único en `CodigoLote`), así que **nunca se renombra el registro**: se reasignan los `LoteId` de las existencias y líneas de ese producto a otro `Lote`, que se busca o se crea. Los demás productos con el mismo código no se tocan.

| Método | Ruta | Qué hace |
|---|---|---|
| GET | `api/Lotes/correccion/vista-previa?productoId=&codigoLote=&codigoNuevo=` | Qué va a pasar, sin cambiar nada (`CorreccionLotePrevia`). |
| PUT | `api/Lotes/correccion` | Aplica la corrección. Body `CorreccionLoteRequest`. Devuelve `CorreccionLoteResponse`. |

```json
{ "productoId": 268, "codigoLote": "L1234", "codigoNuevo": "L12345", "cambiarVencimiento": true, "fechaVencimiento": "2027-12-31", "ticketId": 3, "motivo": "lote mal leído" }
```
- `codigoNuevo` vacío o igual al actual: el código no cambia (el código se normaliza a mayúsculas).
- `cambiarVencimiento` en false: cada existencia y línea conserva su vencimiento. En true se pone `fechaVencimiento` en todas (null = sin vencimiento).
- **Vista previa:** existencias afectadas (depósito, serie, vencimiento, cantidad, consignado), con `fusionaConExistenciaId` si se van a sumar a otra del código nuevo (mismo depósito y serie); cantidad de líneas de remito de ingreso y de egreso, con el número de cada remito; y `fusionaLotes` si el código nuevo ya existe para el producto.
- **Existencias** (`CorreccionLoteService` + `ExistenciaService.MoverExistencia`, la misma lógica que usa `CorregirExistencia`):
  - si ya hay una existencia con la clave nueva, se suman las cantidades, las consignaciones pasan a ella y la original se borra (una destino con vencimiento propio lo conserva si no se pidió cambiarlo);
  - si no, se cambia el `LoteId` en el lugar;
  - cada una registra un `AjusteNegativo` en la clave vieja y un `AjustePositivo` en la nueva, con la observación `Corrección de soporte · Lote A → B · Ticket #T · motivo`;
  - un cambio solo de vencimiento no genera movimientos.
- **Líneas de remito** (`DetalleRemito`) del producto con ese lote: se les cambia `LoteId` y, si corresponde, `FechaVencimiento`. No mueven stock (ya se movió con las existencias).
- Los `MovimientoStock` históricos **no se tocan**: son historia. La corrección queda en los ajustes nuevos y en la auditoría.
- Con ticket, un solo comentario de soporte con el resumen, por ejemplo: "Lote L1234 de {producto} corregido a L12345: 3 existencias (1 fusionada), 7 líneas de remito; vencimiento 31/12/2026 → 31/12/2027."
- Errores:
  - `CorreccionSinMotivo` (400): faltan ticket y motivo.
  - `CorreccionSinCambios` (400): el código es el mismo y el vencimiento ya es ese.
  - `ProductoUnitarioCantidadInvalida` (400): la fusión dejaría más de 1 unidad con la misma serie de un producto unitario.
  - 404: el producto o el ticket no existen, o el producto no tiene existencias ni líneas con ese lote.
- Rol Administrador: 403 en los dos endpoints.

## Códigos de error de soporte (`DomainErrorCode`)
| Código | Valor | Cuándo |
|---|---|---|
| `CorreccionSinCambios` | 3010 | La corrección no cambia nada. |
| `CorreccionSinMotivo` | 3011 | Falta el ticket o el motivo. |
| `CorreccionStockYaMovido` | 3012 | Hay que sacar stock que ya no está. |

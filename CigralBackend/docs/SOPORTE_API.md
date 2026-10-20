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
- Servicio: `ExistenciaService.CorregirExistencia`.

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

## Códigos de error de soporte (`DomainErrorCode`)
| Código | Valor | Cuándo |
|---|---|---|
| `CorreccionSinCambios` | 3010 | La corrección no cambia nada. |
| `CorreccionSinMotivo` | 3011 | Falta el ticket o el motivo. |
| `CorreccionStockYaMovido` | 3012 | Hay que sacar stock que ya no está. |

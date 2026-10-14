# Revisión de vencimientos heredados entre lotes

Hasta la migración `LotesSoloCodigo`, el lote guardaba producto y vencimiento. Cuando un código de
lote ya existía para otro producto (los fabricantes repiten códigos), la existencia nueva tomaba el
vencimiento del lote existente, es decir, el del **otro** producto. En el backup del 12/10/2026 había
48 existencias en esa situación.

Desde esa migración el lote es solo el código y cada existencia guarda su propio vencimiento, así
que el problema no se repite. Las existencias afectadas antes conservan la fecha heredada: el dato
original no quedó guardado en ningún lado, por lo que hay que revisarlas a mano.

## Consulta

Lista las existencias cuyo lote también lo usa otro producto **con el mismo vencimiento**, que es la
huella de una fecha heredada. Aparecen los dos lados de cada caso (el producto original y el que
heredó la fecha); hay que verificar contra la mercadería o el remito del proveedor.

```sql
-- Existencias cuyo lote lo usa otro producto con el mismo vencimiento (posible fecha heredada)
SELECT l."CodigoLote", e."FechaVencimiento"::date AS vencimiento, p."Nombre" AS producto,
       d."Nombre" AS deposito, e."Cantidad", e."Id" AS existencia_id
FROM "Existencias" e
JOIN "Lotes" l ON l."Id" = e."LoteId"
JOIN "Productos" p ON p."Id" = e."ProductoId"
JOIN "Depositos" d ON d."Id" = e."DepositoId"
WHERE EXISTS (
    SELECT 1 FROM "Existencias" o
    WHERE o."LoteId" = e."LoteId" AND o."ProductoId" <> e."ProductoId"
      AND o."FechaVencimiento" IS NOT DISTINCT FROM e."FechaVencimiento")
ORDER BY l."CodigoLote", p."Nombre";
```

Para corregir una existencia:

```sql
UPDATE "Existencias" SET "FechaVencimiento" = '2027-05-31' WHERE "Id" = <existencia_id>;
```

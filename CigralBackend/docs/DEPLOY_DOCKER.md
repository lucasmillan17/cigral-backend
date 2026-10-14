# Deploy con Docker (Easypanel) y PostgreSQL

## Imagen

El `Dockerfile` está en la raíz del repo. Compila la API con .NET 8 y la publica en una imagen
`aspnet:8.0` que escucha en el puerto **8080** (HTTP; el HTTPS lo pone Easypanel).

Para probarla localmente (requiere Docker):

```bash
docker build -t cigral-api .
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=cigral;Username=postgres;Password=..." \
  -e Jwt__Key="..." \
  cigral-api
```

Swagger queda en `http://localhost:8080/swagger`.

## Easypanel

1. **Servicio PostgreSQL**: crear un servicio *Postgres* en el proyecto. Anotar host interno,
   usuario y contraseña. La extensión `citext` viene incluida en la imagen oficial.
2. **Servicio App**:
   - *Source*: GitHub → `lucasmillan17/cigral-backend`, rama a desplegar.
   - *Build*: Dockerfile (ruta `Dockerfile`).
   - *Domains*: `test-api.cigral.com.ar` → puerto `8080`.
   - *Environment*:

     ```
     ConnectionStrings__DefaultConnection=Host=<host interno postgres>;Port=5432;Database=cigral;Username=<usuario>;Password=<contraseña>
     Jwt__Key=<clave aleatoria de 64+ caracteres, distinta a la de appsettings.json>
     AuthHeader__Key=<valor del header x-cigral-auth que envía el cliente de escritorio>
     Swagger__Habilitado=true   # opcional: solo en el entorno de pruebas
     ```

Al arrancar, la API aplica las migraciones pendientes y, si la base está vacía, crea el usuario
administrador inicial.

## Deploy automático (GitHub Actions)

`.github/workflows/deploy.yml` se ejecuta cada vez que entra código a `development` o `main`
(merge de un pull request o push directo): compila la API y, si compila, llama al webhook de
deploy de Easypanel.

| Rama | Secret de GitHub con el webhook | Servicio |
|---|---|---|
| `development` | `EASYPANEL_DEPLOY_URL_DEVELOPMENT` | test-api.cigral.com.ar |
| `main` | `EASYPANEL_DEPLOY_URL_MAIN` | producción (pendiente) |

El webhook se obtiene en Easypanel, en el servicio de la API → *Deployments* → *Deploy webhook*.
Lleva un token, así que va como secret y nunca en el código. Si el secret de una rama no está
definido, el workflow compila igual pero no despliega (queda una advertencia en la ejecución).

## Seguridad

- **Header `x-cigral-auth`**: si `AuthHeader__Key` está definida, la API rechaza con 401 toda
  petición que no traiga ese header con el valor correcto (antes lo validaba Cloudflare). Sin la
  variable no se exige, como en desarrollo local.
- **Login**: máximo 10 intentos por minuto por IP (429 al superarlo) y bloqueo del usuario por
  5 minutos tras 5 contraseñas incorrectas.
- **Swagger**: solo se publica en desarrollo o con `Swagger__Habilitado=true`.
- La IP del cliente se toma de `X-Forwarded-For`, que agrega el proxy de Easypanel. Si se vuelve a
  activar el proxy de Cloudflare, el límite por IP pasa a contar por nodo de Cloudflare.

## Migrar datos desde SQL Server

La base de producción actual está en SQL Server. Para pasar sus datos a PostgreSQL:

1. Restaurar el backup (`.bak`) en un SQL Server accesible.
2. Crear el esquema vacío en PostgreSQL:

   ```bash
   dotnet ef database update --project CigralBackend.Infraestructure --startup-project CigralBackend \
     --connection "Host=...;Database=cigral;Username=...;Password=..."
   ```

   No levantar la API antes de copiar los datos: crearía el usuario administrador y la base
   ya no estaría vacía.

3. Copiar los datos:

   ```bash
   dotnet run --project tools/MigradorSqlServerPostgres -- \
     "Server=localhost\SQLEXPRESS;Database=CigralRestore;Integrated Security=True;TrustServerCertificate=True" \
     "Host=...;Database=cigral;Username=...;Password=..."
   ```

   La herramienta verifica que el destino esté vacío, copia todo en una transacción, reajusta
   las secuencias de los Id y compara la cantidad de filas de cada tabla. El usuario de
   PostgreSQL tiene que ser superusuario (desactiva las FK durante la carga).

## Diferencias con SQL Server a tener en cuenta

- **Mayúsculas**: todas las columnas de texto son `citext`, así que búsquedas, comparaciones e
  índices únicos no distinguen mayúsculas, igual que antes.
- **Fechas**: `DateTime` se guarda como `timestamp` sin zona horaria
  (`Npgsql.EnableLegacyTimestampBehavior` en `Program.cs`), igual que `datetime2`.
- **Nombres**: las tablas y columnas conservan mayúsculas (`"Productos"`, `"Id"`), así que en SQL
  escrito a mano hay que ponerlas entre comillas dobles.
- **Migraciones**: las de SQL Server se reemplazaron por `InicialPostgres`. Quedan en el
  historial de git (rama `tests`) por si hiciera falta consultarlas.

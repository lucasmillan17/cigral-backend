// Copia los datos de la base SQL Server de Cigral a PostgreSQL.
//
// Requisitos:
//   1. La base PostgreSQL destino ya tiene el esquema creado con las migraciones de EF
//      (dotnet ef database update) y está vacía.
//   2. La base SQL Server origen está en la misma versión de esquema
//      (última migración: 20260422003808_RegistrosAuditoria1).
//
// Uso:
//   dotnet run --project tools/MigradorSqlServerPostgres -- "<cadena SQL Server>" "<cadena PostgreSQL>"
//
// Copia tabla por tabla las columnas con el mismo nombre, desactiva las FK durante la carga
// (requiere un usuario superusuario en PostgreSQL), reajusta las secuencias de los Id y
// verifica que la cantidad de filas coincida.

using System.Data;
using Microsoft.Data.SqlClient;
using Npgsql;
using NpgsqlTypes;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

if (args.Length != 2)
{
    Console.Error.WriteLine("Uso: MigradorSqlServerPostgres \"<cadena SQL Server>\" \"<cadena PostgreSQL>\"");
    return 1;
}

await using var sql = new SqlConnection(args[0]);
await using var pg = new NpgsqlConnection(args[1]);
await sql.OpenAsync();
await pg.OpenAsync();

// Tablas destino (las crea EF). El historial de migraciones no se copia: es distinto en PostgreSQL.
var tablas = new List<string>();
await using (var cmd = new NpgsqlCommand(
    "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE' AND table_name <> '__EFMigrationsHistory' ORDER BY table_name", pg))
await using (var r = await cmd.ExecuteReaderAsync())
    while (await r.ReadAsync()) tablas.Add(r.GetString(0));

foreach (var tabla in tablas)
{
    await using var check = new NpgsqlCommand($"SELECT EXISTS (SELECT 1 FROM \"{tabla}\")", pg);
    if ((bool)(await check.ExecuteScalarAsync())!)
    {
        Console.Error.WriteLine($"La tabla destino \"{tabla}\" no está vacía. Abortando para no duplicar datos.");
        return 1;
    }
}

await using var tx = await pg.BeginTransactionAsync();

// Desactiva triggers de FK durante la carga para no depender del orden de las tablas.
await using (var cmd = new NpgsqlCommand("SET session_replication_role = replica", pg, tx))
    await cmd.ExecuteNonQueryAsync();

var resumen = new List<(string Tabla, long Origen, long Destino)>();

foreach (var tabla in tablas)
{
    var columnasPg = await ColumnasPostgres(pg, tx, tabla);
    var columnasSql = await ColumnasSqlServer(sql, tabla);
    var columnas = columnasPg.Where(c => columnasSql.Contains(c.Nombre, StringComparer.OrdinalIgnoreCase)).ToList();

    var faltantes = columnasPg.Where(c => !c.Nullable && !c.TieneDefault && !columnas.Contains(c)).Select(c => c.Nombre).ToList();
    if (faltantes.Count > 0)
        throw new InvalidOperationException($"{tabla}: columnas obligatorias sin equivalente en SQL Server: {string.Join(", ", faltantes)}");

    var listaPg = string.Join(", ", columnas.Select(c => $"\"{c.Nombre}\""));
    var listaSql = string.Join(", ", columnas.Select(c => $"[{c.Nombre}]"));

    long filas = 0;
    await using (var leer = new SqlCommand($"SELECT {listaSql} FROM [{tabla}]", sql) { CommandTimeout = 0 })
    await using (var reader = await leer.ExecuteReaderAsync(CommandBehavior.SequentialAccess))
    await using (var copy = await pg.BeginBinaryImportAsync($"COPY \"{tabla}\" ({listaPg}) FROM STDIN (FORMAT BINARY)"))
    {
        while (await reader.ReadAsync())
        {
            await copy.StartRowAsync();
            for (var i = 0; i < columnas.Count; i++)
            {
                var valor = reader.GetValue(i);
                if (valor is DBNull) await copy.WriteNullAsync();
                else await copy.WriteAsync(Convertir(valor, columnas[i].Tipo), columnas[i].Tipo);
            }
            filas++;
        }
        await copy.CompleteAsync();
    }

    await using (var contar = new SqlCommand($"SELECT COUNT_BIG(*) FROM [{tabla}]", sql))
    {
        var origen = (long)(await contar.ExecuteScalarAsync())!;
        resumen.Add((tabla, origen, filas));
    }

    // Reajusta la secuencia de la columna identidad para que los próximos Id no choquen.
    if (columnas.Any(c => c.Nombre == "Id" && c.EsIdentidad))
    {
        await using var seq = new NpgsqlCommand(
            $"SELECT setval(pg_get_serial_sequence('\"{tabla}\"', 'Id'), COALESCE((SELECT MAX(\"Id\") FROM \"{tabla}\"), 0) + 1, false)", pg, tx);
        await seq.ExecuteScalarAsync();
    }

    Console.WriteLine($"  {tabla,-25} {filas,8} filas");
}

await using (var cmd = new NpgsqlCommand("SET session_replication_role = DEFAULT", pg, tx))
    await cmd.ExecuteNonQueryAsync();

var diferencias = resumen.Where(r => r.Origen != r.Destino).ToList();
if (diferencias.Count > 0)
{
    foreach (var d in diferencias) Console.Error.WriteLine($"Diferencia en {d.Tabla}: origen {d.Origen}, destino {d.Destino}");
    await tx.RollbackAsync();
    return 1;
}

await tx.CommitAsync();
Console.WriteLine($"Listo: {resumen.Sum(r => r.Destino)} filas copiadas en {resumen.Count} tablas.");
return 0;

static object Convertir(object valor, NpgsqlDbType tipo) => (valor, tipo) switch
{
    (DateTimeOffset dto, NpgsqlDbType.Timestamp) => dto.DateTime,
    (Guid g, NpgsqlDbType.Text or NpgsqlDbType.Citext) => g.ToString(),
    (byte b, NpgsqlDbType.Integer) => (int)b,
    (short s, NpgsqlDbType.Integer) => (int)s,
    _ => valor,
};

static async Task<List<string>> ColumnasSqlServer(SqlConnection sql, string tabla)
{
    var lista = new List<string>();
    await using var cmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @t", sql);
    cmd.Parameters.AddWithValue("@t", tabla);
    await using var r = await cmd.ExecuteReaderAsync();
    while (await r.ReadAsync()) lista.Add(r.GetString(0));
    if (lista.Count == 0) throw new InvalidOperationException($"La tabla {tabla} no existe en SQL Server.");
    return lista;
}

static async Task<List<ColumnaPg>> ColumnasPostgres(NpgsqlConnection pg, NpgsqlTransaction tx, string tabla)
{
    var lista = new List<ColumnaPg>();
    await using var cmd = new NpgsqlCommand(
        """
        SELECT column_name, udt_name, is_nullable = 'YES', column_default IS NOT NULL OR is_identity = 'YES', is_identity = 'YES'
        FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name = @t
        ORDER BY ordinal_position
        """, pg, tx);
    cmd.Parameters.AddWithValue("t", tabla);
    await using var r = await cmd.ExecuteReaderAsync();
    while (await r.ReadAsync())
        lista.Add(new ColumnaPg(r.GetString(0), MapearTipo(r.GetString(1)), r.GetBoolean(2), r.GetBoolean(3), r.GetBoolean(4)));
    return lista;
}

static NpgsqlDbType MapearTipo(string udt) => udt switch
{
    "citext" => NpgsqlDbType.Citext,
    "text" or "varchar" => NpgsqlDbType.Text,
    "int2" => NpgsqlDbType.Smallint,
    "int4" => NpgsqlDbType.Integer,
    "int8" => NpgsqlDbType.Bigint,
    "bool" => NpgsqlDbType.Boolean,
    "numeric" => NpgsqlDbType.Numeric,
    "float8" => NpgsqlDbType.Double,
    "float4" => NpgsqlDbType.Real,
    "timestamp" => NpgsqlDbType.Timestamp,
    "timestamptz" => NpgsqlDbType.TimestampTz,
    "date" => NpgsqlDbType.Date,
    "uuid" => NpgsqlDbType.Uuid,
    "bytea" => NpgsqlDbType.Bytea,
    _ => throw new NotSupportedException($"Tipo PostgreSQL no contemplado: {udt}"),
};

record ColumnaPg(string Nombre, NpgsqlDbType Tipo, bool Nullable, bool TieneDefault, bool EsIdentidad);

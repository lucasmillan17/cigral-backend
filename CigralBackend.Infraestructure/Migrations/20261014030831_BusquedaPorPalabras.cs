using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigralBackend.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class BusquedaPorPalabras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

            // Usada por Busqueda.Coincide: cada palabra del texto tiene que aparecer en el campo,
            // en cualquier orden y sin distinguir mayúsculas ni acentos. Texto vacío = coincide.
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION coincide_busqueda(campo text, texto text)
                RETURNS boolean
                LANGUAGE sql STABLE PARALLEL SAFE
                AS $$
                    SELECT CASE
                        WHEN texto IS NULL OR btrim(texto) = '' THEN true
                        WHEN campo IS NULL THEN false
                        ELSE NOT EXISTS (
                            SELECT 1
                            FROM regexp_split_to_table(lower(public.unaccent(btrim(texto))), '\s+') AS palabra
                            WHERE strpos(lower(public.unaccent(campo)), palabra) = 0
                        )
                    END
                $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS coincide_busqueda(text, text);");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:unaccent", ",,");
        }
    }
}

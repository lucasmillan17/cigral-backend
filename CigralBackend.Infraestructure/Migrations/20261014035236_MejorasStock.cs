using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigralBackend.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class MejorasStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantidadDisponible",
                table: "Lotes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaVencimiento",
                table: "Lotes",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            // Auditoría y consignaciones se guardaban en UTC y el resto en hora de Argentina.
            // Ahora todo se guarda en hora de Argentina (UTC-3, sin horario de verano): se corrige lo existente.
            migrationBuilder.Sql(@"UPDATE ""Auditorias"" SET ""Fecha"" = ""Fecha"" - INTERVAL '3 hours';");
            migrationBuilder.Sql(@"UPDATE ""Consignaciones"" SET ""FechaModificacion"" = ""FechaModificacion"" - INTERVAL '3 hours';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE ""Auditorias"" SET ""Fecha"" = ""Fecha"" + INTERVAL '3 hours';");
            migrationBuilder.Sql(@"UPDATE ""Consignaciones"" SET ""FechaModificacion"" = ""FechaModificacion"" + INTERVAL '3 hours';");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaVencimiento",
                table: "Lotes",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CantidadDisponible",
                table: "Lotes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

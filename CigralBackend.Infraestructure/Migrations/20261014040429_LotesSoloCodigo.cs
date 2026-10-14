using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigralBackend.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class LotesSoloCodigo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. El vencimiento pasa a cada detalle de remito (antes se leía del lote).
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimiento",
                table: "DetallesRemito",
                type: "timestamp without time zone",
                nullable: true);

            // 2. Se copian los vencimientos del lote antes de borrar sus columnas.
            migrationBuilder.Sql(@"
                UPDATE ""DetallesRemito"" d SET ""FechaVencimiento"" = l.""FechaVencimiento""
                FROM ""Lotes"" l WHERE d.""LoteId"" = l.""Id"" AND d.""FechaVencimiento"" IS NULL;
                UPDATE ""Existencias"" e SET ""FechaVencimiento"" = l.""FechaVencimiento""
                FROM ""Lotes"" l WHERE e.""LoteId"" = l.""Id"" AND e.""FechaVencimiento"" IS NULL;
            ");

            // 3. El lote queda solo con el código: sin producto, vencimiento ni baja lógica.
            migrationBuilder.DropForeignKey(
                name: "FK_Lotes_Productos_ProductoId",
                table: "Lotes");

            migrationBuilder.DropIndex(
                name: "IX_Lotes_ProductoId",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "FechaVencimiento",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "Lotes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Lotes",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimiento",
                table: "Lotes",
                type: "timestamp without time zone",
                nullable: true);

            // Reversión aproximada: el producto y el vencimiento se reconstruyen desde las
            // existencias y los detalles de remito que usan cada lote (ProductoId queda nullable).
            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "Lotes",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""Lotes"" l SET
                    ""ProductoId"" = COALESCE(
                        (SELECT min(e.""ProductoId"") FROM ""Existencias"" e WHERE e.""LoteId"" = l.""Id""),
                        (SELECT min(d.""ProductoId"") FROM ""DetallesRemito"" d WHERE d.""LoteId"" = l.""Id"")),
                    ""FechaVencimiento"" = COALESCE(
                        (SELECT min(e.""FechaVencimiento"") FROM ""Existencias"" e WHERE e.""LoteId"" = l.""Id""),
                        (SELECT min(d.""FechaVencimiento"") FROM ""DetallesRemito"" d WHERE d.""LoteId"" = l.""Id""));
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_ProductoId",
                table: "Lotes",
                column: "ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lotes_Productos_ProductoId",
                table: "Lotes",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "FechaVencimiento",
                table: "DetallesRemito");
        }
    }
}

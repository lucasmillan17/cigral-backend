using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigralBackend.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionDePropiedadActivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Proveedores",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Marcas",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Lotes",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Depositos",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_RemitosIngreso_NumeroRemito",
                table: "RemitosIngreso",
                column: "NumeroRemito",
                unique: true,
                filter: "[NumeroRemito] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RemitosEgreso_NumeroRemito",
                table: "RemitosEgreso",
                column: "NumeroRemito",
                unique: true,
                filter: "[NumeroRemito] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_CodigoLote",
                table: "Lotes",
                column: "CodigoLote",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RemitosIngreso_NumeroRemito",
                table: "RemitosIngreso");

            migrationBuilder.DropIndex(
                name: "IX_RemitosEgreso_NumeroRemito",
                table: "RemitosEgreso");

            migrationBuilder.DropIndex(
                name: "IX_Lotes_CodigoLote",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Marcas");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Clientes");

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Depositos",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigralBackend.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class CambioGTINANoRequerido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_GTIN",
                table: "Productos");

            migrationBuilder.AlterColumn<string>(
                name: "GTIN",
                table: "Productos",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(14)",
                oldMaxLength: 14);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_GTIN",
                table: "Productos",
                column: "GTIN",
                unique: true,
                filter: "[GTIN] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_GTIN",
                table: "Productos");

            migrationBuilder.AlterColumn<string>(
                name: "GTIN",
                table: "Productos",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(14)",
                oldMaxLength: 14,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_GTIN",
                table: "Productos",
                column: "GTIN",
                unique: true);
        }
    }
}

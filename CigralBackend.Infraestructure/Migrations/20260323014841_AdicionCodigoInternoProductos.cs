using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigralBackend.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionCodigoInternoProductos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoInterno",
                table: "Productos",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CodigoInterno",
                table: "Productos",
                column: "CodigoInterno",
                unique: true,
                filter: "[CodigoInterno] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_CodigoInterno",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "CodigoInterno",
                table: "Productos");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigralBackend.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class GTINComoIndice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Productos_GTIN",
                table: "Productos",
                column: "GTIN",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_GTIN",
                table: "Productos");
        }
    }
}

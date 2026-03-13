using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigralBackend.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionCodigoGenerico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoGenerico",
                table: "Productos",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CodigoGenerico",
                table: "Productos",
                column: "CodigoGenerico",
                unique: true,
                filter: "[CodigoGenerico] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_CodigoGenerico",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "CodigoGenerico",
                table: "Productos");
        }
    }
}

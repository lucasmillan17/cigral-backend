using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigralBackend.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class eliminacionUniqueNroRemito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RemitosIngreso_NumeroRemito",
                table: "RemitosIngreso");

            migrationBuilder.DropIndex(
                name: "IX_RemitosEgreso_NumeroRemito",
                table: "RemitosEgreso");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}

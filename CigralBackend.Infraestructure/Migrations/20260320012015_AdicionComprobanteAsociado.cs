using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigralBackend.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionComprobanteAsociado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ComprobanteAsociado",
                table: "RemitosIngreso",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComprobanteAsociado",
                table: "RemitosEgreso",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComprobanteAsociado",
                table: "RemitosIngreso");

            migrationBuilder.DropColumn(
                name: "ComprobanteAsociado",
                table: "RemitosEgreso");
        }
    }
}

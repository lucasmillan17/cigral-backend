using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigralBackend.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class FKNullablesRemitos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RemitoIngresoId",
                table: "DetallesRemito",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "RemitoEgresoId",
                table: "DetallesRemito",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_RemitosIngreso_DepositoId",
                table: "RemitosIngreso",
                column: "DepositoId");

            migrationBuilder.CreateIndex(
                name: "IX_RemitosEgreso_DepositoId",
                table: "RemitosEgreso",
                column: "DepositoId");

            migrationBuilder.AddForeignKey(
                name: "FK_RemitosEgreso_Depositos_DepositoId",
                table: "RemitosEgreso",
                column: "DepositoId",
                principalTable: "Depositos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RemitosIngreso_Depositos_DepositoId",
                table: "RemitosIngreso",
                column: "DepositoId",
                principalTable: "Depositos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RemitosEgreso_Depositos_DepositoId",
                table: "RemitosEgreso");

            migrationBuilder.DropForeignKey(
                name: "FK_RemitosIngreso_Depositos_DepositoId",
                table: "RemitosIngreso");

            migrationBuilder.DropIndex(
                name: "IX_RemitosIngreso_DepositoId",
                table: "RemitosIngreso");

            migrationBuilder.DropIndex(
                name: "IX_RemitosEgreso_DepositoId",
                table: "RemitosEgreso");

            migrationBuilder.AlterColumn<int>(
                name: "RemitoIngresoId",
                table: "DetallesRemito",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RemitoEgresoId",
                table: "DetallesRemito",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}

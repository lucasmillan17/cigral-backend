using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigralBackend.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class CreacionVistaEntidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE VIEW vw_Entidades_Resumen AS
            SELECT 
                Id AS IdOriginal, 
                'Cliente' AS TipoEntidad, 
                RazonSocial,
                GLN,
                Email, 
                Cuit, 
                Telefono,
                Direccion,
                Activo
            FROM Clientes
            UNION ALL
            SELECT 
                Id AS IdOriginal, 
                'Proveedor' AS TipoEntidad, 
                RazonSocial,
                GLN,
                Email, 
                Cuit, 
                Telefono,
                Direccion,
                Activo
            FROM Proveedores
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW vw_Entidades_Resumen");
        }
    }
}

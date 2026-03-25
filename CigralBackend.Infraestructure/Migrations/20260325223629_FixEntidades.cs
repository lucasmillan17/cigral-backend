using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigralBackend.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class FixEntidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            ALTER VIEW vw_Entidades_Resumen AS
            SELECT 
                Id, /* <-- Agregamos el Id nativo que espera EntityBase */
                Id AS IdOriginal, 
                'Cliente' AS TipoEntidad, 
                RazonSocial, 
                Cuit, 
                Direccion,
                Email,
                Telefono,
                GLN,
                Activo
            FROM Clientes
            UNION ALL
            SELECT 
                Id, 
                Id AS IdOriginal, 
                'Proveedor' AS TipoEntidad, 
                RazonSocial, 
                Cuit, 
                Direccion,
                Email,
                Telefono,
                GLN,
                Activo
            FROM Proveedores
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CigralBackend.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class TicketsSoporte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "citext", maxLength: 4000, nullable: true),
                    CodigoCrudo = table.Column<string>(type: "citext", maxLength: 2000, nullable: true),
                    ResultadoParserJson = table.Column<string>(type: "citext", nullable: true),
                    Pantalla = table.Column<string>(type: "citext", maxLength: 50, nullable: true),
                    DepositoId = table.Column<int>(type: "integer", nullable: true),
                    ProductoId = table.Column<int>(type: "integer", nullable: true),
                    LoteEtiqueta = table.Column<string>(type: "citext", maxLength: 100, nullable: true),
                    SerieEtiqueta = table.Column<string>(type: "citext", maxLength: 100, nullable: true),
                    VencimientoEtiqueta = table.Column<string>(type: "citext", maxLength: 30, nullable: true),
                    CreadoPorId = table.Column<int>(type: "integer", nullable: false),
                    CreadoPorNombre = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    VersionApp = table.Column<string>(type: "citext", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketAdjuntos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TicketId = table.Column<int>(type: "integer", nullable: false),
                    NombreArchivo = table.Column<string>(type: "citext", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    Datos = table.Column<byte[]>(type: "bytea", nullable: false),
                    Tamano = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketAdjuntos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketAdjuntos_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketComentarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TicketId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioNombre = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Texto = table.Column<string>(type: "citext", maxLength: 4000, nullable: false),
                    EsDeSoporte = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketComentarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketComentarios_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketAdjuntos_TicketId",
                table: "TicketAdjuntos",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComentarios_TicketId",
                table: "TicketComentarios",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CreadoPorId",
                table: "Tickets",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Estado",
                table: "Tickets",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_FechaActualizacion",
                table: "Tickets",
                column: "FechaActualizacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketAdjuntos");

            migrationBuilder.DropTable(
                name: "TicketComentarios");

            migrationBuilder.DropTable(
                name: "Tickets");
        }
    }
}

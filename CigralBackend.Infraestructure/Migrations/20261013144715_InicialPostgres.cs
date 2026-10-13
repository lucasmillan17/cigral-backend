using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CigralBackend.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class InicialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "citext", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "citext", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "citext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreCompleto = table.Column<string>(type: "citext", nullable: true),
                    EsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UltimoLogin = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UserName = table.Column<string>(type: "citext", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "citext", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "citext", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "citext", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "citext", nullable: true),
                    SecurityStamp = table.Column<string>(type: "citext", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "citext", nullable: true),
                    PhoneNumber = table.Column<string>(type: "citext", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Auditorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Entidad = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    EntidadId = table.Column<string>(type: "citext", maxLength: 50, nullable: false),
                    Accion = table.Column<string>(type: "citext", maxLength: 50, nullable: false),
                    Campo = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    ValorAnterior = table.Column<string>(type: "citext", nullable: true),
                    ValorActual = table.Column<string>(type: "citext", nullable: false),
                    Usuario = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RazonSocial = table.Column<string>(type: "citext", maxLength: 200, nullable: true),
                    GLN = table.Column<string>(type: "citext", maxLength: 13, nullable: true),
                    Email = table.Column<string>(type: "citext", maxLength: 100, nullable: true),
                    Cuit = table.Column<string>(type: "citext", maxLength: 11, nullable: true),
                    Telefono = table.Column<string>(type: "citext", maxLength: 20, nullable: true),
                    Direccion = table.Column<string>(type: "citext", maxLength: 200, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Depositos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    Codigo = table.Column<string>(type: "citext", maxLength: 20, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Depositos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Marcas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marcas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RazonSocial = table.Column<string>(type: "citext", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "citext", maxLength: 100, nullable: true),
                    Cuit = table.Column<string>(type: "citext", maxLength: 11, nullable: true),
                    Telefono = table.Column<string>(type: "citext", maxLength: 20, nullable: true),
                    Direccion = table.Column<string>(type: "citext", maxLength: 200, nullable: true),
                    GLN = table.Column<string>(type: "citext", maxLength: 13, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "citext", nullable: true),
                    ClaimValue = table.Column<string>(type: "citext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "citext", nullable: true),
                    ClaimValue = table.Column<string>(type: "citext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "citext", nullable: false),
                    ProviderKey = table.Column<string>(type: "citext", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "citext", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LoginProvider = table.Column<string>(type: "citext", nullable: false),
                    Name = table.Column<string>(type: "citext", nullable: false),
                    Value = table.Column<string>(type: "citext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RemitosEgreso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Observaciones = table.Column<string>(type: "citext", maxLength: 500, nullable: true),
                    NumeroRemito = table.Column<string>(type: "citext", maxLength: 50, nullable: true),
                    DepositoId = table.Column<int>(type: "integer", nullable: false),
                    ComprobanteAsociado = table.Column<string>(type: "citext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemitosEgreso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RemitosEgreso_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RemitosEgreso_Depositos_DepositoId",
                        column: x => x.DepositoId,
                        principalTable: "Depositos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "citext", maxLength: 500, nullable: false),
                    GTIN = table.Column<string>(type: "citext", maxLength: 14, nullable: true),
                    MarcaId = table.Column<int>(type: "integer", nullable: true),
                    Precio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    EsUnitario = table.Column<bool>(type: "boolean", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CodigoGenerico = table.Column<string>(type: "citext", nullable: true),
                    CodigoInterno = table.Column<string>(type: "citext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_Marcas_MarcaId",
                        column: x => x.MarcaId,
                        principalTable: "Marcas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RemitosIngreso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProveedorId = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Observaciones = table.Column<string>(type: "citext", maxLength: 500, nullable: true),
                    NumeroRemito = table.Column<string>(type: "citext", maxLength: 50, nullable: true),
                    DepositoId = table.Column<int>(type: "integer", nullable: false),
                    ComprobanteAsociado = table.Column<string>(type: "citext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemitosIngreso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RemitosIngreso_Depositos_DepositoId",
                        column: x => x.DepositoId,
                        principalTable: "Depositos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RemitosIngreso_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CodigoLote = table.Column<string>(type: "citext", maxLength: 50, nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CantidadDisponible = table.Column<int>(type: "integer", nullable: false),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lotes_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesRemito",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RemitoIngresoId = table.Column<int>(type: "integer", nullable: true),
                    RemitoEgresoId = table.Column<int>(type: "integer", nullable: true),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    LoteId = table.Column<int>(type: "integer", nullable: true),
                    NumeroSerie = table.Column<string>(type: "citext", maxLength: 100, nullable: true),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    InformacionAdicional = table.Column<string>(type: "citext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesRemito", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesRemito_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesRemito_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesRemito_RemitosEgreso_RemitoEgresoId",
                        column: x => x.RemitoEgresoId,
                        principalTable: "RemitosEgreso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesRemito_RemitosIngreso_RemitoIngresoId",
                        column: x => x.RemitoIngresoId,
                        principalTable: "RemitosIngreso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Existencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DepositoId = table.Column<int>(type: "integer", nullable: false),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    NumSerie = table.Column<string>(type: "citext", maxLength: 100, nullable: true),
                    LoteId = table.Column<int>(type: "integer", nullable: true),
                    FechaVencimiento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    InformacionAdicional = table.Column<string>(type: "citext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Existencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Existencias_Depositos_DepositoId",
                        column: x => x.DepositoId,
                        principalTable: "Depositos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Existencias_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Existencias_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosStock",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    FechaMovimiento = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    DepositoId = table.Column<int>(type: "integer", nullable: false),
                    LoteId = table.Column<int>(type: "integer", nullable: true),
                    NumeroSerie = table.Column<string>(type: "citext", maxLength: 100, nullable: true),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    StockAnterior = table.Column<int>(type: "integer", nullable: false),
                    StockNuevo = table.Column<int>(type: "integer", nullable: false),
                    RemitoIngresoId = table.Column<int>(type: "integer", nullable: true),
                    RemitoEgresoId = table.Column<int>(type: "integer", nullable: true),
                    Usuario = table.Column<string>(type: "citext", maxLength: 100, nullable: true),
                    Observaciones = table.Column<string>(type: "citext", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosStock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosStock_Depositos_DepositoId",
                        column: x => x.DepositoId,
                        principalTable: "Depositos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosStock_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosStock_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosStock_RemitosEgreso_RemitoEgresoId",
                        column: x => x.RemitoEgresoId,
                        principalTable: "RemitosEgreso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosStock_RemitosIngreso_RemitoIngresoId",
                        column: x => x.RemitoIngresoId,
                        principalTable: "RemitosIngreso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Consignaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExistenciaId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consignaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consignaciones_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Consignaciones_Existencias_ExistenciaId",
                        column: x => x.ExistenciaId,
                        principalTable: "Existencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_Entidad",
                table: "Auditorias",
                column: "Entidad");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_Fecha",
                table: "Auditorias",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Consignaciones_ClienteId_ExistenciaId",
                table: "Consignaciones",
                columns: new[] { "ClienteId", "ExistenciaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consignaciones_ExistenciaId",
                table: "Consignaciones",
                column: "ExistenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesRemito_LoteId",
                table: "DetallesRemito",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesRemito_ProductoId",
                table: "DetallesRemito",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesRemito_RemitoEgresoId",
                table: "DetallesRemito",
                column: "RemitoEgresoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesRemito_RemitoIngresoId",
                table: "DetallesRemito",
                column: "RemitoIngresoId");

            migrationBuilder.CreateIndex(
                name: "IX_Existencias_DepositoId",
                table: "Existencias",
                column: "DepositoId");

            migrationBuilder.CreateIndex(
                name: "IX_Existencias_LoteId",
                table: "Existencias",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Existencias_ProductoId",
                table: "Existencias",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_CodigoLote",
                table: "Lotes",
                column: "CodigoLote",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_ProductoId",
                table: "Lotes",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_DepositoId",
                table: "MovimientosStock",
                column: "DepositoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_FechaMovimiento",
                table: "MovimientosStock",
                column: "FechaMovimiento");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_LoteId",
                table: "MovimientosStock",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_ProductoId_DepositoId",
                table: "MovimientosStock",
                columns: new[] { "ProductoId", "DepositoId" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_RemitoEgresoId",
                table: "MovimientosStock",
                column: "RemitoEgresoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_RemitoIngresoId",
                table: "MovimientosStock",
                column: "RemitoIngresoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_Tipo",
                table: "MovimientosStock",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CodigoGenerico",
                table: "Productos",
                column: "CodigoGenerico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CodigoInterno",
                table: "Productos",
                column: "CodigoInterno",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_GTIN",
                table: "Productos",
                column: "GTIN",
                unique: true,
                filter: "\"GTIN\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_MarcaId",
                table: "Productos",
                column: "MarcaId");

            migrationBuilder.CreateIndex(
                name: "IX_RemitosEgreso_ClienteId",
                table: "RemitosEgreso",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_RemitosEgreso_DepositoId",
                table: "RemitosEgreso",
                column: "DepositoId");

            migrationBuilder.CreateIndex(
                name: "IX_RemitosIngreso_DepositoId",
                table: "RemitosIngreso",
                column: "DepositoId");

            migrationBuilder.CreateIndex(
                name: "IX_RemitosIngreso_ProveedorId",
                table: "RemitosIngreso",
                column: "ProveedorId");

            // Vista usada por EntidadResumen (antes: migraciones CreacionVistaEntidades / FixEntidades de SQL Server)
            migrationBuilder.Sql(@"
                CREATE VIEW ""vw_Entidades_Resumen"" AS
                SELECT ""Id"", ""Id"" AS ""IdOriginal"", CAST('Cliente' AS citext) AS ""TipoEntidad"",
                       ""RazonSocial"", ""Cuit"", ""Direccion"", ""Email"", ""Telefono"", ""GLN"", ""Activo""
                FROM ""Clientes""
                UNION ALL
                SELECT ""Id"", ""Id"" AS ""IdOriginal"", CAST('Proveedor' AS citext) AS ""TipoEntidad"",
                       ""RazonSocial"", ""Cuit"", ""Direccion"", ""Email"", ""Telefono"", ""GLN"", ""Activo""
                FROM ""Proveedores"";
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS ""vw_Entidades_Resumen"";");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Auditorias");

            migrationBuilder.DropTable(
                name: "Consignaciones");

            migrationBuilder.DropTable(
                name: "DetallesRemito");

            migrationBuilder.DropTable(
                name: "MovimientosStock");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Existencias");

            migrationBuilder.DropTable(
                name: "RemitosEgreso");

            migrationBuilder.DropTable(
                name: "RemitosIngreso");

            migrationBuilder.DropTable(
                name: "Lotes");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Depositos");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Marcas");
        }
    }
}

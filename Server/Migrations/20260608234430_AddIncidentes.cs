using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSTDigitalRD.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Charlas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tema = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Instructor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Obra = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Cuadrilla = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaCharla = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DuracionMinutos = table.Column<int>(type: "int", nullable: false),
                    TotalAsistentes = table.Column<int>(type: "int", nullable: false),
                    Latitud = table.Column<double>(type: "float", nullable: false),
                    Longitud = table.Column<double>(type: "float", nullable: false),
                    PrecisionGps = table.Column<double>(type: "float", nullable: false),
                    GpsCapturado = table.Column<bool>(type: "bit", nullable: false),
                    FotoCapturada = table.Column<bool>(type: "bit", nullable: false),
                    FotoBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConteoFacial = table.Column<int>(type: "int", nullable: false),
                    Firmado = table.Column<bool>(type: "bit", nullable: false),
                    FirmaBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HoraFirma = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HashSha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Charlas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntregasEPP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreTrabajador = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CedulaTrabajador = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Cargo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cuadrilla = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Obra = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntregadoPor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Firmado = table.Column<bool>(type: "bit", nullable: false),
                    FirmaBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HashSha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntregasEPP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Incidentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Obra = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Afectado = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Inspector = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AtencionMedica = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Testigos = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaIncidente = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiasPerdidos = table.Column<int>(type: "int", nullable: false),
                    Latitud = table.Column<double>(type: "float", nullable: false),
                    Longitud = table.Column<double>(type: "float", nullable: false),
                    PrecisionGps = table.Column<double>(type: "float", nullable: false),
                    GpsCapturado = table.Column<bool>(type: "bit", nullable: false),
                    CantidadFotos = table.Column<int>(type: "int", nullable: false),
                    NotificarMTRAB = table.Column<bool>(type: "bit", nullable: false),
                    NotificadoMTRAB = table.Column<bool>(type: "bit", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Firmado = table.Column<bool>(type: "bit", nullable: false),
                    FirmaBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HashSha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidentes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AsistentesCharla",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CharlaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Cargo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Presente = table.Column<bool>(type: "bit", nullable: false),
                    FirmaBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsistentesCharla", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsistentesCharla_Charlas_CharlaId",
                        column: x => x.CharlaId,
                        principalTable: "Charlas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticulosEPP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntregaEPPId = table.Column<int>(type: "int", nullable: false),
                    TipoEPP = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticulosEPP", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticulosEPP_EntregasEPP_EntregaEPPId",
                        column: x => x.EntregaEPPId,
                        principalTable: "EntregasEPP",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccionesCorrectivas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidenteId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Responsable = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaLimite = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccionesCorrectivas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccionesCorrectivas_Incidentes_IncidenteId",
                        column: x => x.IncidenteId,
                        principalTable: "Incidentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccionesCorrectivas_Estado",
                table: "AccionesCorrectivas",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_AccionesCorrectivas_IncidenteId",
                table: "AccionesCorrectivas",
                column: "IncidenteId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticulosEPP_EntregaEPPId",
                table: "ArticulosEPP",
                column: "EntregaEPPId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticulosEPP_Estado",
                table: "ArticulosEPP",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_ArticulosEPP_FechaVencimiento",
                table: "ArticulosEPP",
                column: "FechaVencimiento");

            migrationBuilder.CreateIndex(
                name: "IX_AsistentesCharla_CharlaId",
                table: "AsistentesCharla",
                column: "CharlaId");

            migrationBuilder.CreateIndex(
                name: "IX_Charlas_FechaCharla",
                table: "Charlas",
                column: "FechaCharla");

            migrationBuilder.CreateIndex(
                name: "IX_Charlas_Instructor",
                table: "Charlas",
                column: "Instructor");

            migrationBuilder.CreateIndex(
                name: "IX_Charlas_Obra",
                table: "Charlas",
                column: "Obra");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasEPP_CedulaTrabajador",
                table: "EntregasEPP",
                column: "CedulaTrabajador");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasEPP_FechaEntrega",
                table: "EntregasEPP",
                column: "FechaEntrega");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasEPP_Obra",
                table: "EntregasEPP",
                column: "Obra");

            migrationBuilder.CreateIndex(
                name: "IX_Incidentes_Estado",
                table: "Incidentes",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Incidentes_FechaIncidente",
                table: "Incidentes",
                column: "FechaIncidente");

            migrationBuilder.CreateIndex(
                name: "IX_Incidentes_Obra",
                table: "Incidentes",
                column: "Obra");

            migrationBuilder.CreateIndex(
                name: "IX_Incidentes_Tipo",
                table: "Incidentes",
                column: "Tipo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccionesCorrectivas");

            migrationBuilder.DropTable(
                name: "ArticulosEPP");

            migrationBuilder.DropTable(
                name: "AsistentesCharla");

            migrationBuilder.DropTable(
                name: "Incidentes");

            migrationBuilder.DropTable(
                name: "EntregasEPP");

            migrationBuilder.DropTable(
                name: "Charlas");
        }
    }
}

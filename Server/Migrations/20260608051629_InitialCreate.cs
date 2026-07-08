using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSTDigitalRD.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Inspecciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Area = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Obra = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TipoInspeccion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Inspector = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ResponsableArea = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaInspeccion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CantidadTrabajadores = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Latitud = table.Column<double>(type: "float", nullable: false),
                    Longitud = table.Column<double>(type: "float", nullable: false),
                    PrecisionGps = table.Column<double>(type: "float", nullable: false),
                    GpsCapturado = table.Column<bool>(type: "bit", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Firmado = table.Column<bool>(type: "bit", nullable: false),
                    FirmaBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HoraFirma = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HashSha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CantidadFotos = table.Column<int>(type: "int", nullable: false),
                    PlanAccion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspecciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspeccionId = table.Column<int>(type: "int", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Resultado = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistItems_Inspecciones_InspeccionId",
                        column: x => x.InspeccionId,
                        principalTable: "Inspecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItems_InspeccionId",
                table: "ChecklistItems",
                column: "InspeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspecciones_Estado",
                table: "Inspecciones",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Inspecciones_FechaInspeccion",
                table: "Inspecciones",
                column: "FechaInspeccion");

            migrationBuilder.CreateIndex(
                name: "IX_Inspecciones_Inspector",
                table: "Inspecciones",
                column: "Inspector");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChecklistItems");

            migrationBuilder.DropTable(
                name: "Inspecciones");
        }
    }
}

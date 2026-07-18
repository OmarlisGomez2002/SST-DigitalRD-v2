using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSTDigitalRD.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddZonasObra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ZonasObra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObraId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZonasObra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZonasObra_ObrasActivas_ObraId",
                        column: x => x.ObraId,
                        principalTable: "ObrasActivas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ZonasObra_Activa",
                table: "ZonasObra",
                column: "Activa");

            migrationBuilder.CreateIndex(
                name: "IX_ZonasObra_ObraId",
                table: "ZonasObra",
                column: "ObraId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ZonasObra");
        }
    }
}

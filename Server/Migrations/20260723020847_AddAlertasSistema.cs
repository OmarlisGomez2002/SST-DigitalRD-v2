using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSTDigitalRD.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertasSistema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertasSistema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nivel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Zona = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NivelRiesgo = table.Column<int>(type: "int", nullable: false),
                    Leida = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertasSistema", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertasSistema_FechaCreacion",
                table: "AlertasSistema",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasSistema_Leida",
                table: "AlertasSistema",
                column: "Leida");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasSistema_Tipo",
                table: "AlertasSistema",
                column: "Tipo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertasSistema");
        }
    }
}

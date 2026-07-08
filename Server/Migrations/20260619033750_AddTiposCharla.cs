using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSTDigitalRD.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddTiposCharla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TiposCharla",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposCharla", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TiposCharla_Nombre",
                table: "TiposCharla",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TiposCharla");
        }
    }
}

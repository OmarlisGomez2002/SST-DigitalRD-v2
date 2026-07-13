using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSTDigitalRD.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramaSST : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgramaSST",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Vigencia = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Politica = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MatrizRiesgos = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PlanEmergencia = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramaSST", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgramaSST");
        }
    }
}

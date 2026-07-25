using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSTDigitalRD.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddPoliticaPrivacidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AceptoPolitica",
                table: "UsuariosSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAceptoPolitica",
                table: "UsuariosSistema",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AceptoPolitica",
                table: "UsuariosSistema");

            migrationBuilder.DropColumn(
                name: "FechaAceptoPolitica",
                table: "UsuariosSistema");
        }
    }
}

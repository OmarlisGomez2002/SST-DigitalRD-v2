using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSTDigitalRD.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthToUsuarioSistema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "PasswordHash",
                table: "UsuariosSistema",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "PasswordSalt",
                table: "UsuariosSistema",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoAcceso",
                table: "UsuariosSistema",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "UsuariosSistema");

            migrationBuilder.DropColumn(
                name: "PasswordSalt",
                table: "UsuariosSistema");

            migrationBuilder.DropColumn(
                name: "UltimoAcceso",
                table: "UsuariosSistema");
        }
    }
}

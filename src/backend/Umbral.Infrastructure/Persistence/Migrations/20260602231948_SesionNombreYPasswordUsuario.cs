using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbral.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SesionNombreYPasswordUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "password_asignada",
                table: "usuarios_administrables",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nombre",
                table: "sesiones",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE sesiones
                SET nombre = 'Sesión ' || LEFT(REPLACE(id::text, '-', ''), 8)
                WHERE nombre IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "nombre",
                table: "sesiones",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "nombre",
                table: "sesiones");

            migrationBuilder.DropColumn(
                name: "password_asignada",
                table: "usuarios_administrables");
        }
    }
}

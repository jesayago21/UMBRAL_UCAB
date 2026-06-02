using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbral.Infrastructure.Persistence.Migrations;

public partial class AddNombreSesion : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "nombre",
            table: "sesiones");
    }
}

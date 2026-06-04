using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbral.Infrastructure.Persistence.Migrations;

public partial class CodigoAccesoSesionYJugadorEquipo : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "codigo_acceso",
            table: "sesiones",
            type: "character varying(32)",
            maxLength: 32,
            nullable: true);

        migrationBuilder.Sql(
            """
            UPDATE sesiones
            SET codigo_acceso = UPPER(SUBSTRING(REPLACE(gen_random_uuid()::text, '-', '') FROM 1 FOR 8))
            WHERE codigo_acceso IS NULL;
            """);

        migrationBuilder.AlterColumn<string>(
            name: "codigo_acceso",
            table: "sesiones",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(32)",
            oldMaxLength: 32,
            oldNullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "jugador_id",
            table: "equipos_sesion",
            type: "uuid",
            nullable: true);

        migrationBuilder.Sql(
            """
            UPDATE equipos_sesion
            SET jugador_id = gen_random_uuid()
            WHERE jugador_id IS NULL;
            """);

        migrationBuilder.DropColumn(
            name: "codigo_acceso",
            table: "equipos_sesion");

        migrationBuilder.AlterColumn<Guid>(
            name: "jugador_id",
            table: "equipos_sesion",
            type: "uuid",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_equipos_sesion_sesion_id_jugador_id",
            table: "equipos_sesion",
            columns: new[] { "sesion_id", "jugador_id" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_equipos_sesion_sesion_id_jugador_id",
            table: "equipos_sesion");

        migrationBuilder.DropColumn(
            name: "codigo_acceso",
            table: "sesiones");

        migrationBuilder.DropColumn(
            name: "jugador_id",
            table: "equipos_sesion");

        migrationBuilder.AddColumn<string>(
            name: "codigo_acceso",
            table: "equipos_sesion",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "LEGACY00");
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbral.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
/// <summary>
/// La migración polimórfica añadió <c>tipo_etapa</c> con default vacío; las filas existentes
/// deben marcarse como BusquedaTesoro o Trivia para que EF TPH pueda materializarlas.
/// </summary>
public partial class BackfillEtapasDiscriminator : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE etapas
            SET tipo_etapa = 'Trivia'
            WHERE (tipo_etapa IS NULL OR tipo_etapa = '')
              AND categoria_ids_json IS NOT NULL;

            UPDATE etapas
            SET tipo_etapa = 'BusquedaTesoro'
            WHERE tipo_etapa IS NULL OR tipo_etapa = '';
            """);

        migrationBuilder.AlterColumn<string>(
            name: "tipo_etapa",
            table: "etapas",
            type: "character varying(21)",
            maxLength: 21,
            nullable: false,
            defaultValue: "BusquedaTesoro",
            oldClrType: typeof(string),
            oldType: "character varying(21)",
            oldMaxLength: 21,
            oldDefaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "tipo_etapa",
            table: "etapas",
            type: "character varying(21)",
            maxLength: 21,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "character varying(21)",
            oldMaxLength: 21,
            oldDefaultValue: "BusquedaTesoro");
    }
}

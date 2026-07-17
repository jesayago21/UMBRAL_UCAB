using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbral.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPistasPorTiempoLiberacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "etapa_iniciada_en",
                table: "contextos_mision",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "pausada_desde",
                table: "contextos_mision",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pistas_entregadas_json",
                table: "contextos_mision",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "segundos_pausa_acumulados",
                table: "contextos_mision",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "etapa_iniciada_en",
                table: "contextos_mision");

            migrationBuilder.DropColumn(
                name: "pausada_desde",
                table: "contextos_mision");

            migrationBuilder.DropColumn(
                name: "pistas_entregadas_json",
                table: "contextos_mision");

            migrationBuilder.DropColumn(
                name: "segundos_pausa_acumulados",
                table: "contextos_mision");
        }
    }
}

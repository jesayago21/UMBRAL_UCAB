using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbral.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTriviaTimerContextoMision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "timer_cerrado_en",
                table: "contextos_mision",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "trivia_en_transicion",
                table: "contextos_mision",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "timer_cerrado_en",
                table: "contextos_mision");

            migrationBuilder.DropColumn(
                name: "trivia_en_transicion",
                table: "contextos_mision");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbral.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeudaPendienteParticipante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // El índice simple queda cubierto por el único compuesto
            // (sesion_id, participante_id, pregunta_id); alinear modelo EF.
            migrationBuilder.DropIndex(
                name: "IX_respuestas_trivia_sesion_id",
                table: "respuestas_trivia");

            migrationBuilder.AddColumn<int>(
                name: "deuda_pendiente",
                table: "participantes_sesion",
                type: "integer",
                nullable: false,
                defaultValueSql: "0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deuda_pendiente",
                table: "participantes_sesion");

            migrationBuilder.CreateIndex(
                name: "IX_respuestas_trivia_sesion_id",
                table: "respuestas_trivia",
                column: "sesion_id");
        }
    }
}

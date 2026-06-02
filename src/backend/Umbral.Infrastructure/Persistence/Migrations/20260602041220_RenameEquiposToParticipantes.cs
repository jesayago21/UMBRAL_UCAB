using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbral.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameEquiposToParticipantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "equipos_sesion",
                newName: "participantes_sesion");

            migrationBuilder.RenameIndex(
                name: "IX_equipos_sesion_sesion_id_nombre",
                table: "participantes_sesion",
                newName: "IX_participantes_sesion_sesion_id_nombre");

            migrationBuilder.RenameIndex(
                name: "IX_equipos_sesion_sesion_id_jugador_id",
                table: "participantes_sesion",
                newName: "IX_participantes_sesion_sesion_id_jugador_id");

            migrationBuilder.RenameColumn(
                name: "equipo_id",
                table: "evidencias",
                newName: "participante_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "participante_id",
                table: "evidencias",
                newName: "equipo_id");

            migrationBuilder.RenameIndex(
                name: "IX_participantes_sesion_sesion_id_jugador_id",
                table: "participantes_sesion",
                newName: "IX_equipos_sesion_sesion_id_jugador_id");

            migrationBuilder.RenameIndex(
                name: "IX_participantes_sesion_sesion_id_nombre",
                table: "participantes_sesion",
                newName: "IX_equipos_sesion_sesion_id_nombre");

            migrationBuilder.RenameTable(
                name: "participantes_sesion",
                newName: "equipos_sesion");
        }
    }
}

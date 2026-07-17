using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbral.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRespuestasTrivia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "respuestas_trivia",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sesion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pregunta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    indice_opcion = table.Column<int>(type: "integer", nullable: false),
                    timestamp_servidor = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fuera_de_tiempo = table.Column<bool>(type: "boolean", nullable: false),
                    es_correcta = table.Column<bool>(type: "boolean", nullable: false),
                    puntos_otorgados = table.Column<int>(type: "integer", nullable: false),
                    tiempo_respuesta_ms = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_respuestas_trivia", x => x.id);
                    table.ForeignKey(
                        name: "FK_respuestas_trivia_sesiones_sesion_id",
                        column: x => x.sesion_id,
                        principalTable: "sesiones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_respuestas_trivia_sesion_id",
                table: "respuestas_trivia",
                column: "sesion_id");

            migrationBuilder.CreateIndex(
                name: "IX_respuestas_trivia_sesion_participante_pregunta",
                table: "respuestas_trivia",
                columns: new[] { "sesion_id", "participante_id", "pregunta_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "respuestas_trivia");
        }
    }
}

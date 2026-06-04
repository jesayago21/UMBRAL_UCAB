using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbral.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContextoTrivia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contextos_trivia",
                columns: table => new
                {
                    SesionId = table.Column<Guid>(type: "uuid", nullable: false),
                    pregunta_actual_index = table.Column<int>(type: "integer", nullable: false),
                    timer_cerrado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    preguntas_ordenadas_json = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contextos_trivia", x => x.SesionId);
                    table.ForeignKey(
                        name: "FK_contextos_trivia_sesiones_SesionId",
                        column: x => x.SesionId,
                        principalTable: "sesiones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "contextos_trivia");
        }
    }
}

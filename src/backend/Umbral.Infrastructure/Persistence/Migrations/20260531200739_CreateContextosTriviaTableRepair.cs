using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbral.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateContextosTriviaTableRepair : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS contextos_trivia (
                    "SesionId" uuid NOT NULL,
                    pregunta_actual_index integer NOT NULL,
                    timer_cerrado_en timestamp with time zone NULL,
                    categorias_titulo character varying(500) NOT NULL DEFAULT 'Trivia',
                    preguntas_ordenadas_json jsonb NOT NULL,
                    CONSTRAINT "PK_contextos_trivia" PRIMARY KEY ("SesionId"),
                    CONSTRAINT "FK_contextos_trivia_sesiones_SesionId"
                        FOREIGN KEY ("SesionId") REFERENCES sesiones (id) ON DELETE CASCADE
                );
                ALTER TABLE contextos_trivia
                    ADD COLUMN IF NOT EXISTS categorias_titulo character varying(500) NOT NULL DEFAULT 'Trivia';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS contextos_trivia;");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbral.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SesionNombreYPasswordUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE usuarios_administrables
                    ADD COLUMN IF NOT EXISTS password_asignada character varying(128);

                ALTER TABLE sesiones
                    ADD COLUMN IF NOT EXISTS nombre character varying(120);

                UPDATE sesiones
                SET nombre = 'Sesión ' || LEFT(REPLACE(id::text, '-', ''), 8)
                WHERE nombre IS NULL;

                ALTER TABLE sesiones
                    ALTER COLUMN nombre SET NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE sesiones DROP COLUMN IF EXISTS nombre;
                ALTER TABLE usuarios_administrables DROP COLUMN IF EXISTS password_asignada;
                """);
        }
    }
}

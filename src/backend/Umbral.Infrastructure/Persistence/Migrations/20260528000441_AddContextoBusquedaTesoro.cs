using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbral.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContextoBusquedaTesoro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contextos_bt",
                columns: table => new
                {
                    SesionId = table.Column<Guid>(type: "uuid", nullable: false),
                    mision_snapshot_json = table.Column<string>(type: "jsonb", nullable: false),
                    etapa_actual_index = table.Column<int>(type: "integer", nullable: false),
                    ganador_etapa_actual_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contextos_bt", x => x.SesionId);
                    table.ForeignKey(
                        name: "FK_contextos_bt_sesiones_SesionId",
                        column: x => x.SesionId,
                        principalTable: "sesiones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contextos_bt");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbral.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "misiones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_misiones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sesiones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_sesion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    operador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    iniciada_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    finalizada_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sesiones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "etapas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    mision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    codigo_qr_solucion = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_etapas", x => x.id);
                    table.ForeignKey(
                        name: "FK_etapas_misiones_mision_id",
                        column: x => x.mision_id,
                        principalTable: "misiones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "equipos_sesion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sesion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    codigo_acceso = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    puntaje_total = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipos_sesion", x => x.id);
                    table.ForeignKey(
                        name: "FK_equipos_sesion_sesiones_sesion_id",
                        column: x => x.sesion_id,
                        principalTable: "sesiones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "eventos_sesion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sesion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload_json = table.Column<string>(type: "text", nullable: false),
                    ocurrido_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventos_sesion", x => x.id);
                    table.ForeignKey(
                        name: "FK_eventos_sesion_sesiones_sesion_id",
                        column: x => x.sesion_id,
                        principalTable: "sesiones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "evidencias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sesion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    etapa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_qr_leido = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    timestamp_servidor = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    resultado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evidencias", x => x.id);
                    table.ForeignKey(
                        name: "FK_evidencias_sesiones_sesion_id",
                        column: x => x.sesion_id,
                        principalTable: "sesiones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pistas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    etapa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contenido = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    tipo_liberacion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    segundos_liberacion = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pistas", x => x.id);
                    table.ForeignKey(
                        name: "FK_pistas_etapas_etapa_id",
                        column: x => x.etapa_id,
                        principalTable: "etapas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_equipos_sesion_sesion_id_nombre",
                table: "equipos_sesion",
                columns: new[] { "sesion_id", "nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_etapas_mision_id_orden",
                table: "etapas",
                columns: new[] { "mision_id", "orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_eventos_sesion_sesion_id_ocurrido_en",
                table: "eventos_sesion",
                columns: new[] { "sesion_id", "ocurrido_en" });

            migrationBuilder.CreateIndex(
                name: "IX_evidencias_sesion_id",
                table: "evidencias",
                column: "sesion_id");

            migrationBuilder.CreateIndex(
                name: "IX_pistas_etapa_id",
                table: "pistas",
                column: "etapa_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "equipos_sesion");

            migrationBuilder.DropTable(
                name: "eventos_sesion");

            migrationBuilder.DropTable(
                name: "evidencias");

            migrationBuilder.DropTable(
                name: "pistas");

            migrationBuilder.DropTable(
                name: "sesiones");

            migrationBuilder.DropTable(
                name: "etapas");

            migrationBuilder.DropTable(
                name: "misiones");
        }
    }
}

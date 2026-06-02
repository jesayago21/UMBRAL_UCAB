using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbral.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PolymorphicMisionAndIdentidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "mision_id",
                table: "sesiones",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "etapas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "codigo_qr_solucion",
                table: "etapas",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.AddColumn<string>(
                name: "categoria_ids_json",
                table: "etapas",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_etapa",
                table: "etapas",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "contextos_mision",
                columns: table => new
                {
                    SesionId = table.Column<Guid>(type: "uuid", nullable: false),
                    mision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mision_snapshot_json = table.Column<string>(type: "jsonb", nullable: false),
                    etapa_actual_index = table.Column<int>(type: "integer", nullable: false),
                    ganador_etapa_actual_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pregunta_trivia_actual_index = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contextos_mision", x => x.SesionId);
                    table.ForeignKey(
                        name: "FK_contextos_mision_sesiones_SesionId",
                        column: x => x.SesionId,
                        principalTable: "sesiones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_administrables",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    keycloak_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    username = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    apellido = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    roles_json = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_administrables", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_administrables_email",
                table: "usuarios_administrables",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_administrables_username",
                table: "usuarios_administrables",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contextos_mision");

            migrationBuilder.DropTable(
                name: "usuarios_administrables");

            migrationBuilder.DropColumn(
                name: "mision_id",
                table: "sesiones");

            migrationBuilder.DropColumn(
                name: "categoria_ids_json",
                table: "etapas");

            migrationBuilder.DropColumn(
                name: "tipo_etapa",
                table: "etapas");

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "etapas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "codigo_qr_solucion",
                table: "etapas",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120,
                oldNullable: true);
        }
    }
}

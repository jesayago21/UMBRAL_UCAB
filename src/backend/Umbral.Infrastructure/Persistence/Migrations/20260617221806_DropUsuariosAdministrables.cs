using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbral.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropUsuariosAdministrables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usuarios_administrables");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usuarios_administrables",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    apellido = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    keycloak_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    password_asignada = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    username = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
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
    }
}

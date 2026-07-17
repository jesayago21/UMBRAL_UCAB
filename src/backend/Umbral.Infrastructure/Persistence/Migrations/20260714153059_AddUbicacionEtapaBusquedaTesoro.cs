using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbral.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUbicacionEtapaBusquedaTesoro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "latitud",
                table: "etapas",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitud",
                table: "etapas",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "radio_metros",
                table: "etapas",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "latitud",
                table: "etapas");

            migrationBuilder.DropColumn(
                name: "longitud",
                table: "etapas");

            migrationBuilder.DropColumn(
                name: "radio_metros",
                table: "etapas");
        }
    }
}

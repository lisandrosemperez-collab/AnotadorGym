using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnotadorGymApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregadoPropiedadesOrdenEnRutinas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumeroSerie",
                table: "RutinaSeries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumeroSemana",
                table: "RutinaSemanas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumeroEjercicio",
                table: "RutinaEjercicios",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumeroDia",
                table: "RutinaDias",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroSerie",
                table: "RutinaSeries");

            migrationBuilder.DropColumn(
                name: "NumeroSemana",
                table: "RutinaSemanas");

            migrationBuilder.DropColumn(
                name: "NumeroEjercicio",
                table: "RutinaEjercicios");

            migrationBuilder.DropColumn(
                name: "NumeroDia",
                table: "RutinaDias");
        }
    }
}

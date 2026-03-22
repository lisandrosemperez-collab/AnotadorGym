using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnotadorGymApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class CambioNombrePropiedadRutinaEjercicioEnRutinaSeries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SemanaIdUI",
                table: "RutinaSemanas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SemanaIdUI",
                table: "RutinaSemanas",
                type: "INTEGER",
                nullable: true);
        }
    }
}

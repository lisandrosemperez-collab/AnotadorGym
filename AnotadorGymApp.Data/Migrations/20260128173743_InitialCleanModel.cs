using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnotadorGymApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCleanModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiasEntrenamientos",
                columns: table => new
                {
                    DiaEntrenamientoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Volumen = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiasEntrenamientos", x => x.DiaEntrenamientoId);
                });

            migrationBuilder.CreateTable(
                name: "GrupoMusculares",
                columns: table => new
                {
                    GrupoMuscularId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoMusculares", x => x.GrupoMuscularId);
                });

            migrationBuilder.CreateTable(
                name: "Musculos",
                columns: table => new
                {
                    MusculoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Musculos", x => x.MusculoId);
                });

            migrationBuilder.CreateTable(
                name: "Rutinas",
                columns: table => new
                {
                    RutinaId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Completado = table.Column<bool>(type: "INTEGER", nullable: false),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false),
                    ImageSource = table.Column<string>(type: "TEXT", nullable: true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true),
                    TiempoPorSesion = table.Column<string>(type: "TEXT", nullable: true),
                    Dificultad = table.Column<string>(type: "TEXT", nullable: true),
                    FrecuenciaPorGrupo = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rutinas", x => x.RutinaId);
                });

            migrationBuilder.CreateTable(
                name: "Ejercicios",
                columns: table => new
                {
                    EjercicioId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MusculoPrimarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    GrupoMuscularId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Mejor = table.Column<double>(type: "REAL", nullable: true),
                    Iniciar = table.Column<double>(type: "REAL", nullable: true),
                    Ultimo = table.Column<double>(type: "REAL", nullable: true),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ejercicios", x => x.EjercicioId);
                    table.ForeignKey(
                        name: "FK_Ejercicios_GrupoMusculares_GrupoMuscularId",
                        column: x => x.GrupoMuscularId,
                        principalTable: "GrupoMusculares",
                        principalColumn: "GrupoMuscularId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ejercicios_Musculos_MusculoPrimarioId",
                        column: x => x.MusculoPrimarioId,
                        principalTable: "Musculos",
                        principalColumn: "MusculoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RutinaSemanas",
                columns: table => new
                {
                    SemanaId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RutinaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Completado = table.Column<bool>(type: "INTEGER", nullable: false),
                    NombreSemana = table.Column<string>(type: "TEXT", nullable: false),
                    SemanaIdUI = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RutinaSemanas", x => x.SemanaId);
                    table.ForeignKey(
                        name: "FK_RutinaSemanas_Rutinas_RutinaId",
                        column: x => x.RutinaId,
                        principalTable: "Rutinas",
                        principalColumn: "RutinaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseSecondaryMuscles",
                columns: table => new
                {
                    EjerciciosSecundariosEjercicioId = table.Column<int>(type: "INTEGER", nullable: false),
                    MusculosSecundariosMusculoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseSecondaryMuscles", x => new { x.EjerciciosSecundariosEjercicioId, x.MusculosSecundariosMusculoId });
                    table.ForeignKey(
                        name: "FK_ExerciseSecondaryMuscles_Ejercicios_EjerciciosSecundariosEjercicioId",
                        column: x => x.EjerciciosSecundariosEjercicioId,
                        principalTable: "Ejercicios",
                        principalColumn: "EjercicioId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseSecondaryMuscles_Musculos_MusculosSecundariosMusculoId",
                        column: x => x.MusculosSecundariosMusculoId,
                        principalTable: "Musculos",
                        principalColumn: "MusculoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosEjercicios",
                columns: table => new
                {
                    RegistroEjercicioId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiaEntrenamientoId = table.Column<int>(type: "INTEGER", nullable: false),
                    EjercicioId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosEjercicios", x => x.RegistroEjercicioId);
                    table.ForeignKey(
                        name: "FK_RegistrosEjercicios_DiasEntrenamientos_DiaEntrenamientoId",
                        column: x => x.DiaEntrenamientoId,
                        principalTable: "DiasEntrenamientos",
                        principalColumn: "DiaEntrenamientoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrosEjercicios_Ejercicios_EjercicioId",
                        column: x => x.EjercicioId,
                        principalTable: "Ejercicios",
                        principalColumn: "EjercicioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RutinaDias",
                columns: table => new
                {
                    DiaId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiaIdUI = table.Column<int>(type: "INTEGER", nullable: false),
                    SemanaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Completado = table.Column<bool>(type: "INTEGER", nullable: false),
                    NombreRutinaDia = table.Column<string>(type: "TEXT", nullable: false),
                    Notas = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RutinaDias", x => x.DiaId);
                    table.ForeignKey(
                        name: "FK_RutinaDias_RutinaSemanas_SemanaId",
                        column: x => x.SemanaId,
                        principalTable: "RutinaSemanas",
                        principalColumn: "SemanaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosSeries",
                columns: table => new
                {
                    RegistroSerieId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RegistroEjercicioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Kilos = table.Column<double>(type: "REAL", nullable: false),
                    Reps = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosSeries", x => x.RegistroSerieId);
                    table.ForeignKey(
                        name: "FK_RegistrosSeries_RegistrosEjercicios_RegistroEjercicioId",
                        column: x => x.RegistroEjercicioId,
                        principalTable: "RegistrosEjercicios",
                        principalColumn: "RegistroEjercicioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RutinaEjercicios",
                columns: table => new
                {
                    RutinaEjercicioId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Completado = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    EjercicioId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RutinaEjercicios", x => x.RutinaEjercicioId);
                    table.ForeignKey(
                        name: "FK_RutinaEjercicios_Ejercicios_EjercicioId",
                        column: x => x.EjercicioId,
                        principalTable: "Ejercicios",
                        principalColumn: "EjercicioId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RutinaEjercicios_RutinaDias_DiaId",
                        column: x => x.DiaId,
                        principalTable: "RutinaDias",
                        principalColumn: "DiaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RutinaSeries",
                columns: table => new
                {
                    SerieId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Descanso = table.Column<string>(type: "TEXT", nullable: true),
                    Repeticiones = table.Column<int>(type: "INTEGER", nullable: true),
                    Porcentaje1RM = table.Column<int>(type: "INTEGER", nullable: true),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    EstadoSerie = table.Column<int>(type: "INTEGER", nullable: false),
                    EjercicioId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RutinaSeries", x => x.SerieId);
                    table.ForeignKey(
                        name: "FK_RutinaSeries_RutinaEjercicios_EjercicioId",
                        column: x => x.EjercicioId,
                        principalTable: "RutinaEjercicios",
                        principalColumn: "RutinaEjercicioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiasEntrenamientos_Fecha",
                table: "DiasEntrenamientos",
                column: "Fecha",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_GrupoMuscularId",
                table: "Ejercicios",
                column: "GrupoMuscularId");

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_MusculoPrimarioId",
                table: "Ejercicios",
                column: "MusculoPrimarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_Nombre",
                table: "Ejercicios",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSecondaryMuscles_MusculosSecundariosMusculoId",
                table: "ExerciseSecondaryMuscles",
                column: "MusculosSecundariosMusculoId");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoMusculares_Nombre",
                table: "GrupoMusculares",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Musculos_Nombre",
                table: "Musculos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosEjercicios_DiaEntrenamientoId",
                table: "RegistrosEjercicios",
                column: "DiaEntrenamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosEjercicios_EjercicioId",
                table: "RegistrosEjercicios",
                column: "EjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosSeries_RegistroEjercicioId",
                table: "RegistrosSeries",
                column: "RegistroEjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_RutinaDias_SemanaId",
                table: "RutinaDias",
                column: "SemanaId");

            migrationBuilder.CreateIndex(
                name: "IX_RutinaEjercicios_DiaId",
                table: "RutinaEjercicios",
                column: "DiaId");

            migrationBuilder.CreateIndex(
                name: "IX_RutinaEjercicios_EjercicioId",
                table: "RutinaEjercicios",
                column: "EjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_RutinaSemanas_RutinaId",
                table: "RutinaSemanas",
                column: "RutinaId");

            migrationBuilder.CreateIndex(
                name: "IX_RutinaSeries_EjercicioId",
                table: "RutinaSeries",
                column: "EjercicioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseSecondaryMuscles");

            migrationBuilder.DropTable(
                name: "RegistrosSeries");

            migrationBuilder.DropTable(
                name: "RutinaSeries");

            migrationBuilder.DropTable(
                name: "RegistrosEjercicios");

            migrationBuilder.DropTable(
                name: "RutinaEjercicios");

            migrationBuilder.DropTable(
                name: "DiasEntrenamientos");

            migrationBuilder.DropTable(
                name: "Ejercicios");

            migrationBuilder.DropTable(
                name: "RutinaDias");

            migrationBuilder.DropTable(
                name: "GrupoMusculares");

            migrationBuilder.DropTable(
                name: "Musculos");

            migrationBuilder.DropTable(
                name: "RutinaSemanas");

            migrationBuilder.DropTable(
                name: "Rutinas");
        }
    }
}

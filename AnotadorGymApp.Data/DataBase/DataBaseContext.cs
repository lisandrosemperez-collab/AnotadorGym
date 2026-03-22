using AnotadorGymApp.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AnotadorGymApp.Data.DataBase
{
    public class DataBaseContext : DbContext
    {                       
        public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options) { }        

        #region DbSetsMadres
        //Madres
        public DbSet<GrupoMuscular> GrupoMusculares { get; set; }        
        public DbSet<Ejercicio> Ejercicios { get;set; }        
        public DbSet<Rutinas> Rutinas { get; set; }  
        public DbSet<Musculo> Musculos { get; set; }
        public DbSet<DiaEntrenamiento> DiasEntrenamientos { get; set; }
        #endregion
        #region DbSetsHijas
        public DbSet<RegistroEjercicio> RegistrosEjercicios { get; set; }
        public DbSet<RegistroSerie> RegistrosSeries { get; set; }        
        public DbSet<RutinaSemana> RutinaSemanas { get; set; }
        public DbSet<RutinaDia> RutinaDias { get; set; }
        public DbSet<RutinaEjercicio> RutinaEjercicios { get; set; }
        public DbSet<RutinaSeries> RutinaSeries { get; set; }
        #endregion        
        #region //ModelCreating
        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);            

            #region //Ejercicio //TERMINADO
            model.Entity<Ejercicio>(entity =>
            {
                entity.HasKey(r => r.EjercicioId);
                entity.HasIndex(r => r.Nombre).IsUnique();

                entity.HasMany(r => r.RegistrosEjercicio)
                    .WithOne(r => r.Ejercicio)
                    .HasForeignKey(r => r.EjercicioId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                #region Rutina                
                entity.HasMany(r => r.RutinasEjercicios)
                    .WithOne(r => r.Ejercicio)                    
                    .HasForeignKey(r => r.EjercicioId)
                    .OnDelete(DeleteBehavior.Cascade);
                #endregion
                #region Muscles
                entity.HasMany(r => r.MusculosSecundarios)
                    .WithMany(r => r.EjerciciosSecundarios)
                    .UsingEntity(j => j.ToTable("EjercicioMusculoSecundario"));                                        
                
                entity.HasOne(r => r.MusculoPrimario)
                    .WithMany(r => r.EjerciciosPrimarios)
                    .HasForeignKey(r => r.MusculoPrimarioId)
                    .OnDelete(DeleteBehavior.Cascade);                    
                
                entity.HasOne(r => r.GrupoMuscular)
                    .WithMany(r => r.Ejercicios)
                    .HasForeignKey(r => r.GrupoMuscularId)
                    .OnDelete(DeleteBehavior.Cascade);
                #endregion
            });            
            model.Entity<DiaEntrenamiento>(entity =>
            {
                entity.HasKey(r => r.DiaEntrenamientoId);                
                entity.HasIndex(r => r.Fecha)
                    .IsUnique();

                entity.HasMany(r => r.RegistroEjercicios)
                        .WithOne(r => r.DiaEntrenamiento)
                        .HasForeignKey(r => r.DiaEntrenamientoId).OnDelete(DeleteBehavior.Cascade);
                
            });
            model.Entity<RegistroEjercicio>(entity =>
            {
                entity.HasKey(r => r.RegistroEjercicioId);

                entity.HasMany(r => r.RegistroSeries)
                    .WithOne(r => r.RegistroEjercicio)
                    .HasForeignKey(r => r.RegistroEjercicioId).OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.DiaEntrenamiento)
                    .WithMany(r => r.RegistroEjercicios)
                    .HasForeignKey(r => r.DiaEntrenamientoId).OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Ejercicio)
                    .WithMany(r => r.RegistrosEjercicio)
                    .HasForeignKey(r => r.EjercicioId).OnDelete(DeleteBehavior.Cascade);
            });
            model.Entity<RegistroSerie>(entity =>
            {
                entity.HasKey(r => r.RegistroSerieId);

                entity.HasOne(r => r.RegistroEjercicio)
                    .WithMany(r => r.RegistroSeries)
                    .HasForeignKey(r => r.RegistroEjercicioId).OnDelete(deleteBehavior: DeleteBehavior.Cascade);  
            });
            #endregion

            #region//RUTINAS
            model.Entity<Rutinas>(entity =>
            {                
                entity.Property(r => r.Nombre).IsRequired(true);
                entity.HasKey(r => r.RutinaId);
                entity.Property(r => r.RutinaId).ValueGeneratedOnAdd();

                entity.HasMany(r => r.Semanas)
                    .WithOne(r => r.Rutina)
                    .HasForeignKey(r => r.RutinaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            model.Entity<RutinaSemana>(entity =>
            {
                entity.HasKey(r => r.SemanaId);
                entity.Property(r => r.SemanaId).ValueGeneratedOnAdd();

                entity.HasMany(r => r.Dias)
                    .WithOne(r => r.Semana)
                    .HasForeignKey(r => r.SemanaId )
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rs => rs.Rutina)
                    .WithMany(r => r.Semanas)
                    .HasForeignKey(rs => rs.RutinaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });                            
            model.Entity<RutinaDia>(entity =>
            {
                entity.HasKey(r => r.DiaId);
                entity.Property(r => r.DiaId).ValueGeneratedOnAdd();

                entity.HasMany(r => r.Ejercicios)
                    .WithOne(r => r.Dia)
                    .HasForeignKey(r => r.DiaId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(rd => rd.Semana)
                    .WithMany(rs => rs.Dias)
                    .HasForeignKey(rd => rd.SemanaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            model.Entity<RutinaEjercicio>(entity =>
            {
                entity.HasKey(r => r.RutinaEjercicioId );
                entity.Property(r => r.RutinaEjercicioId).ValueGeneratedOnAdd();
                
                entity.HasMany(r => r.Series)
                    .WithOne(r => r.RutinaEjercicio)
                    .HasForeignKey(r => r.EjercicioId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(re => re.Dia)
                    .WithMany(rd => rd.Ejercicios)
                    .HasForeignKey(re => re.DiaId )
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(re => re.Ejercicio)
                    .WithMany(re => re.RutinasEjercicios)
                    .HasForeignKey(re => re.EjercicioId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(re => re.Completado).HasDefaultValue(false);

            });
            model.Entity<RutinaSeries>(entity =>
            {
                entity.HasKey(r => r.SerieId );
                entity.Property(r => r.SerieId).ValueGeneratedOnAdd();

                entity.HasOne(r => r.RutinaEjercicio)
                        .WithMany(r => r.Series)
                        .HasForeignKey(r => r.EjercicioId)
                        .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.Descanso)
                        .HasConversion(
                            v => v.HasValue ? v.Value.ToString(@"hh\:mm\:ss") : "00:00:00",
                            v => v == null || v == "00:00:00" || string.IsNullOrEmpty(v) ? TimeSpan.Zero : TimeSpan.Parse(v))
                        .HasColumnType("TEXT");
            });
            #endregion

            #region//Musculos y Grupos Musculares            
            model.Entity<GrupoMuscular>(entity =>
            {
                entity.HasKey(r => r.GrupoMuscularId);
                entity.HasIndex(r => r.Nombre).IsUnique(true);

                entity.HasMany(r => r.Ejercicios)
                    .WithOne(r => r.GrupoMuscular)
                    .HasForeignKey(r => r.GrupoMuscularId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            model.Entity<Musculo>(entity =>
            {
                entity.HasKey(r => r.MusculoId);
                entity.HasIndex(r => r.Nombre).IsUnique(true);

                entity.HasMany(r => r.EjerciciosPrimarios)
                    .WithOne(r => r.MusculoPrimario)
                    .HasForeignKey(r => r.MusculoPrimarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(r => r.EjerciciosSecundarios)
                    .WithMany(r => r.MusculosSecundarios)
                    .UsingEntity(r => r.ToTable("ExerciseSecondaryMuscles"));
            });
            #endregion            
        }
        #endregion        
        

    }
}



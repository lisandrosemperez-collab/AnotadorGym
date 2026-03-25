using AnotadorGymApp.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Data.DataBase.Services
{
    public class RegistrosService
    {
        private readonly DataBaseContext _dataBaseContext;
        public RegistrosService(DataBaseContext dataBaseContext)
        {
            _dataBaseContext = dataBaseContext;
        }
        public (DateTime inicio, DateTime fin) ObtenerRangoSemana(DateTime fechaReferencia)
        {
            int diff = (7 + (fechaReferencia.DayOfWeek - DayOfWeek.Monday)) % 7;
            var inicio = fechaReferencia.AddDays(-diff);
            var fin = inicio.AddDays(6);

            return (inicio, fin);
        }       
        public async Task<List<DiaEntrenamiento>> ObtenerDiasEntrenamientoPorRango(DateTime inicio, DateTime fin)
        {
            return await _dataBaseContext.DiasEntrenamientos
                .Where(w => w.Fecha >= inicio
                    && w.Fecha <= fin
                    && w.RegistroEjercicios.Any())
                .Include(w => w.RegistroEjercicios)
                    .ThenInclude(e => e.Ejercicio)
                .Include(w => w.RegistroEjercicios)
                    .ThenInclude(e => e.RegistroSeries)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<DiaEntrenamiento> ObtenerOCrearDiaEntrenamientoActual()
        {
            var diaActual = DateTime.Today;

            var workoutDay = await _dataBaseContext.DiasEntrenamientos
                .Include(w => w.RegistroEjercicios)
                    .ThenInclude(e => e.RegistroSeries)
                .Include(w => w.RegistroEjercicios)
                    .ThenInclude(e => e.Ejercicio)
                .AsSplitQuery()                
                .FirstOrDefaultAsync(w => w.Fecha.Date == diaActual);

            if (workoutDay is not null)
                return workoutDay;

            // Si no existe, lo creo
            workoutDay = new DiaEntrenamiento
            {
                Fecha = diaActual,                
            };

            _dataBaseContext.DiasEntrenamientos.Add(workoutDay);
            await _dataBaseContext.SaveChangesAsync();

            return workoutDay;
        }
        public async Task <RegistroEjercicio> ObtenerOCrearRegistroEjercicioAsync(RutinaSeries rutinaSeries, DiaEntrenamiento diaEntrenamientoActual)
        {
            if (rutinaSeries?.RutinaEjercicio == null || diaEntrenamientoActual == null)
            {
                Debug.WriteLine("⚠️ Datos incompletos para ObtenerOCrearExerciseLogAsync");
                return null;
            }

            // Buscar ExerciseLog existente
            var rejistroEjercicio = diaEntrenamientoActual.RegistroEjercicios
                .FirstOrDefault(w => w.EjercicioId == rutinaSeries.RutinaEjercicio.EjercicioId);

            // Si no existe, crear uno nuevo
            if (rejistroEjercicio == null)
            {
                rejistroEjercicio = new RegistroEjercicio()
                {
                    Ejercicio = rutinaSeries.RutinaEjercicio.Ejercicio,
                    EjercicioId = rutinaSeries.RutinaEjercicio.EjercicioId,
                    DiaEntrenamiento = diaEntrenamientoActual,
                    DiaEntrenamientoId = diaEntrenamientoActual.DiaEntrenamientoId
                };

                diaEntrenamientoActual.RegistroEjercicios.Add(rejistroEjercicio);
                Debug.WriteLine($"📝 Nuevo ExerciseLog creado para ejercicio: {rutinaSeries.RutinaEjercicio.Ejercicio.Nombre}");
            }

            await _dataBaseContext.SaveChangesAsync();
            return rejistroEjercicio;
        }
        public async Task <RegistroSerie> ObtenerOCrearRegistroSerieAsync(RegistroEjercicio registroEjercicio, RutinaSeries rutinaSeries)
        {            
            if (registroEjercicio == null || rutinaSeries == null)
            {
                Debug.WriteLine("⚠️ Datos incompletos para ObtenerOCrearSetLogAsync");
                return null;
            }

            // Buscar SetLog existente
            var setLog = registroEjercicio.RegistroSeries?.FirstOrDefault(s =>
                s.RegistroSerieId == rutinaSeries.SetLog?.RegistroSerieId);

            // Si no existe, crear uno nuevo
            if (setLog == null)
            {
                setLog = new RegistroSerie()
                {                                     
                    RegistroEjercicioId = registroEjercicio.RegistroEjercicioId,
                    RegistroEjercicio = registroEjercicio,
                };
                
                registroEjercicio.RegistroSeries.Add(setLog);
                Debug.WriteLine($"➕ Nuevo SetLog creado en {registroEjercicio.Ejercicio.Nombre}");
            }

            // Actualizar valores
            setLog.Kilos = rutinaSeries.KilosTemp;
            setLog.Reps = rutinaSeries.RepsTemp;
            setLog.Tipo = rutinaSeries.Tipo;
            rutinaSeries.SetLog = setLog; // Asegurar referencia bidireccional
            await _dataBaseContext.SaveChangesAsync();

            return setLog;
        }
        public async void ActualizarProgresoEjercicio(Ejercicio exercise, RegistroSerie setLog)
        {
            if (exercise == null || setLog == null)
            {
                Debug.WriteLine("⚠️ Exercise o SetLog nulo en ActualizarProgresoExerciseAsync");
                return;
            }                        

            // Solo procesar series que afectan progreso RM
            if (setLog.Tipo != TipoSerie.Max_Rm && setLog.Tipo != TipoSerie.Normal)
            {
                Debug.WriteLine($"ℹ️ Serie tipo {setLog.Tipo} - No afecta progreso RM");
                return;
            }

            // Validar datos
            if (setLog.Kilos <= 0 || setLog.Reps <= 0)
            {
                Debug.WriteLine($"⚠️ SetLog inválido: {setLog.Kilos}kg x {setLog.Reps}");
                return;
            }

            // Guardar valores anteriores para comparación
            var anteriorMejor = exercise.Mejor;
            var anteriorIniciar = exercise.Iniciar;

            // ÚLTIMO: Siempre actualizar
            exercise.Ultimo = setLog.Kilos;

            // MEJOR: Solo si es mejor que el anterior
            if (exercise.Mejor == null || setLog.Kilos > exercise.Mejor)
            {
                exercise.Mejor = setLog.Kilos;
                Debug.WriteLine($"🏆 Nuevo récord: {setLog.Kilos}kg");
            }

            // INICIAR: Solo si es Max_Rm y es nulo/cero
            if (setLog.Tipo == TipoSerie.Max_Rm)
            {
                if (exercise.Iniciar == 0 || exercise.Iniciar == null)
                {
                    exercise.Iniciar = setLog.Kilos;
                    Debug.WriteLine($"🎯 Primer RM: {setLog.Kilos}kg");
                }
            }
            await _dataBaseContext.SaveChangesAsync();

            Debug.WriteLine($"📊 Progreso - Iniciar: {exercise.Iniciar}kg, " +
                           $"Mejor: {exercise.Mejor}kg, Último: {exercise.Ultimo}kg");
        }
        public async Task EliminarTodosLosEntrenamientos()
        {
            try
            {
                var todosWorkoutDays = await _dataBaseContext.DiasEntrenamientos.ToListAsync();
                _dataBaseContext.DiasEntrenamientos.RemoveRange(todosWorkoutDays);
                await _dataBaseContext.SaveChangesAsync();

                Debug.WriteLine("🗑️ Todos los entrenamientos eliminados");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error eliminando entrenamientos: {ex.Message}");
                throw;
            }
        }
    }
}

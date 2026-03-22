using AnotadorGymApp.Data.DataBase;
using AnotadorGymApp.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Data.Initialization.Importers
{
    public class DebugDiaEntrenamientosPrueba
    {
        public async Task ImportAsync(List<DiaEntrenamiento> diaEntrenamientos, DataBaseContext _dbFactory)
        {
            var exercise = _dbFactory.Ejercicios.FirstOrDefault(e => e.Nombre == "Curl de Bíceps con Barra Recta");
            var exercise1 = _dbFactory.Ejercicios.FirstOrDefault(e => e.Nombre == "Curl de Bíceps con Barra en Banco Scott");
            var exercise2 = _dbFactory.Ejercicios.FirstOrDefault(e => e.Nombre == "Curl de Bíceps con Mancuerna en Concentración");
            var mitad = diaEntrenamientos.Count / 3;

            List<DiaEntrenamiento> primeraMitad = diaEntrenamientos.Take(mitad).ToList();
            List<DiaEntrenamiento> segundaMitad = diaEntrenamientos.Skip(mitad).Take(mitad).ToList();
            List<DiaEntrenamiento> terceraMitad = diaEntrenamientos.Skip(mitad * 2).ToList();

            Debug.WriteLine($"📊 Dividido: {primeraMitad.Count} + {segundaMitad.Count} + {terceraMitad.Count} = {diaEntrenamientos.Count} días");

            if (exercise == null || exercise1 == null)
            {
                Debug.WriteLine($"❌ Ejercicios no encontrados: 'Curva lateral de 45°'={exercise == null}, 'curl con barra'={exercise1 == null}");
                return;
            }

            await AgregarMitades(_dbFactory, primeraMitad, exercise);
            await AgregarMitades(_dbFactory, segundaMitad, exercise1);
            await AgregarMitades(_dbFactory, terceraMitad, exercise2);
        }
        private async Task AgregarMitades(DataBaseContext _dbFactory, List<DiaEntrenamiento> mitad, Ejercicio ejercicio)
        {
            foreach (var DiaEntramientoActualTemp in mitad)
            {
                try
                {
                    var diaEntrenamiento = await _dbFactory.DiasEntrenamientos.FirstOrDefaultAsync(w => w.Fecha.Date == DiaEntramientoActualTemp.Fecha.Date);
                    if (diaEntrenamiento != null)
                    {
                        _dbFactory.DiasEntrenamientos.Remove(diaEntrenamiento);
                        await _dbFactory.SaveChangesAsync();
                    }

                    diaEntrenamiento = new DiaEntrenamiento
                    {
                        Fecha = DiaEntramientoActualTemp.Fecha,
                    };
                    _dbFactory.DiasEntrenamientos.Add(diaEntrenamiento);
                    await _dbFactory.SaveChangesAsync();
                    Debug.WriteLine($"✅ Nuevo WorkoutDay creado: {diaEntrenamiento.Fecha:dd/MM/yyyy} (ID: {diaEntrenamiento.DiaEntrenamientoId})");

                    Debug.WriteLine("Buscando ExerciseLog");
                    var registroEjercicio = await _dbFactory.RegistrosEjercicios.FirstOrDefaultAsync(e => e.DiaEntrenamientoId == diaEntrenamiento.DiaEntrenamientoId && e.EjercicioId == ejercicio.EjercicioId);

                    if (registroEjercicio == null)
                    {
                        registroEjercicio = new RegistroEjercicio()
                        {                            
                            EjercicioId = ejercicio.EjercicioId,
                        };
                        diaEntrenamiento.RegistroEjercicios.Add(registroEjercicio);
                        Debug.WriteLine($"✅ Nuevo ExerciseLog creado para {ejercicio.Nombre}");
                    }
                    else
                    {
                        Debug.WriteLine($"✅ ExerciseLog existente para {ejercicio.Nombre}");
                    }


                    foreach (var registroSerieTemp in DiaEntramientoActualTemp.RegistroEjercicios.First().RegistroSeries)
                    {
                        var setLog = new RegistroSerie()
                        {
                            Kilos = registroSerieTemp.Kilos,
                            Reps = registroSerieTemp.Reps,
                            Tipo = registroSerieTemp.Tipo,
                        };
                        registroEjercicio.RegistroSeries.Add(setLog);
                    }

                    await _dbFactory.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error: {ex}");

                }
            }
        }
    }
}

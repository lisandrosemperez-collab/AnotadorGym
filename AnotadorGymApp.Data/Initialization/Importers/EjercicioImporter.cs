using AnotadorGymApp.Data.Initialization.Importers.Abstractions;
using AnotadorGymApp.Data.Models.Entities;
using AnotadorGymApp.Data.Models.Sources;
using AnotadorGymApp.Data.DataBase;
using AnotadorGymApp.Data.DataBase.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnotadorGymApp.Data.Models.DTOs.Ejercicios;

namespace AnotadorGymApp.Data.Initialization.Importers
{
    public class EjercicioImporter : IDataImporter<EjerciciosSource>
    {        
        public async Task ImportarAsync(DataBaseContext db,
        EjerciciosSource ejerciciosSource,
        IProgress<double> progress,        
        CancellationToken token)
        {
            await using var SqliteBulk = new BulkInsertScope(db);
            var ok = await IniciarDatosEjercicios(db, ejerciciosSource.Datos, progress);

            if (ok) { ok = await SqliteBulk.CommitAsync(); }
            
            ejerciciosSource.CargadoExitoso = ok;
        }        

        public async Task<bool> IniciarDatosEjercicios(DataBaseContext _dbFactory, List<EjercicioDTO> exercises, IProgress<double>? progress = null)
        {

            try
            {
                // 1. Cargar datos existentes UNA vez
                var existingBodyParts = await _dbFactory.GrupoMusculares
                    .ToDictionaryAsync(b => b.Nombre.ToLowerInvariant(), b => b);

                var existingMuscles = await _dbFactory.Musculos
                    .ToDictionaryAsync(m => m.Nombre.ToLowerInvariant(), m => m);

                var existingExerciseNames = await _dbFactory.Ejercicios
                    .Select(e => e.Nombre.ToLowerInvariant())
                    .ToHashSetAsync();

                var nuevosExercises = new List<Ejercicio>();

                // 2. Procesar en memoria
                foreach (var ex in exercises)
                {
                    try
                    {
                        // Validación rápida
                        if (string.IsNullOrWhiteSpace(ex.Nombre)) continue;

                        var nombreLower = ex.Nombre.Trim().ToLowerInvariant();

                        // Verificar si ya existe
                        if (existingExerciseNames.Contains(nombreLower)) continue;


                        // BodyPart
                        GrupoMuscular? grupoMuscular = null;
                        if (ex.GrupoMuscular != null && !string.IsNullOrWhiteSpace(ex.GrupoMuscular.Nombre))
                        {
                            var key = ex.GrupoMuscular.Nombre.Trim().ToLowerInvariant();
                            if (!existingBodyParts.TryGetValue(key, out grupoMuscular))
                            {
                                grupoMuscular = new GrupoMuscular
                                {
                                    Nombre = ex.GrupoMuscular.Nombre.Trim(),
                                };
                                existingBodyParts[key] = grupoMuscular;
                            }
                        }

                        // Primary Muscle                        
                        Musculo? musculoPrimario = null;
                        if (ex.MusculoPrimario != null && !string.IsNullOrWhiteSpace(ex.MusculoPrimario.Nombre))
                        {
                            var key = ex.MusculoPrimario.Nombre.Trim().ToLowerInvariant();
                            if (!existingMuscles.TryGetValue(key, out musculoPrimario))
                            {
                                musculoPrimario = new Musculo
                                {
                                    Nombre = ex.MusculoPrimario.Nombre.Trim(),
                                };
                                existingMuscles[key] = musculoPrimario;
                            }
                        }

                        // Secondary Muscles
                        var musculosSecundarios = new List<Musculo>();
                        if (ex.MusculosSecundarios != null)
                        {
                            foreach (var sec in ex.MusculosSecundarios.Where(s => s != null && !string.IsNullOrWhiteSpace(s.Nombre)))
                            {
                                var secKey = sec.Nombre.Trim().ToLowerInvariant();
                                if (!existingMuscles.TryGetValue(secKey, out var secondary))
                                {
                                    // Crear nuevo usando el MISMO ID de la API
                                    secondary = new Musculo
                                    {
                                        Nombre = sec.Nombre.Trim(),
                                    };
                                    existingMuscles[secKey] = secondary;
                                }
                                musculosSecundarios.Add(secondary);
                            }
                        }

                        // Crear Exercise
                        var exercise = new Ejercicio
                        {
                            Nombre = ex.Nombre.Trim(),
                            Descripcion = ex.Descripcion ?? string.Empty,
                            MusculoPrimario = musculoPrimario,
                            GrupoMuscular = grupoMuscular,
                        };

                        // Agregar músculos secundarios
                        foreach (var secMuscle in musculosSecundarios)
                        {
                            exercise.MusculosSecundarios.Add(secMuscle);
                        }

                        nuevosExercises.Add(exercise);
                        existingExerciseNames.Add(nombreLower);
                    }
                    catch (Exception innerEx)
                    {
                        Debug.WriteLine($"❌ ERROR en '{ex.Nombre}': {innerEx.Message}");
                    }
                }

                Debug.WriteLine($"🚀 Guardando {nuevosExercises.Count} elementos en batches...");

                //3 Optimizar SQLite temporalmente para inserción masiva                
                return await GuardarEnBatches(nuevosExercises, existingBodyParts, existingMuscles, progress, _dbFactory);


            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ ERROR CRÍTICO: {ex.Message}");
                progress?.Report(0);
                throw;
            }            
        }
        private async Task<bool> GuardarEnBatches(List<Ejercicio> nuevosExercises, Dictionary<string, GrupoMuscular> existingBodyParts, Dictionary<string, Musculo> existingMuscles, IProgress<double>? progress, DataBaseContext _dbFactory)
        {            
            try
            {
                //Guardar músculos y BodyParts NUEVOS
                var nuevosBodyParts = existingBodyParts.Values
                    .Where(b => b.GrupoMuscularId == 0)
                    .ToList();

                var nuevosMuscles = existingMuscles.Values
                    .Where(m => m.MusculoId == 0)
                    .ToList();

                // Guardar BodyParts nuevos
                if (nuevosBodyParts.Any())
                {
                    await _dbFactory.GrupoMusculares.AddRangeAsync(nuevosBodyParts);
                    await _dbFactory.SaveChangesAsync();

                    // Actualizar IDs en el diccionario
                    foreach (var bodyPart in nuevosBodyParts)
                    {
                        var key = bodyPart.Nombre.ToLowerInvariant();
                        existingBodyParts[key] = bodyPart;
                    }
                }

                // Guardar músculos nuevos
                if (nuevosMuscles.Any())
                {
                    await _dbFactory.Musculos.AddRangeAsync(nuevosMuscles);
                    await _dbFactory.SaveChangesAsync();

                    // Actualizar IDs en el diccionario
                    foreach (var muscle in nuevosMuscles)
                    {
                        var key = muscle.Nombre.ToLowerInvariant();
                        existingMuscles[key] = muscle;
                    }
                }

                // Ahora guardar los ejercicios nuevos
                if (nuevosExercises.Any())
                {
                    // Configurar músculos y BodyParts para los ejercicios
                    foreach (var exercise in nuevosExercises)
                    {
                        // Actualizar BodyPart si es necesario
                        if (exercise.GrupoMuscular != null && exercise.GrupoMuscular.GrupoMuscularId == 0)
                        {
                            var key = exercise.GrupoMuscular.Nombre.ToLowerInvariant();
                            if (existingBodyParts.TryGetValue(key, out var existingBodyPart))
                            {
                                exercise.GrupoMuscular = existingBodyPart;
                            }
                        }

                        // Actualizar músculo primario si es necesario
                        if (exercise.MusculoPrimario != null && exercise.MusculoPrimario.MusculoId == 0)
                        {
                            var key = exercise.MusculoPrimario.Nombre.ToLowerInvariant();
                            if (existingMuscles.TryGetValue(key, out var existingMuscle))
                            {
                                exercise.MusculoPrimario = existingMuscle;
                            }
                        }

                        // Actualizar músculos secundarios si es necesario
                        if (exercise.MusculosSecundarios.Any())
                        {
                            var secondaryMusclesToUpdate = new List<Musculo>();
                            foreach (var secMuscle in exercise.MusculosSecundarios)
                            {
                                if (secMuscle.MusculoId == 0)
                                {
                                    var key = secMuscle.Nombre.ToLowerInvariant();
                                    if (existingMuscles.TryGetValue(key, out var existingMuscle))
                                    {
                                        secondaryMusclesToUpdate.Add(existingMuscle);
                                    }
                                    else
                                    {
                                        secondaryMusclesToUpdate.Add(secMuscle);
                                    }
                                }
                                else
                                {
                                    secondaryMusclesToUpdate.Add(secMuscle);
                                }
                            }

                            // Limpiar y agregar los músculos actualizados
                            exercise.MusculosSecundarios.Clear();
                            foreach (var muscle in secondaryMusclesToUpdate)
                            {
                                exercise.MusculosSecundarios.Add(muscle);
                            }
                        }
                    }
                }

                // Guardar ejercicios en batches
                const int TAMANO_BATCH = 100;
                int guardados = 0;
                int total = nuevosExercises.Count;

                for (int i = 0; i < total; i += TAMANO_BATCH)
                {
                    var batch = nuevosExercises.Skip(i).Take(TAMANO_BATCH).ToList();
                    await _dbFactory.Ejercicios.AddRangeAsync(batch);
                    await _dbFactory.SaveChangesAsync(); // Guardar este batch

                    guardados += batch.Count;
                    double nuevoProgreso = (double)guardados / total * 80;
                    progress?.Report(nuevoProgreso);

                    Debug.WriteLine($"🔍 DESPUÉS: Progreso = {nuevoProgreso}");
                    await Task.Delay(100);
                }
                                            
                Debug.WriteLine($"✅ Guardado completado: {nuevosExercises.Count} ejercicios nuevos");
                return true;
            }
            catch (Exception ex)
            {                
                Debug.WriteLine($"❌ ERROR durante el guardado: {ex.Message}");
                Debug.WriteLine($"Detalles: {ex.InnerException?.Message}");
                return false;
                throw;
            }
        }        
    }
}

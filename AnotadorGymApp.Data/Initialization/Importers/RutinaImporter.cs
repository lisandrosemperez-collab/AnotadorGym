using AnotadorGymApp.Data.DataBase;
using AnotadorGymApp.Data.DataBase.Services;
using AnotadorGymApp.Data.Initialization.Importers.Abstractions;
using AnotadorGymApp.Data.Models.DTOs.Rutina;
using AnotadorGymApp.Data.Models.Entities;
using AnotadorGymApp.Data.Models.Sources;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Data.Initialization.Importers
{
    public class RutinaImporter : IDataImporter<RutinasSource>
    {        

        public async Task ImportarAsync(DataBaseContext db, 
        RutinasSource rutinasSource,
        IProgress<double>? progress,
        CancellationToken token)
        {
            await using var SqliteBulk = new BulkInsertScope(db);
            var ok = await IniciarDatosRutinas(db, rutinasSource.Datos, token, progress);

            if (ok) {ok = await SqliteBulk.CommitAsync(); }            

            rutinasSource.CargadoExitoso = ok;
        }        
        public async Task<bool> IniciarDatosRutinas(DataBaseContext _dbFactory, List<RutinaDto> rutinasDTO, CancellationToken token, IProgress<double>? progress = null)
        {
            if (rutinasDTO == null || !rutinasDTO.Any())
            {
                Debug.WriteLine("⚠️ Lista de rutinas vacía");
                return false;
            }
            var exercisesNoEncontrados = new List<string>();
            var rutinasGuardadas = 0;            

            try
            {
                #region COMPROBACION DE RUTINAS NUEVAS
                var rutinasJson = rutinasDTO.Select(r => r.Nombre.Trim().ToLower())
                                        .Where(r => !string.IsNullOrEmpty(r))
                                        .Distinct()
                                        .ToArray();
                Debug.WriteLine($"📊 {rutinasJson.Length} nombres únicos de rutinas en input");

                var rutinasExistentes = await _dbFactory.Rutinas
                                                        .Where(n => rutinasJson.Contains(n.Nombre.Trim().ToLower()))
                                                        .Select(r => r.Nombre.Trim().ToLower())
                                                        .ToHashSetAsync();
                Debug.WriteLine($"📊 {rutinasExistentes.Count} rutinas ya existen en BD");

                var rutinasNuevasDTO = rutinasDTO.Where(r => !rutinasExistentes.Contains(r.Nombre.Trim().ToLower())).ToList();

                if (!rutinasNuevasDTO.Any())
                {
                    Debug.WriteLine("✅ Todas las rutinas ya existen en la base de datos");
                    return false;
                }
                Debug.WriteLine($"🆕 {rutinasNuevasDTO.Count} rutinas nuevas para agregar");
                #endregion

                #region PRECARGAR EXERCISES
                var nombreExercises = rutinasNuevasDTO.SelectMany(r => r.Semanas)
                                            .SelectMany(s => s.Dias)
                                            .Where(d => d.Ejercicios != null)
                                            .SelectMany(d => d.Ejercicios)
                                            .Where(e => e?.Ejercicio != null && !string.IsNullOrWhiteSpace(e.Ejercicio.Nombre))
                                            .Select(e => e.Ejercicio.Nombre.Trim().ToLower())
                                            .Distinct()
                                            .ToArray();

                var exercisesExistentes = new Dictionary<string, Ejercicio>();

                if (!nombreExercises.Any())
                {
                    Debug.WriteLine("⚠️ No hay ejercicios para buscar en la BD");
                    exercisesExistentes = new Dictionary<string, Ejercicio>();
                }
                else
                {
                    exercisesExistentes = await _dbFactory.Ejercicios
                        .Where(e => nombreExercises.Contains(e.Nombre.Trim().ToLower()))
                        .ToDictionaryAsync(e => e.Nombre.Trim().ToLower(), e => e);
                }

                #endregion

                #region Limpiar y preparar rutinas nuevas
                foreach (var rutina in rutinasNuevasDTO.ToList())
                {                    
                    foreach (var semana in rutina.Semanas.ToList())
                    {                        
                        foreach (var dia in semana.Dias.ToList())
                        {                            
                            #region VALIDACIÓN INICIAL
                            if (dia.Ejercicios == null)
                            {
                                semana.Dias.Remove(dia);
                                Debug.WriteLine($"🗑️ Día '{dia.NumeroDia}' eliminado (Ejercicios es null)");
                                continue;
                            }
                            #endregion

                            #region FILTRAR EJERCICIOS VÁLIDOS Y SERIES                                                                

                            foreach (var rutinaEjercicio in dia.Ejercicios.ToList())
                            {

                                if (rutinaEjercicio?.Ejercicio == null || string.IsNullOrWhiteSpace(rutinaEjercicio.Ejercicio.Nombre))
                                {
                                    dia.Ejercicios.Remove(rutinaEjercicio);
                                    Debug.WriteLine($"❌ Ejercicio eliminado (Exercise es null o sin nombre)"); continue;
                                }
                                

                                var nombreEjercicio = rutinaEjercicio.Ejercicio.Nombre.Trim().ToLower();

                                if (exercisesExistentes.TryGetValue(nombreEjercicio, out var ejercicio))
                                {
                                    rutinaEjercicio.Ejercicio.EjercicioId = ejercicio.EjercicioId;                                    
                                    Debug.WriteLine($"✅ Ejercicio encontrado: {nombreEjercicio}");
                                }
                                else
                                {
                                    dia.Ejercicios.Remove(rutinaEjercicio);
                                    exercisesNoEncontrados.Add(nombreEjercicio);
                                    Debug.WriteLine($"❌ Exercise no encontrado en día '{dia.NumeroDia}': {nombreEjercicio}");
                                    continue;
                                }
                            }
                            #endregion

                            #region VERIFICAR SI EL DÍA QUEDÓ VACÍO
                            if (!dia.Ejercicios.Any())  // Si no quedaron ejercicios válidos
                            {
                                // Eliminar el día completo de la semana
                                semana.Dias.Remove(dia);
                                Debug.WriteLine($"🗑️ Día eliminado: todos los ejercicios eran inválidos");
                            }
                            else
                            {
                                Debug.WriteLine($"✅ Día conservado: {dia.Ejercicios.Count} ejercicios válidos");
                            }
                            #endregion
                        }

                        #region VERIFICAR SI LA SEMANA QUEDÓ VACÍA
                        if (!semana.Dias.Any())
                        {
                            // Eliminar la semana completa de la rutina
                            rutina.Semanas.Remove(semana);
                            Debug.WriteLine($"🗑️ Semana eliminada: todos los días eran inválidos");
                        }
                        #endregion
                    }
                    #region VERIFICAR SI LA RUTINA QUEDÓ VACÍA
                    if (!rutina.Semanas.Any())  // Si no quedaron semanas válidas
                    {
                        // Eliminar la rutina completa
                        rutinasNuevasDTO.Remove(rutina);
                        Debug.WriteLine($"🗑️ Rutina '{rutina.Nombre}' eliminada: todas las semanas eran inválidas");
                    }
                    #endregion
                }
                #endregion

                var rutinas = rutinasNuevasDTO.Select(r => new Rutinas
                {
                    Nombre = r.Nombre.Trim(),
                    Descripcion = r.Descripcion?.Trim(),
                    FrecuenciaPorGrupo = r.FrecuenciaPorGrupo?.Trim(),
                    Dificultad = r.Dificultad?.Trim(),
                    TiempoPorSesion = r.TiempoPorSesion?.Trim(),
                    ImageSource = r.ImageSource?.Trim(),
                    Semanas = r.Semanas.Select(s => new RutinaSemana
                    {
                        NumeroSemana = s.NumeroSemana,
                        Dias = s.Dias.Select(d => new RutinaDia
                        {
                            NumeroDia = d.NumeroDia,
                            Ejercicios = d.Ejercicios.Select(e => new RutinaEjercicio
                            {
                                EjercicioId = exercisesExistentes[e.Ejercicio.Nombre.Trim().ToLower()].EjercicioId,
                                NumeroEjercicio = e.NumeroEjercicio,
                                Series = new ObservableCollection<RutinaSeries>(e.Series.Select(ser => new RutinaSeries
                                {
                                    Repeticiones = ser.Repeticiones ?? 0,
                                    Porcentaje1RM = ser.Porcentaje1RM ?? 0,
                                    Descanso = TimeSpan.TryParse(ser.Descanso?.Trim(), out var descanso) ? descanso: TimeSpan.Zero,
                                    Tipo = ser.Tipo ?? TipoSerie.Normal,
                                    NumeroSerie = ser.NumeroSerie                                    
                                }))
                            }).ToList()
                        }).ToList()
                    }).ToList()
                }).ToList();

                #region GUARDAR EN BD                    

                progress?.Report(80);
                if (rutinas.Any())
                {
                    rutinasGuardadas = 0;
                    int totalRutinas = rutinas.Count;
                    const int PROGRESO_INICIAL = 80;
                    const int PROGRESO_FINAL = 100;

                    double incremento = (double)(PROGRESO_FINAL - PROGRESO_INICIAL) / totalRutinas;

                    foreach (var rutina in rutinas)
                    {
                        try
                        {                                                                                 
                            await _dbFactory.Rutinas.AddAsync(rutina);
                            await _dbFactory.SaveChangesAsync();                            
                            
                            rutinasGuardadas++;

                            double progreso = PROGRESO_INICIAL + rutinasGuardadas * incremento;
                            progress?.Report(Math.Min(progreso, PROGRESO_FINAL));


                            Debug.WriteLine($"✅ Rutina guardada: {rutinasGuardadas}/{totalRutinas} - {rutina.Nombre}");
                            Debug.WriteLine($"   📊 Progreso actual: {progreso:F1}%");                            
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"❌ Error al guardar rutina '{rutina.Nombre}': {ex.Message}");
                            Debug.WriteLine($"   StackTrace: {ex.StackTrace}");                            
                            continue;
                        }
                        finally
                        {
                            _dbFactory.ChangeTracker.Clear();                            
                        }

                    }

                    progress?.Report(PROGRESO_FINAL);
                    Debug.WriteLine($"🎯 Progreso final: 100% (todas las rutinas guardadas)");

                    Debug.WriteLine($"✅ Guardadas {rutinasGuardadas}/{totalRutinas} rutinas nuevas");

                    // Mostrar ejercicios no encontrados
                    if (exercisesNoEncontrados.Any())
                    {
                        var unicos = exercisesNoEncontrados.Distinct().ToList();
                        Debug.WriteLine($"⚠️ {unicos.Count} ejercicios no encontrados en BD:");
                        foreach (var ex in unicos.Take(10))
                        {
                            Debug.WriteLine($"   - {ex}");
                        }
                        if (unicos.Count > 10)
                        {
                            Debug.WriteLine($"   ... y {unicos.Count - 10} más");
                        }
                    }
                    Debug.WriteLine($"✅ Rutinas guardadas exitosamente: {rutinasNuevasDTO.Count}");                                        
                }
                else
                {
                    Debug.WriteLine("ℹ️ No hay rutinas válidas para guardar");                    
                    return false;
                }
                #endregion
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en transacción: {ex.Message}");
                return false;
            }          
            
            //await VerificarInsercion(_dbFactory.Rutinas.Count(), _dbFactory);

            return true;
        }
        private async Task VerificarInsercion(int rutinasEsperadas, DataBaseContext _dbFactory)
        {
            var rutinasInsertadas = await _dbFactory.Rutinas.ToListAsync();
            var semanasInsertadas = await _dbFactory.RutinaSemanas.ToListAsync();
            var diasInsertadas = await _dbFactory.RutinaDias.ToListAsync();
            var ejerciciosInsertados = await _dbFactory.RutinaEjercicios.ToListAsync();
            var seriesInsertadas = await _dbFactory.RutinaSeries.ToListAsync();

            Debug.WriteLine("=== RESUMEN DE INSERCIÓN ===");
            Debug.WriteLine($"Rutinas: {rutinasInsertadas.Count}/{rutinasEsperadas}");
            Debug.WriteLine($"Semanas: {semanasInsertadas.Count}");
            Debug.WriteLine($"Días: {diasInsertadas.Count}");
            Debug.WriteLine($"Ejercicios en rutinas: {ejerciciosInsertados.Count}");
            Debug.WriteLine($"Series: {seriesInsertadas.Count}");

            // Mostrar estadísticas detalladas
            foreach (var rutina in rutinasInsertadas)
            {
                var semanasDeRutina = semanasInsertadas.Count(s => s.RutinaId == rutina.RutinaId);
                var ejerciciosDeRutina = await _dbFactory.RutinaEjercicios
                    .Where(e => e.Dia.Semana.RutinaId == rutina.RutinaId)
                    .CountAsync();

                Debug.WriteLine($"  {rutina.Nombre}: {semanasDeRutina} semanas, {ejerciciosDeRutina} ejercicios");
            }
        }        
        
    }
}

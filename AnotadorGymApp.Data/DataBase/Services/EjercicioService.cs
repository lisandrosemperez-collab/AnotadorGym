using AnotadorGymApp.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Data.DataBase.Services
{
    public class EjercicioService
    {        
        public readonly DataBaseContext _database;
        public EjercicioService(DataBaseContext database)
        {
            _database = database;
        }
        public async Task<ObservableCollection<Ejercicio>> FiltrarEjercicios(string ejercicioBuscado, string? filtroTiempoSeleccionado)
        {
            var ejerciciosFiltrados = new List<Ejercicio>();

            try
            {
                var consulta = _database.Ejercicios
                                        .Include(e => e.RegistrosEjercicio)
                                            .ThenInclude(log => log.DiaEntrenamiento)
                                        .Include(e => e.RegistrosEjercicio)
                                            .ThenInclude(log => log.RegistroSeries)
                                        .Include(e => e.MusculoPrimario)
                                        .AsQueryable();

                // Solo ejercicios con registros que tengan series
                consulta = consulta.Where(e => e.RegistrosEjercicio.Any(log => log.RegistroSeries.Any()));

                // Filtro por nombre
                if (!string.IsNullOrWhiteSpace(ejercicioBuscado))
                {
                    consulta = consulta.Where(e => e.Nombre.ToLower().Contains(ejercicioBuscado.ToLower()));
                }


                if (filtroTiempoSeleccionado != "Todos")
                {
                    var dias = filtroTiempoSeleccionado switch
                    {
                        "Semana" => 7,
                        "Mes" => 30,
                        "3 Meses" => 90,
                        _ => 0
                    };

                    var fechaLimite = DateTime.Today.AddDays(-dias);

                    consulta = consulta.Where(e => e.RegistrosEjercicio
                                        .Any(log => log.DiaEntrenamiento.Fecha >= fechaLimite &&
                                                    log.RegistroSeries.Any()));

                }

                ejerciciosFiltrados = await consulta.AsSplitQuery()
                                                        .ToListAsync();

                Debug.WriteLine($"🎯 Ejercicios filtrados: {ejerciciosFiltrados.Count}");
                return new ObservableCollection<Ejercicio>(ejerciciosFiltrados);                                
            }
            catch (Exception ex) { Debug.WriteLine(ex);return new ObservableCollection<Ejercicio>(ejerciciosFiltrados); }
        }        
        public async Task<List<Ejercicio>> ObtenerEjercicios()
        {
            return await _database.Ejercicios.Select(e => 
                new Ejercicio { Nombre = e.Nombre,EjercicioId=e.EjercicioId,})
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<List<Ejercicio>> ObtenerEjerciciosRecientes7Dias()
        {
            var fechaLimite = DateTime.Today.AddDays(-7);

            var exerciseRecientes = await _database.Ejercicios.Where(e => e.RegistrosEjercicio.Any(e => e.DiaEntrenamiento.Fecha >= fechaLimite)).ToListAsync();
            return exerciseRecientes;
        }
        public async Task<ObservableCollection<Ejercicio>> ObtenerEjerciciosRecientes30Dias()
        {
            try
            {
                var fechaLimite = DateTime.Today.AddDays(-30);

                var exercises = await _database.Ejercicios
                    .Where(e => e.RegistrosEjercicio.Any(log => log.DiaEntrenamiento.Fecha >= fechaLimite))
                        .Include(e => e.RegistrosEjercicio)
                            .ThenInclude(log => log.DiaEntrenamiento)
                        .Include(e => e.RegistrosEjercicio)
                            .ThenInclude(log => log.RegistroSeries)
                    .AsSplitQuery()
                    .OrderByDescending(e => e.RegistrosEjercicio.Max(log => log.DiaEntrenamiento.Fecha))
                    .Take(10)
                    .ToListAsync();

                return new ObservableCollection<Ejercicio>(exercises);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en ObtenerEjerciciosRecientes30Dias: {ex.Message}");
                return new ObservableCollection<Ejercicio>();
            }
        }
        public async Task EliminarEjerciciosPorDefecto()
        {
            try
            {
                var todosEjercicios = await _database.Ejercicios.ToListAsync();

                _database.Ejercicios.RemoveRange(todosEjercicios);
                await _database.SaveChangesAsync();

                Debug.WriteLine($"🗑️ {todosEjercicios.Count} ejercicios personalizados eliminados");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error eliminando ejercicios personalizados: {ex.Message}");
                throw;
            }
        }        
    }
}

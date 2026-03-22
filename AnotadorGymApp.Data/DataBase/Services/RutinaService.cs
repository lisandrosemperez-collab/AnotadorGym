using AnotadorGymApp.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Data.DataBase.Services
{
    public class RutinaService
    {
        private readonly DataBaseContext _database;               
        public RutinaService(DataBaseContext database)
        {
            _database = database;
        }

        #region SETS
        public async Task DesactivarRutina(int rutinaId)
        {
            using var transaction = await _database.Database.BeginTransactionAsync();
            // SERIES
            await _database.RutinaSeries.Where(s => s.RutinaEjercicio.Dia.Semana.RutinaId == rutinaId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.EstadoSerie, 0));

            // EJERCICIOS
            await _database.RutinaEjercicios.Where(e => e.Dia.Semana.RutinaId == rutinaId)
                .ExecuteUpdateAsync(e => e.SetProperty(p => p.Completado, false));

            // DIAS
            await _database.RutinaDias.Where(d => d.Semana.RutinaId == rutinaId)
                .ExecuteUpdateAsync(d => d.SetProperty(p => p.Completado, false));

            // SEMANAS
            await _database.RutinaSemanas.Where(s => s.RutinaId == rutinaId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Completado,false));

            // RUTINA
            await _database.Rutinas.Where(r => r.RutinaId == rutinaId)
                .ExecuteUpdateAsync(r => r.SetProperty(p => p.Activa, false).SetProperty(p => p.Completado,false));

            _database.ChangeTracker.Clear();

            await transaction.CommitAsync();
        }
        public async Task<Rutinas> AgregarRutina(string nombre, int semanas,int dias)
        {
            if (string.IsNullOrWhiteSpace(nombre)) { nombre = "Rutina Del Usuario"; }
            semanas = semanas <= 0 ? 1 : semanas; dias = dias <= 0 ? 1 : dias;            

            try
            {
                var nuevaRutina = new Rutinas
                {
                    Nombre = nombre,
                    ImageSource = "rutina_default.jpg",
                    Activa = false,                    
                };
                await _database.Rutinas.AddAsync(nuevaRutina);

                var semanasNuevas = await AgregarRutinaSemana(nuevaRutina, semanas);

                if(semanasNuevas?.Any() == true)
                {
                    await AgregarRutinaDia(semanasNuevas.Last(), dias);                    
                }

                await _database.SaveChangesAsync();                
                return nuevaRutina;
            }
            catch (Exception ex)
            {                
                Debug.WriteLine($"❌ Error al agregar rutina: {ex.Message}");

                throw new ApplicationException("No se pudo crear la rutina", ex);

            }
        }
        public async Task<List<RutinaSemana>> AgregarRutinaSemana(Rutinas rutina, int semanasAAgregar)
        {
            if (rutina == null || semanasAAgregar <= 0)
                return null;

            try
            {

                int numeroSemanaInicial = rutina.Semanas.Any()
                        ? rutina.Semanas.Max(s => s.NumeroSemana) + 1
                        : 1;                
                var nuevasSemanas = new List<RutinaSemana>();

                for (int i = 0; i < semanasAAgregar; i++)
                {
                    int numeroSemana = numeroSemanaInicial + i;

                    var nuevaSemana = new RutinaSemana
                    {                        
                        RutinaId = rutina.RutinaId,
                        NombreSemana = $"Semana {numeroSemana}",
                        NumeroSemana = numeroSemanaInicial + i
                    };
                    
                    rutina.Semanas.Add(nuevaSemana);
                    nuevasSemanas.Add(nuevaSemana);
                }
                
                await _database.SaveChangesAsync();
                return nuevasSemanas;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al agregar semanas: {ex.Message}");                
                return null;
            }
        }
        public async Task<List<RutinaDia>> AgregarRutinaDia(RutinaSemana semana,int diasAAgregar)
        {
            try
            {                
                int numeroDiaInicial = semana.Dias.Any()
                        ? semana.Dias.Max(s => s.NumeroDia) + 1
                        : 1;
                var nuevosDias = new List<RutinaDia>();
                
                for (int i = 0; i < diasAAgregar; i++)
                {
                    int numeroDia = numeroDiaInicial + i;
                    var nuevodia = new RutinaDia
                    {
                        NombreRutinaDia = $"Día {numeroDia}",
                        NumeroDia = numeroDia,
                        Completado = false
                    };                                        
                    semana.Dias.Add(nuevodia);
                    nuevosDias.Add(nuevodia);
                }
                await _database.SaveChangesAsync();
                return nuevosDias;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al agregar días: {ex.Message}");
                return null;
            }
        }
        public async Task<RutinaSeries> AgregarRutinaSerie(RutinaEjercicio ejercicio)
        {
            if (ejercicio == null)
            {
                Debug.WriteLine("⚠️ itemEjercicio es nulo");
                return null;
            }
            try
            {
                var rutinaSerie = new RutinaSeries
                {
                    NumeroSerie = ejercicio.Series.Any() ? ejercicio.Series.Max(s => s.NumeroSerie) + 1 : 1,
                    EjercicioId = ejercicio.EjercicioId,
                    RutinaEjercicio = ejercicio,                    
                };
                
                ejercicio.Series.Add(rutinaSerie);
                
                await _database.SaveChangesAsync();                
                await _database.Entry(rutinaSerie).ReloadAsync();
                return rutinaSerie;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al agregar serie: {ex.Message}");
                return null;
            }


        }
        public async Task AgregarRutinaEjercicio(RutinaDia rutinaDia, List<Ejercicio> ejercicios)
        {
            if (rutinaDia == null || ejercicios == null || !ejercicios.Any()) return;

            int numeroInicial = rutinaDia.Ejercicios.Count + 1;            

            foreach (var exercise in ejercicios)
            {
                if (exercise == null) continue;
                _database.Entry(exercise).State = EntityState.Unchanged;
                var rutinaEjercicio = new RutinaEjercicio()
                {                    
                    EjercicioId = exercise.EjercicioId,
                    Ejercicio = exercise,
                    NumeroEjercicio = numeroInicial++
                };                    
                rutinaDia.Ejercicios.Add(rutinaEjercicio);
                
            }
            await _database.SaveChangesAsync();                        
        }
        public async Task<bool> EliminarRutinaAsync(Rutinas rutina)
        {
            if (rutina == null)
                return false;

            try
            {                
                _database.Rutinas.Remove(rutina);
                await _database.SaveChangesAsync();        
                
                Debug.WriteLine($"🏁 Rutina Eliminada: {rutina.Nombre}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al eliminar rutina: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> EliminarRutinaSemanas(Rutinas rutina, int semanasAEliminar)
        {
            if (rutina?.Semanas == null)
                return false;

            if (semanasAEliminar > rutina.Semanas.Count)
                semanasAEliminar = rutina.Semanas.Count;

            try
            {
                // Obtener las últimas semanas a eliminar
                var semanasParaEliminar = rutina.Semanas
                        .TakeLast(semanasAEliminar)
                        .ToList();

                if (!semanasParaEliminar.Any())
                    return false;

                foreach (var semana in semanasParaEliminar)
                {                    
                    rutina.Semanas.Remove(semana);
                }                

                Renumerar(rutina.Semanas, (s, i) => s.NumeroSemana = i);

                int cambios = await _database.SaveChangesAsync();                

                Debug.WriteLine($"✅ Eliminadas {semanasParaEliminar.Count} semanas ({cambios} cambios en BD)");
                Debug.WriteLine($"🏁 Semanas restantes: {rutina.Semanas.Count}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al eliminar semanas: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> EliminarRutinaDia(RutinaDia rutinaDia)
        {
            if (rutinaDia == null)
            {
                Debug.WriteLine("⚠️ Día es nulo");
                return false;
            }

            try
            {                             
                if (rutinaDia.Semana is RutinaSemana semana && semana.Dias.Contains(rutinaDia))
                {
                    semana.Dias.Remove(rutinaDia);
                    Renumerar(semana.Dias, (d, i) => d.NumeroDia = i);
                }                

                await _database.SaveChangesAsync();                
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error eliminando día: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> EliminarRutinaEjercicio(RutinaEjercicio rutinaEjercicio)
        {
            if (rutinaEjercicio == null)
            {
                Debug.WriteLine("⚠️ Ejercicio es nulo");
                return false;
            }

            try
            {                                
                if (rutinaEjercicio.Dia is RutinaDia rutinaDia && rutinaDia.Ejercicios.Contains(rutinaEjercicio))
                {
                    rutinaDia.Ejercicios.Remove(rutinaEjercicio);
                    Renumerar(rutinaDia.Ejercicios, (e, i) => e.NumeroEjercicio = i);
                }                

                await _database.SaveChangesAsync();

                Debug.WriteLine($"✅ Ejercicio eliminado: {rutinaEjercicio.Ejercicio?.Nombre}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error eliminando ejercicio: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> EliminarRutinaSerie(RutinaSeries serie)
        {
            if (serie == null)
            {
                Debug.WriteLine("⚠️ Serie es nula");
                return false;
            }

            try
            {                
                if (serie.RutinaEjercicio is RutinaEjercicio rutinaEjercicio && rutinaEjercicio.Series.Contains(serie))
                {
                    rutinaEjercicio.Series.Remove(serie);
                    Renumerar(rutinaEjercicio.Series, (s, i) => s.NumeroSerie = i);
                }                

                await _database.SaveChangesAsync();

                Debug.WriteLine($"✅ Serie eliminada");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error eliminando serie: {ex.Message}");
                return false;
            }
        }
        public static void Renumerar<T>(IEnumerable<T> items, Action<T, int> setNumero)
        {
            int i = 1;

            foreach (var item in items)
            {
                setNumero(item, i);
                i++;
            }
        }
        #endregion

        #region GETS
        public RutinaSemana ObtenerUltimaSemanaNoCompleta(Rutinas rutinas)
        {
            return rutinas.Semanas.FirstOrDefault(s => !s.Completado);
        }
        public RutinaDia ObtenerUltimoDiaNoCompleto(RutinaSemana semana)
        {
            return semana.Dias.FirstOrDefault(d => !d.Completado);
        }
        public async Task<Rutinas> ObtenerRutinaActual(int id)
        {
            var rutina = await _database.Rutinas
                .AsSplitQuery()                
                .Include(r => r.Semanas.OrderBy(s => s.NumeroSemana))
                    .ThenInclude(r => r.Dias.OrderBy(s => s.NumeroDia))
                        .ThenInclude(r => r.Ejercicios.OrderBy(s => s.NumeroEjercicio))
                            .ThenInclude(r => r.Series.OrderBy(s => s.NumeroSerie))
                .Include(r => r.Semanas)
                    .ThenInclude(s => s.Dias)
                        .ThenInclude(d => d.Ejercicios)
                            .ThenInclude(e => e.Ejercicio)                                    
                .FirstOrDefaultAsync(r => r.RutinaId == id);
            return rutina;
        }
        public async Task<Rutinas> ObtenerRutinaActivaConDias()
        {
            var rutina = await _database.Rutinas                
                .AsNoTracking()
                .AsSplitQuery()
                    .Include(r => r.Semanas)
                        .ThenInclude(r => r.Dias)
                                .FirstOrDefaultAsync(r => r.Activa);
            return rutina;
        }
        public async Task<(string nombre, int iD)?> ObtenerIdRutinaActiva()
        {
            return await _database.Rutinas
                .Where(r => r.Activa == true).Select(r => new ValueTuple<string,int>(r.Nombre,r.RutinaId)).FirstOrDefaultAsync();            
        }        
        public async Task<List<Rutinas>> ObtenerRutinas()
        {
            var rutinas = new List<Rutinas>();

            rutinas = await _database.Rutinas.Include(r => r.Semanas)
                                                .ThenInclude(s => s.Dias)
                                                    .AsNoTracking()
                                                        .ToListAsync();
            return rutinas;
        }
        public async Task<Rutinas> ObtenerRutinaActualyUI(int id)
        {
            Rutinas rutina = await ObtenerRutinaActual(id);
            if (rutina == null) return null;

            rutina.Semanas = new ObservableCollection<RutinaSemana>(rutina.Semanas ?? []);

            foreach (var semana in rutina.Semanas)
            {
                semana.Dias = new ObservableCollection<RutinaDia>(semana.Dias ?? []);

                foreach (var dia in semana.Dias)
                {
                    dia.Ejercicios = new ObservableCollection<RutinaEjercicio>(dia.Ejercicios ?? []);

                    foreach (var ejercicio in dia.Ejercicios)
                    {
                        ejercicio.Series = new ObservableCollection<RutinaSeries>(ejercicio.Series ?? []);
                    }
                }
            }

            return rutina;
        }

        #endregion

        #region Verificar RutinasCompletadas
        public async Task<bool> VerificarDiaCompletadoAsync(int diaId)
        {
            var dia = await _database.RutinaDias
                .Include(d => d.Ejercicios)
                .FirstOrDefaultAsync(d => d.DiaId == diaId);

            if (dia == null) return false;

            return dia.Ejercicios?.All(e => e.Completado) ?? false;
        }
        public async Task<bool> VerificarSemanaCompletadoAsync(int semanaId)
        {
            var semana = await _database.RutinaSemanas
                .Include(s => s.Dias)
                .ThenInclude(d => d.Ejercicios)
                .FirstOrDefaultAsync(s => s.SemanaId == semanaId);

            if (semana == null) return false;

            return semana.Dias?.All(d => d.Completado) ?? false;
        }
        #endregion                                
        public async Task GuardarCambiosAsync()
        {
            await _database.SaveChangesAsync();
        }          
        public async Task DebugDescansoEnBD()
        {
            await using var connection = _database.Database.GetDbConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT SerieId, Descanso FROM RutinaSeries";

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var id = reader.GetInt32(0);
                    var descanso = reader.IsDBNull(1) ? "NULL" : reader.GetString(1);
                    Debug.WriteLine($"📀 BD: Serie {id} - Descanso = '{descanso}'");
                }
            }

            command.CommandText = "SELECT SerieId, Descanso, typeof(Descanso) as TipoDato FROM RutinaSeries";

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var id = reader.GetInt32(0);
                    var descansoRaw = reader.IsDBNull(1) ? "NULL" : reader.GetString(1);
                    var tipoDato = reader.GetString(2);

                    Debug.WriteLine($"  Serie {id}: DescansoRaw='{descansoRaw}', TipoDato={tipoDato}");
                }
            }
            await connection.CloseAsync();
        }
        public async Task ActivarRutina(Rutinas rutinaActual)
        {
            var rutinaActiva = await ObtenerIdRutinaActiva();
            
            if (rutinaActiva != null && rutinaActiva.Value.iD != rutinaActual.RutinaId) await DesactivarRutina(rutinaActiva.Value.iD);            
            await _database.Rutinas
                .Where(r => r.RutinaId == rutinaActual.RutinaId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Activa, true));            
        }
        public bool HayCambiosSinGuardar()
        {
            return _database.ChangeTracker.HasChanges();
        }

        public List<RutinaSeries> InicializarTempDescanso(List<RutinaSeries> rutinaSeries,EventHandler handler)
        {            
            foreach (var serie in rutinaSeries)
            {
                if (serie.TempDescanso == null || serie.TempDescanso == TimeSpan.Zero)
                {
                    serie.TempDescanso = serie.Descanso;
                }                
                serie.DescansoTerminado -= handler;
                serie.DescansoTerminado += handler;
            }
            return rutinaSeries;
        }
    }
}

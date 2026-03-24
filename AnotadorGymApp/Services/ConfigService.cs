using AnotadorGymApp.Data.DataBase;
using AnotadorGymApp.Data.DataBase.Services;
using AnotadorGymApp.Data.Models.DTOs.Ejercicios;
using AnotadorGymApp.Data.Models.DTOs.Rutina;
using AnotadorGymApp.Data.Models.Entities;
using AnotadorGymApp.Data.Models.Results;
using AnotadorGymApp.Data.Models.Sources;
using AnotadorGymApp.Resources.Styles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AnotadorGymApp.Services
{
    public class ConfigService
    {
        public bool TemaOscuro { get; private set; }
        public ConfigService()
        {
            TemaOscuro = Preferences.Get("TemaOscuro", false);
        }

        public void GuardarTema(bool temaOscuro)
        {
            TemaOscuro = temaOscuro;
            Preferences.Set("TemaOscuro", temaOscuro);
            AplicarTema();
        }
        public void CambiarTema()
        {
            GuardarTema(!TemaOscuro);
        }
        public void AplicarTema()
        {
            var dictionaries = Application.Current.Resources.MergedDictionaries;

            var temas = dictionaries
                        .Where(d => d is DarkTheme || d is LightTheme)
                        .ToList();

            foreach (var tema in temas)
                dictionaries.Remove(tema);

            if (TemaOscuro)
                dictionaries.Add(new DarkTheme());
            else
                dictionaries.Add(new LightTheme());
        }            
        public async Task<EjerciciosSource> CargarDatosInicialesEjercicios()
        {
            List<EjercicioDTO> datos = null;
            bool usarDatosDemo = Preferences.Get("UsarDatosDemo", false);

            var archivosPrioridad = usarDatosDemo
                                        ? new[] { "EjerciciosEJEMPLO.json", "Ejercicios.json" }
                                        : new[] { "Ejercicios.json", "EjerciciosEJEMPLO.json" };

            var result = await CargarDatosAsync(apiUrl: "https://anotadorgymappapi.onrender.com/api/ejercicios/all",
                deserializarApi: json =>
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    return JsonSerializer.Deserialize<List<EjercicioDTO>>(json, options);
                },
                deserializarLocal: json => JsonSerializer.Deserialize<List<EjercicioDTO>>(json),
                archivosPrioridad: archivosPrioridad
                );

            return new EjerciciosSource { CargadoExitoso = result.exitoso, Datos = result.datos, EsDemo = result.esDemo, Origen = result.origen };
        }
        public async Task<RutinasSource> CargarDatosInicialesRutinas()
        {            
            bool usarDatosDemo = Preferences.Get("UsarDatosDemo", false);
            var archivosPrioridad = usarDatosDemo ?
                        new[] { "RutinasEJEMPLO.json", "Rutinas.json" } :
                        new[] { "Rutinas.json", "RutinasEJEMPLO.json" };

            var result = await CargarDatosAsync(apiUrl: "https://anotadorgymappapi.onrender.com/api/rutinas",
                deserializarApi: json =>
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var res = JsonSerializer.Deserialize<RutinaListResult>(json, options);
                    return res?.Items ?? new List<RutinaDto>();
                },
                deserializarLocal: json => JsonSerializer.Deserialize<List<RutinaDto>>(json),
                archivosPrioridad: archivosPrioridad
                );


            return new RutinasSource
            {
                Datos = result.datos,               
                Origen = result.origen,
                EsDemo = result.esDemo,
                CargadoExitoso = result.exitoso,                
            };                                            
        }
        private async Task<(T datos, string origen, bool esDemo,bool exitoso)>CargarDatosAsync<T>(
            string apiUrl,
            Func<string, T> deserializarApi,
            Func<string, T> deserializarLocal,
            string[] archivosPrioridad)
        {
            // 1. Intentar API
            var jsonApi = await ObtenerRespuestaApi(apiUrl);

            if (!string.IsNullOrWhiteSpace(jsonApi))
            {
                try
                {
                    var datos = deserializarApi(jsonApi);

                    if (datos != null)
                    {
                        return (datos,"API", false,true);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"⚠️ Error deserializando API: {ex.Message}");
                }
            }

            // 2. Fallback local
            foreach (var archivo in archivosPrioridad)
            {
                var jsonLocal = await LeerArchivoAsync(archivo);

                if (string.IsNullOrWhiteSpace(jsonLocal))
                    continue;
                try
                {
                    var datos = deserializarLocal(jsonLocal);

                    if (datos != null)
                    {
                        return (datos, archivo, archivo.Contains("EJEMPLO"),true);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"⚠️ Error deserializando {archivo}: {ex.Message}");
                }
            }

            throw new InvalidOperationException("No hay datos disponibles");
        }
        private async Task<string> ObtenerRespuestaApi(string url, int maxRetries = 3)
        {
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(20)
            };

            for (int intento = 1; intento <= maxRetries; intento++)
            {
                try
                {
                    Debug.WriteLine($"🌐 Intento {intento}/{maxRetries}");

                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine("✅ Respuesta OK desde API");
                        return await response.Content.ReadAsStringAsync();
                    }

                    Debug.WriteLine($"⚠️ HTTP {response.StatusCode}");
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine("⏳ Timeout");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Error: {ex.Message}");
                }

                await Task.Delay(2000 * intento);
            }

            return null;
        }
        private async Task<string> LeerArchivoAsync(string nombreArchivo)
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync(nombreArchivo);
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error leyendo {nombreArchivo}: {ex.Message}");
                return null;
            }
        }        
        public async Task<List<DiaEntrenamiento>> CargarDiaEntrenamientoPruebas()
        {
            try
            {                
                string json = await LeerArchivoAsync("WorkoutDays.json");
                Debug.WriteLine("📦 Contenido del archivo JSON:");
                Debug.WriteLine(json);

                if (string.IsNullOrWhiteSpace(json))
                {
                    Console.WriteLine("⚠️ El archivo está vacío");
                }
                else
                {
                    var datos = JsonSerializer.Deserialize<List<DiaEntrenamiento>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    Console.WriteLine($"✅ Se cargaron {datos?.Count ?? 0} WorkutDays");
                    if (datos.Any())
                    {                        
                        Debug.WriteLine($"✅WorkutDays Cargados {datos.Count}");
                        return datos;
                    }
                    else { Debug.WriteLine($"WorkutDays no tiene datos: {datos.Count}"); }                    
                }
                return new List<DiaEntrenamiento>();
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); return new List<DiaEntrenamiento>(); }
        }
        public async Task<bool> CargarImagenesRutinasAsync(DataBaseContext _dbFactory,ImagenPersistenteService imagenPersistenteService)
        {
            try
            {
                var rutinas = await _dbFactory.Rutinas.ToListAsync();
                foreach (var rutina in rutinas)
                {                   
                    var nuevaRuta = await imagenPersistenteService.CopiarImagenEmbebidaAsync(rutina.ImageSource);
                    if (nuevaRuta != null)
                    {
                        rutina.ImageSource = nuevaRuta;
                    }
                    else
                    {                        
                        var defaultRuta = await imagenPersistenteService.CopiarImagenEmbebidaAsync("rutina_default.jpg");
                        rutina.ImageSource = defaultRuta ?? "rutina_default.jpg";
                    }
                }
                await _dbFactory.SaveChangesAsync();
                return true;
            } catch (Exception ex) 
            {                
                Debug.WriteLine($"Error Cargando Imagenes Rutinas: {ex.Message}"); 
                return false;
            }
        }
    }
}

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
            try
            {                                
                #region API/JSON => BaseDeDatos

                List<EjercicioDTO> datos = null;                    
                bool usarDatosDemo = Preferences.Get("UsarDatosDemo", false);

                // PRIMERO INTENTAR CARGAR DESDE LA API                
                try
                {
                    Debug.WriteLine("🌐 Intentando cargar ejercicios desde la API...");

                // Ajusta esta URL según tu configuración                                        
                    string apiUrl = "https://anotadorgymappapi-production.up.railway.app/api/ejercicios/all";
                    using var httpClient = new HttpClient();
                    var response = await httpClient.GetAsync(apiUrl).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };

                        // Deserializar la respuesta de la API
                        datos = JsonSerializer.Deserialize<List<EjercicioDTO>>(jsonResponse,options);

                        if (datos != null && datos.Count > 0)
                        {
                            Debug.WriteLine($"✅ Cargado desde API: {datos.Count} ejercicios");
                            return new EjerciciosSource
                            {
                                Datos = datos,
                                EsDemo = false,
                                Origen = "API"
                            };
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"⚠️ API respondió con error: {response.StatusCode}");
                    }
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine($"⚠️ Error de conexión a la API: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"⚠️ Error al procesar respuesta de la API: {ex.Message}");
                }                

                // SI FALLA LA API, INTENTAR CARGAR DESDE ARCHIVOS LOCALES
                if (datos == null || datos.Count == 0)
                {
                    Debug.WriteLine("🔍 API no disponible o sin datos, intentando con archivos locales...");
                    var archivosPrioridad = usarDatosDemo
                                                ? new[] { "EjerciciosEJEMPLO.json", "Ejercicios.json" }
                                                : new[] { "Ejercicios.json", "EjerciciosEJEMPLO.json" };

                    foreach (var archivo in archivosPrioridad)
                    {
                        try
                        {
                            Debug.WriteLine($"📦 Intentando cargar: {archivo}");

                            using var stream = await FileSystem.OpenAppPackageFileAsync(archivo);
                            using var reader = new StreamReader(stream);
                            string json = await reader.ReadToEndAsync().ConfigureAwait(false);
                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                datos = JsonSerializer.Deserialize<List<EjercicioDTO>>(json);

                                if (datos != null && datos.Count > 0)
                                {
                                    Debug.WriteLine($"✅ Cargado desde {archivo} : {datos.Count} ejercicios");
                                    return new EjerciciosSource
                                    {
                                        Datos = datos,
                                        EsDemo = archivo.Contains("EJEMPLO"),
                                        Origen = archivo
                                    };                                        
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"⚠️ Error cargando {archivo}: {ex.Message}");
                            continue;
                        }
                    }
                }

                throw new InvalidOperationException("No hay datos de ejercicios");                
                #endregion

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al aplicar migración de ejercicios: {ex.Message}");
                throw new InvalidOperationException("No hay datos de ejercicios");
            }
        }
        public async Task<RutinasSource> CargarDatosInicialesRutinas()
        {            
            
            bool usarDatosDemo = Preferences.Get("UsarDatosDemo", false);
            var datos = new RutinaListResult();

            //PRIMERO CARGAR DESDE API
            try
            {
                Debug.WriteLine("🌐 Intentando cargar ejercicios desde la API...");
                
                string apiUrl = "https://anotadorgymappapi-production.up.railway.app/api/rutinas";
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(apiUrl).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    // Deserializar la respuesta de la API
                    datos = JsonSerializer.Deserialize<RutinaListResult>(jsonResponse, options);

                    if (datos != null && datos.TotalCount > 0)
                    {
                        Debug.WriteLine($"✅ Cargado desde API: {datos.TotalCount} ejercicios");
                        return new RutinasSource
                        {
                            Datos = datos.Items,
                            EsDemo = false,
                            Origen = "API"
                        };
                    }
                }
                else
                {
                    Debug.WriteLine($"⚠️ API respondió con error: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"⚠️ Error de conexión a la API: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Error al procesar respuesta de la API: {ex.Message}");
            }

            //SI FALLA API, CARGAR LOCAL
            try
            {                
                var archivos = usarDatosDemo ?
                            new[] { "RutinasEJEMPLO.json", "Rutinas.json" } :
                            new[] { "Rutinas.json", "RutinasEJEMPLO.json" };

                Debug.WriteLine($"🔍 Modo actual: {(usarDatosDemo ? "DEMO" : "PRODUCCIÓN")}");
                Debug.WriteLine($"🔍 Prioridad de archivos: {string.Join(" -> ", archivos)}");
                
                string archivoCargado = null;                

                foreach (var archivo in archivos)
                {
                    try
                    {
                        Debug.WriteLine($"📦 Intentando cargar: {archivo}");

                        using var stream = await FileSystem.OpenAppPackageFileAsync(archivo);
                        if (stream == null)
                        {
                            Debug.WriteLine($"⚠️ Archivo {archivo} no encontrado");
                            continue;
                        }

                        using var reader = new StreamReader(stream);
                        var json = await reader.ReadToEndAsync().ConfigureAwait(false);

                        if (string.IsNullOrWhiteSpace(json))
                        {
                            Debug.WriteLine($"⚠️ Archivo {archivo} está vacío");
                            continue;
                        }

                        datos.Items = JsonSerializer.Deserialize<List<RutinaDto>>(json);
                        datos.TotalCount = datos.Items?.Count ?? 0;

                        if (datos == null || datos.TotalCount == 0)
                        {
                            Debug.WriteLine($"⚠️ Archivo {archivo} no contiene rutinas válidas");
                            continue;
                        }
                        
                        usarDatosDemo = archivo.Contains("EJEMPLO");
                        Debug.WriteLine($"✅ Se cargaron {datos.TotalCount} rutinas desde {archivo}");

                        return new RutinasSource
                        {
                            Datos = datos.Items,
                            EsDemo = usarDatosDemo,
                            Origen = archivo
                        };
                        
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"❌ Error al cargar {archivo}: {ex.Message}");
                        continue;
                    }
                }

                throw new InvalidOperationException("No hay datos de rutinas");                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error crítico en CargarRutinasInicialesAsync: {ex.Message}");
                throw new InvalidOperationException("No hay datos de rutinas");
            }            
        }
        public async Task<List<DiaEntrenamiento>> CargarDiaEntrenamientoPruebas()
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("WorkoutDays.json");
                using var reader = new StreamReader(stream);
                string json = await reader.ReadToEndAsync().ConfigureAwait(false);
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

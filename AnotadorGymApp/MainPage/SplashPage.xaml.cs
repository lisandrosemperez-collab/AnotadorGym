using AnotadorGymApp.Data.DataBase;
using AnotadorGymApp.Data.DataBase.Initialization;
using AnotadorGymApp.Data.Models.Entities;
using AnotadorGymApp.Data.Models.Sources;
using AnotadorGymApp.Services;
using AnotadorGymApp.Services.AppInitPersistance;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace AnotadorGymApp.MainPageViews;

public partial class SplashPage : ContentPage, INotifyPropertyChanged
{        
    private CancellationTokenSource cancellationToken;    
    private readonly DbInitializer _dbInitializer;
    private readonly ConfigService _configService; 
    private readonly IDbContextFactory<DataBaseContext> _dbFactory;    
    private readonly ImagenPersistenteService _imagenService;
    private double _progreso;    
    public double Progreso
    {
        get => _progreso;
        set
        {
            if (_progreso != value)
            {
                _progreso = value;
                OnPropertyChanged();
            }
        }
    }

    public SplashPage(DbInitializer dbInitializer,ConfigService configService, IDbContextFactory<DataBaseContext> dbContextFactory,ImagenPersistenteService imagenPersistenteService)
	{
		InitializeComponent();	 
        _dbInitializer = dbInitializer;
        _configService = configService;
        _dbFactory = dbContextFactory;
        _imagenService = imagenPersistenteService;
        BindingContext = this;
    }        
    protected override async void OnAppearing()
    {
        base.OnAppearing();        
        await InicializarAplicacion();
    }
    private async Task InicializarAplicacion()
    {
        cancellationToken = new CancellationTokenSource();        
        var tareaProgreso = Task.Run(() => ActualizarMensajeProgreso(cancellationToken.Token));

        try
        {
            await InicializarEnSegundoPlano(cancellationToken.Token);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error en inicialización: {ex}");
            throw;
        }
        finally
        {            
            if (!cancellationToken.IsCancellationRequested)
                cancellationToken.Cancel();
            
            await Task.WhenAny(tareaProgreso, Task.Delay(1000));
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            this.Window.Page = new AppShell();
        });

    }
    private async Task InicializarEnSegundoPlano(CancellationToken token)
    {
        EjerciciosSource ejercicios = null;
        RutinasSource rutinas = null;

        var estado = AppInitPersistence.LeerEstadoInicial();

        bool necesitaEjercicios =
                    estado.PrimerArranque ||
                    !estado.EjerciciosCargadoExitoso;

        bool necesitaRutinas =
                    estado.PrimerArranque ||
                    !estado.RutinasCargadoExitoso;

        bool necesitaImagenesRutinas =
                    estado.PrimerArranque ||
                    !estado.ImagenesRutinasCargadas;

        bool imagenesRutinasCargadas = estado.ImagenesRutinasCargadas;        

        try
        {                                              
            IProgress<double> progress = new Progress<double>(value =>
            {                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Progreso = value / 100.0;
                    OnPropertyChanged(nameof(Progreso));
                });
            });

            if (necesitaEjercicios)
            {
                ejercicios = await _configService.CargarDatosInicialesEjercicios();
            }

            if (necesitaRutinas)
            {
                rutinas = await _configService.CargarDatosInicialesRutinas();
            }
            
            var diasDeEjercicios = await _configService.CargarDiaEntrenamientoPruebas();

            await _dbInitializer.InitializeAsync(estado.PrimerArranque, progress, ejercicios, rutinas, diasDeEjercicios, token);

            if (necesitaImagenesRutinas)
            {
                await using var dataBase = await _dbFactory.CreateDbContextAsync(token);
                imagenesRutinasCargadas = await _configService.CargarImagenesRutinasAsync(dataBase, _imagenService);
            }

            bool ejerciciosCargadosOk =
                ejercicios?.CargadoExitoso ?? estado.EjerciciosCargadoExitoso;

            bool rutinasCargadasOk =
                rutinas?.CargadoExitoso ?? estado.RutinasCargadoExitoso;

            AppInitPersistence.GuardarEstadoInicial(
                 new EjerciciosSource
                 {
                     EsDemo = ejercicios?.EsDemo ?? estado.EjerciciosUsaDatosDemo,
                     Origen = ejercicios?.Origen ?? estado.EjerciciosOrigenDatos,
                     CargadoExitoso = ejerciciosCargadosOk
                 },
                new RutinasSource
                {
                    EsDemo = rutinas?.EsDemo ?? estado.RutinasUsaDatosDemo,
                    Origen = rutinas?.Origen ?? estado.RutinasOrigenDatos,
                    CargadoExitoso = rutinasCargadasOk
                },
                primerArranque: false,
                imagenesRutinasCargadas
            );

            if (!token.IsCancellationRequested)
            {
                await AnimarProgresoSuaveAsync(token);
            }
        }
        catch (Exception ex) { Debug.WriteLine(ex); }
        finally{ Debug.WriteLine("Inicialización en segundo plano completada."); cancellationToken?.Cancel(); }

    }
    private async Task ActualizarMensajeProgreso(CancellationToken cancellationToken)
    {
        var mensajes = new[]
        {
            "Calentando motores  🏋️",
            "Preparando las pesas  💪",
            "Cargando energía  ⚡",
            "Activando modo bestia  🦍",
            "Cargando ganancias  📈",
            "Forjando el físico  ⚒️",
            "Construyendo músculo  🏗️",
            "Preparando la batalla  🛡️",
            "Modo guerrero ON  ⚔️",
            "Legendario en proceso  👑",
            "Despertando fibras  🎯",
            "Inyectando motivación  💉",
            "Preparando la quema  🔥",
            "Afinando la técnica  ✨",
            "Cargando determinación  💯",
            "Quemando excusas  🚫",
            "Activando ganancias  📊",
            "Preparando el pump  💥",
            "Sacando el animal  🐯",
            "No pain, no gain  😤",
        };
        int indice = 0;

        while (!cancellationToken.IsCancellationRequested)
        {            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusLabel.Text = mensajes[indice];
            });
            indice = Random.Shared.Next(0, mensajes.Length);

            try
            {
                await Task.Delay(2000, cancellationToken);
            }
            catch (TaskCanceledException) { break; }
        }        
    }
    private async Task AnimarProgresoSuaveAsync(CancellationToken token)
    {
        for (double i = 0; i <= 100 && !token.IsCancellationRequested; i += 2)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Progreso = i / 100.0;
                OnPropertyChanged(nameof(Progreso));
            });
            await Task.Delay(50, token);
        }
    }
    private async Task VerificarRutinasAplicadas(DataBaseContext database)
    {
        try
        {
            Debug.WriteLine("🔍 VERIFICANDO RUTINAS...");

            var rutinas = await database.Rutinas
                .Include(r => r.Semanas)
                .AsNoTracking()
                .ToListAsync();

            if(rutinas.Count == 0 || rutinas == null)
            {
                Debug.WriteLine($"No Hay rutinas o es null {rutinas.Count}");
                return;
            }

            foreach (Rutinas rutina in rutinas)
            {
                Debug.WriteLine($"Rutina numero: {rutina.RutinaId} Nombre: {rutina.Nombre}");
                Debug.WriteLine($"Cantidad de Semanas: {rutina.Semanas.Count()}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
    private async Task VerificarMigracionesAplicadas(DataBaseContext database)
    {
        try
        {
            Debug.WriteLine("🔍 VERIFICANDO MIGRACIONES...");

            var connection = database.Database.GetDbConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT MigrationId FROM __EFMigrationsHistory";

            using var reader = command.ExecuteReader();
            Debug.WriteLine("📋 Migraciones aplicadas:");
            while (reader.Read())
            {
                string migrationId = reader.GetString(0);
                Debug.WriteLine($"✅ {migrationId}");
            }

            connection.Close();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error al verificar migraciones: {ex.Message}");
        }
    }
    private async Task VerificarWourKoutDays(DataBaseContext database)
    {
        try
        {
            Debug.WriteLine("🔍 VERIFICANDO WORKOUT DAYS...");

            var wourkoutdays = await database.DiasEntrenamientos
                .Include(r => r.RegistroEjercicios)
                    .ThenInclude(e => e.RegistroSeries)
                    .AsNoTracking()
                .OrderByDescending(w => w.Fecha)
                .ToListAsync();

            if (wourkoutdays.Count == 0 || wourkoutdays == null)
            {
                Debug.WriteLine("📭 No hay WorkoutDays en la base de datos");
                return;
            }
            Debug.WriteLine($"📊 Total de WorkoutDays: {wourkoutdays.Count}");

            foreach (DiaEntrenamiento wourkoutday in wourkoutdays)
            {
                Debug.WriteLine($"WourkoutDay numero: {wourkoutday.DiaEntrenamientoId} Fecha: {wourkoutday.Fecha.Date}");
                Debug.WriteLine($"Cantidad de ExerciseLogs: {wourkoutday.RegistroEjercicios.Count()}");
                foreach(RegistroEjercicio log in wourkoutday.RegistroEjercicios)
                {
                    Debug.WriteLine($"ExerciseLog numero: {log.RegistroEjercicioId} SetsLogs: {log.RegistroSeries.Count}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ ERROR en VerificarWorkoutDays: {ex.Message}");
            Debug.WriteLine($"🔍 StackTrace: {ex.StackTrace}");
        }
    }
    private async Task VerificarEstadisticasBaseDatos(DataBaseContext database)
    {
        try
        {
            Debug.WriteLine("📈 ESTADÍSTICAS DE LA BASE DE DATOS:");

            var totalRutinas = await database.Rutinas.CountAsync();
            var totalWorkoutDays = await database.DiasEntrenamientos.CountAsync();
            var totalExerciseLogs = await database.RegistrosEjercicios.CountAsync();
            var totalSetsLog = await database.RegistrosSeries.CountAsync();
            var totalMuscleGroup = await database.Musculos.ToListAsync();
            var totalBodyParts = await database.GrupoMusculares.ToListAsync();

            
            Debug.WriteLine($"🏋️‍♂️ Rutinas: {totalRutinas}");
            Debug.WriteLine($"📅 WorkoutDays: {totalWorkoutDays}");
            Debug.WriteLine($"📊 ExerciseLogs: {totalExerciseLogs}");
            Debug.WriteLine($"⚖️ SetsLog: {totalSetsLog}");
            Debug.WriteLine($"⚖️ Muscles: {totalMuscleGroup.Count}");
            foreach (Musculo muscle in totalMuscleGroup)
            {
                Debug.WriteLine($"⚖️ Muscle: {muscle.Nombre} Id: {muscle.MusculoId}");
            }
            Debug.WriteLine($"⚖️ BodyParts: {totalBodyParts.Count}");
            foreach (GrupoMuscular bodypart in totalBodyParts)
            {
                Debug.WriteLine($"⚖️ BodyPart: {bodypart.Nombre} Id: {bodypart.GrupoMuscularId}");                
            }
            

            // Verificar la ruta de la base de datos
            var connection = database.Database.GetDbConnection();
            Debug.WriteLine($"🗃️ Ruta de BD: {connection.DataSource}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ ERROR en VerificarEstadisticas: {ex.Message}");
        }
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        cancellationToken?.Cancel();
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected override void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
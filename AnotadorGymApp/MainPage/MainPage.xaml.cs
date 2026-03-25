using AnotadorGymApp.ConfiguracionPage;
using AnotadorGymApp.Data.DataBase.Services;
using AnotadorGymApp.Data.Models.Entities;
using AnotadorGymApp.MainPageViews;
using AnotadorGymApp.Services;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microcharts;
using Microcharts.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform.Compatibility;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AnotadorGymApp
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {        
        private readonly ConfigService _configService;
        private readonly ConfigPage _configPage;
        private readonly RutinaService rutinaService;
        private readonly RegistrosService registrosService;
        DateTime? day = DateTime.Now;
        private Rutinas rutinaActiva;
        public Rutinas RutinaActiva
        {
            get => rutinaActiva;
            set
            {
                rutinaActiva = value;
                OnPropertyChanged();
            }
        }
        private DiaEntrenamiento? diaEntrenamiento;
        public DiaEntrenamiento? DiaEntrenamiento
        {
            get => diaEntrenamiento;
            set
            {
                diaEntrenamiento = value;
                OnPropertyChanged();
            }
        }
        private ResumenSemanal resumenSemanal;
        public ResumenSemanal ResumenSemanal {
            get => resumenSemanal;
            set {
                resumenSemanal = value;
                OnPropertyChanged();
            }
        }        
        public MainPage(ConfigService configService,ConfigPage configPage,RutinaService  rutinaService,RegistrosService registrosService)
        {
            InitializeComponent();      
            _configService = configService;
            _configPage = configPage;
            this.rutinaService = rutinaService;
            this.registrosService = registrosService;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = this;
            _ = CargarUi();
        }
        private async Task CargarUi()
        {
            try
            {
                var TaskRutina = CargarRutinaAsync();
                var TaskEntrenoHoy = CargarEntrenoDeHoy();
                var TaskResumenSemanal = CargarResumenSemanal();                

                await Task.WhenAll(TaskRutina, TaskEntrenoHoy,TaskResumenSemanal);

                bool esDemo = Preferences.Get("UsarDatosDemo", false);
                bool notificacionDemo = Preferences.Get("MostrarNotificacionDemoInicial", false);

                if (esDemo && notificacionDemo)
                {

                    bool respuesta = await Shell.Current.DisplayAlert(
                        "🎯 Modo Demo",
                        "Estás usando la versión de demostración con contenido limitado.\n\n" +
                        "¿Deseas obtener la versión completa con todos los ejercicios y rutinas?",
                        "Sí, quiero la versión completa",
                        "Continuar en demo"
                    );

                    if (respuesta)
                    {
                        //EN DESARROLLO
                        //await Launcher.OpenAsync("https://tudominio.com/descargar-app-completa");
                    }
                                        
                    Preferences.Set("MostrarNotificacionDemoInicial", false);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error cargando datos iniciales: {ex.Message}");                
            }
        }
        private async Task CargarRutinaAsync()
        {
            RutinaActiva = await rutinaService.ObtenerRutinaActivaConDias();
            try
            {
                if (RutinaActiva == null)
                {                    
                    Dispatcher.Dispatch(() => RutinaActivaContenedor.IsVisible = false);
                    Dispatcher.Dispatch(() => SinRutinaContenedor.IsVisible = true);
                    Dispatcher.Dispatch(() => IniciarRutinaBorder.IsVisible = false);
                    
                    Debug.WriteLine("ℹ️ No hay rutina activa");
                }
                else
                {
                    Dispatcher.Dispatch(() => IniciarRutinaBorder.IsVisible = true);
                    Dispatcher.Dispatch(() => RutinaActivaContenedor.IsVisible = true);
                    Dispatcher.Dispatch(() => SinRutinaContenedor.IsVisible = false);
                }                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error cargando rutina: {ex.Message}");
                Dispatcher.Dispatch(() => IniciarRutinaBorder.IsVisible = false);
                Dispatcher.Dispatch(() => RutinaActivaContenedor.IsVisible = false);
                Dispatcher.Dispatch(() => SinRutinaContenedor.IsVisible = true);
            }            
        }            
        private async Task CargarEntrenoDeHoy()
        {
            try
            {
                var hoy = DateTime.Today;
                DiaEntrenamiento = await registrosService.ObtenerOCrearDiaEntrenamientoActual();

                if (DiaEntrenamiento != null && DiaEntrenamiento.RegistroEjercicios.Any())
                {
                    WorkutDayBorder.IsVisible = true;
                    ExerciseLogBorder.IsVisible = true;
                }
                else { WorkutDayBorder.IsVisible = false; ExerciseLogBorder.IsVisible = false; }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error cargando entreno: {ex.Message}");
                // También ocultar en caso de error
                WorkutDayGrid.IsVisible = false;
                ExerciseLogCollectionView.IsVisible = false;
            }
        }
        private async Task CargarResumenSemanal(int semanasAtras = 0)
        {
            try
            {
                var fechaReferencia = DateTime.Today.AddDays(-(semanasAtras * 7));
                var (fechaInicioSemana, fechaFinSemana) = registrosService.ObtenerRangoSemana(fechaReferencia);

                List<DiaEntrenamiento> diasSemana = await registrosService
                    .ObtenerDiasEntrenamientoPorRango(fechaInicioSemana, fechaFinSemana);

                ResumenSemanal = new ResumenSemanal
                {
                    FechaInicio = fechaInicioSemana,
                    FechaFin = fechaFinSemana,
                    DiasEntrenados = diasSemana.Count,
                    VolumenTotal = diasSemana.Sum(d => d.VolumenTotal),
                    EjerciciosTotal = diasSemana.Sum(d => d.EjerciciosTotal),
                    SeriesTotal = diasSemana.Sum(d => d.SeriesTotal)
                };

                // Mostrar/ocultar según si hay datos
                ResumenSemanalBorder.IsVisible = diasSemana.Any();
                SinDatosSemanalesContenedor.IsVisible = !diasSemana.Any();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error calculando resumen semanal: {ex.Message}");
                ResumenSemanalBorder.IsVisible = false;
                SinDatosSemanalesContenedor.IsVisible = true;
            }
        }
        private async void OnConfigClicked(object sender, EventArgs e)
        {
            // Efecto visual opcional
            await ConfigButton.ScaleTo(0.8, 50, Easing.Linear);
            await ConfigButton.ScaleTo(1.0, 50, Easing.Linear);
                        
            await Navigation.PushAsync(_configPage);
        }
        private void IniciarRutinaButton_Clicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("//Rutinas");            
        }
        private async void ContinuarRutinaButton_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"ComienzoRutina?rutinaId={RutinaActiva.RutinaId}");
            
        }        
        
    }

}

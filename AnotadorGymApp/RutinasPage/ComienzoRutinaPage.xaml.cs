using AnotadorGymApp.PopUp;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using AnotadorGymApp.Data.Models.Entities;
using AnotadorGymApp.Data.DataBase.Services;
using AnotadorGymApp.Services;

namespace AnotadorGymApp;
[QueryProperty(nameof(RutinaId), "rutinaId")]
public partial class ComienzoRutinaPage : ContentPage, INotifyPropertyChanged
{
    //Guardado de RutinaActual    
    private readonly RutinaService rutinaService;
    private readonly EjercicioService ejercicioService;
    private readonly RegistrosService registrosService;
    private bool _saliendo = false;
    public int RutinaId { get; set; }
    public Rutinas RutinaActual { get; set; } = new Rutinas();
    public DiaEntrenamiento DiaEntrenamientoActual { get; set; } = new DiaEntrenamiento();

    #region Comandos
    private RutinaSeries serieActual;
    public RutinaSeries SerieActual
    {
        get => serieActual;
        set
        {
            if (serieActual != value)
            {
                serieActual = value;
                OnPropertyChanged(nameof(SerieActual));
            }
        }
    }
    public ICommand PlayPauseCommand { get; private set; }
    public ICommand ActualizarCommand { get;private set; }
    #endregion

    #region Timers
    CancellationTokenSource ctsTotalTimer = new CancellationTokenSource();
    public Stopwatch TotalTimer = new Stopwatch();
    public Stopwatch RestTimer = new Stopwatch();
    public Stopwatch ActTimer = new Stopwatch();
    private string tiempo;
    private string TiempoActivo;
    private string TiempoRest;    
    private bool _isPopupOpen;
    #endregion

    #region Notify
    public event PropertyChangedEventHandler? PropertyChanged;    
    public string tiemporest { get { return TiempoRest; } set 
        { 
            TiempoRest = value;
            OnPropertyChanged(nameof(tiemporest));
        }}
    public string tiempoactivo { get { return TiempoActivo; } set
        {
            TiempoActivo = value; OnPropertyChanged(nameof(tiempoactivo));
        }}
    public string Tiempo { get { return tiempo; } set 
        {            
            tiempo = value; OnPropertyChanged(nameof(Tiempo));            
        } }
    #endregion
    public ComienzoRutinaPage(RutinaService rutinaService,EjercicioService ejercicioService,RegistrosService registrosService)
	{
        this.registrosService = registrosService;
        this.rutinaService = rutinaService;     
        this.ejercicioService = ejercicioService;
        InitializeComponent();        
        PlayPauseCommand = new Command((parameters) =>
        {
            if (parameters is ValueTuple<RutinaSeries, RutinaEjercicio> tuple)
            {                
                var (rutinaSeries,exercise) = tuple;
                PlayPause(rutinaSeries, exercise);
            }            
        });                                     
    }   
    private void PlayPause(RutinaSeries rutinaSeries,RutinaEjercicio exercise)
    {
        if (!TotalTimer.IsRunning) { IniciarCronometro_Clicked(null,EventArgs.Empty); }                        
        var estado=rutinaSeries.EstadoSerie;
        
        switch (estado)
        {
            case 1: //PLAY
                if (SerieActual == null)
                {
                    rutinaSeries.EstadoSerie = 2;
                    IncicarActwatch(rutinaSeries);                                        
                    SerieActual = rutinaSeries;                    
                }                
                break;            

            case 2: //PARAR                
                rutinaSeries.EstadoSerie = 3;
                IniciarRestWatch(rutinaSeries);
                break;     

            case 3: //LISTO
                rutinaSeries.EstadoSerie = 4;
                rutinaSeries.DetenerRest();
                GuardarSerie(rutinaSeries);
                SerieActual = null;

                break;
            case 4: //EDITAR
                if (SerieActual == null)
                {
                    rutinaSeries.EstadoSerie = 3;
                    SerieActual = rutinaSeries;
                }
                break;
        }                                       
    }
    private async void GuardarSerie(RutinaSeries rutinaSeries)
    {
        try
        {
            if (rutinaSeries?.RutinaEjercicio?.Ejercicio == null)
            {
                Debug.WriteLine("⚠️ RutinaSeries o Exercise nulo");
                return;
            }

            var registroEjercicio = await registrosService.ObtenerOCrearRegistroEjercicioAsync(rutinaSeries, DiaEntrenamientoActual);
            Debug.WriteLine($"📝 ExerciseLog creado/obtenido - ID: {registroEjercicio?.RegistroEjercicioId}");

            var registroSerie = await registrosService.ObtenerOCrearRegistroSerieAsync(registroEjercicio, rutinaSeries);
            Debug.WriteLine($"📝 SetLog creado/obtenido - Kilos: {registroSerie?.Kilos}kg x {registroSerie?.Reps}");            

            registrosService.ActualizarProgresoEjercicio(rutinaSeries.RutinaEjercicio.Ejercicio,registroSerie);

            #region Verificar Si El Ejercicio Esta Completado
            bool SeriesCompletadas = rutinaSeries.RutinaEjercicio.Series.All(s => s.EstadoSerie == 4);
            rutinaSeries.RutinaEjercicio.Completado = SeriesCompletadas;        
            Debug.WriteLine($"📊 Ejercicio {rutinaSeries.RutinaEjercicio.Ejercicio?.Nombre} - " +
                           $"Completado: {SeriesCompletadas} " +
                           $"({rutinaSeries.RutinaEjercicio.Series?.Count(s => s.EstadoSerie == 4)}/" +
                           $"{rutinaSeries.RutinaEjercicio.Series?.Count})");
            #endregion

            #region Verificar Si El Dia Esta Completado
            RutinaDia itemDia = CollectionDias.SelectedItem as RutinaDia;
            if (itemDia != null)
            {
                bool diaCompletado = await rutinaService.VerificarDiaCompletadoAsync(itemDia.DiaId);
            
                if (diaCompletado && !itemDia.Completado)
                {
                    itemDia.Completado = true;
                    RestTimer.Stop();
                    rutinaSeries.DetenerRest();
                    IniciarCronometro_Clicked(null, EventArgs.Empty);
                    await Shell.Current.DisplayAlert(
                                        "✅ Día Completado",
                                        "¡Felicidades! Has completado todos los ejercicios del día.\n\n" +
                                        "¿Qué deseas hacer?\n" +
                                        "• Agregar ejercicio extra\n" +
                                        "• Finalizar el día",
                                        "Continuar");
                    Debug.WriteLine($"📅 Día {itemDia.NombreRutinaDia} marcado como completado");
                }
                else { itemDia.Completado = false; }
            }
            #endregion

            #region Verificar Si La Semana Se Completo
            RutinaSemana itemSemana = CollectionSemanas.SelectedItem as RutinaSemana;
            if (itemSemana != null)
            {
                bool semanaCompleta = await rutinaService.VerificarSemanaCompletadoAsync(itemSemana.SemanaId);
                if (semanaCompleta && !itemSemana.Completado)
                {
                    itemSemana.Completado = true;
                    await Shell.Current.DisplayAlert("Rutina Terminada", "Rutina Terminada, Empieze una nueva rutina", "Ok");
                }
            }            
            #endregion

            await rutinaService.GuardarCambiosAsync();
            Debug.WriteLine("💾 Cambios guardados en la base de datos");

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error en GuardarSerie: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "No se pudo guardar la serie", "OK");
        }
    }
    private async void IncicarActwatch(RutinaSeries rutinaSeries)
    {                
        ActTimer.Start();
        RestTimer.Stop();
        
        rutinaSeries.DetenerRest();

        while (rutinaSeries.EstadoSerie ==2)
        {
            tiempoactivo = $"{ActTimer.Elapsed.Minutes:D2}:{ActTimer.Elapsed.Seconds:D2}";
            await Task.Delay(1000);
        }
        ActTimer.Stop();
    }
    private async void IniciarRestWatch(RutinaSeries rutinaSeries)
    {                
        RestTimer.Start();
        rutinaSeries.ComienzoRest();

        while (RestTimer.IsRunning)
        {
            tiemporest = $"{RestTimer.Elapsed.Minutes:D2}:{RestTimer.Elapsed.Seconds:D2}";
            await Task.Delay(1000).ConfigureAwait(false);
        }        
    }      
    private async void IniciarCronometro_Clicked(object? sender, EventArgs e)
    {                
        if (!TotalTimer.IsRunning) 
        {
            IniciarButton.Text = "Pausar";            
            TotalTimer.Start();

            try
            {
                while (TotalTimer.IsRunning && !ctsTotalTimer.IsCancellationRequested)
                {
                    Tiempo = $"{TotalTimer.Elapsed.Minutes:D2}:{TotalTimer.Elapsed.Seconds:D2}";

                    await Task.Delay(1000,ctsTotalTimer.Token).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException){}
        }
        else
        {
            Dispatcher.Dispatch(() =>
            {
                TotalTimer.Stop();
                IniciarButton.Text = "Iniciar";
            });

            ctsTotalTimer.Cancel();
            ctsTotalTimer.Dispose();
            ctsTotalTimer = new CancellationTokenSource();            
        }
    }

    #region AÑADIR
    private async void AñadirDia_Clicked(object sender, EventArgs e)
    {        
        if (CollectionSemanas.SelectedItem is RutinaSemana semana)
        {
            await rutinaService.AgregarRutinaDia(semana,1);
        }
        else
        {
            await DisplayAlert("Seleccione una Semana", "Primero seleccione una Semana para agregar un Dia", "Ok");
        }
    }
    private async void AñadirSemana_Clicked(object sender, EventArgs e)
    {
        var nuevasSemanas = await rutinaService.AgregarRutinaSemana(RutinaActual, 1);        
        if (nuevasSemanas?.LastOrDefault() is RutinaSemana semana)
        {
            Debug.WriteLine("Semana agregada exitosamente");
            CollectionSemanas.SelectedItem = semana;
        }
    }
    private async void AñadirEjercicio_Clicked(object sender, EventArgs e)
    {        
        if(CollectionDias.SelectedItem is RutinaDia rutinaDia)
        {
            try
            {
                var popup = new BuscarEjerciciosPopUp(ejercicioService);
                _isPopupOpen = true;
                this.ShowPopup(popup);                
                var ejercicios = await popup.Result.Task;

                var ejerciciosSeleccionados = ejercicios as List<Ejercicio>;
                
                if (ejerciciosSeleccionados.Count > 0)
                {
                    await rutinaService.AgregarRutinaEjercicio(rutinaDia, ejerciciosSeleccionados);                    
                }                

            }catch(Exception ex) { Debug.WriteLine($"{ex}"); }                                                        
        }
        else
        {
            this.DisplayAlert("Seleccione un Dia", "Primero seleccione Semana y un Dia", "Ok");
        }        
    }
    private async void AñadirSerie_Clicked(object sender, EventArgs e)
    {        
        if (sender is Button { BindingContext: RutinaEjercicio ejercicio })
        {
            await rutinaService.AgregarRutinaSerie(ejercicio);
        }
    }
    #endregion
    private void CollectionSemanas_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {        
        if (e.CurrentSelection.FirstOrDefault() is RutinaSemana semana)
        {
            semana.Dias = semana.Dias.ToObservableCollection();
            CollectionDias.ItemsSource = semana.Dias;
        }
    }
    private void CollectionDias_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is RutinaDia dia)
        {
            dia.Ejercicios = dia.Ejercicios.ToObservableCollection();
            CvEjercicios.ItemsSource = dia.Ejercicios;    
        }        
    }        

    #region Vibrar
    private void OnSerieDescansoTerminado(object sender, EventArgs e)
    {
        if (sender is RutinaSeries serie)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await MostrarAlertaDescansoTerminado();
            });
        }
    }
    private async Task MostrarAlertaDescansoTerminado()
    {
        CancellationTokenSource vibrationCts = new CancellationTokenSource();
        //Vobrar solo en Android
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            _ = Task.Run(async () =>
            {
                if (Vibration.Default.IsSupported)
                {
                    while (!vibrationCts.IsCancellationRequested)
                    {                   
                        Vibration.Default.Vibrate();
                        await Task.Delay(1000,vibrationCts.Token);
                    }
                }
            },vibrationCts.Token);
            
        }

        //Mostrar Alerta de MAUI
        await DisplayAlert("¡Descanso Terminado!", "El tiempo de descanso ha finalizado. Presiona OK para continuar.", "OK");

        vibrationCts.Cancel();
    }       
    
    #endregion
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {               
        if (sender is Microsoft.Maui.Controls.Grid Grid)
        {
            CollectionView CvSeries = Grid.FindByName("CvSeries") as CollectionView;
            Border AñadirSeriesBorder = Grid.FindByName("AñadirSeriesBorder") as Border;
            
            if (CvSeries.IsVisible)
            {
                await CvSeries.FadeTo(0, 250);
                CvSeries.IsVisible = false;
                AñadirSeriesBorder.IsVisible = false;
            }
            else
            {
                AñadirSeriesBorder.IsVisible = true;
                CvSeries.IsVisible = true;
                await CvSeries.FadeTo(1, 250);

                if (CvSeries.ItemsSource is ObservableCollection<RutinaSeries> series)
                {
                    var listSeries = series.ToList();
                    series = rutinaService.InicializarTempDescanso(listSeries, OnSerieDescansoTerminado).ToObservableCollection();

                }                
            }
        }
    }
    private async void Finalizar_Clicked(object sender, EventArgs e)
    {
        await ManejarSalidaAsync();
    }    
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        #region Abierto Por PopUp?
        if (_isPopupOpen)
        {
            _isPopupOpen = false; // ← Popup cerrado
            return;
        }
        #endregion

        #region Obtener la Rutina

        DiaEntrenamientoActual = await registrosService.ObtenerOCrearDiaEntrenamientoActual();

        if (RutinaId == 0)
        {
            var fecha = DiaEntrenamientoActual.Fecha;
            RutinaActual = await rutinaService.AgregarRutina($"Rutina Del Usuario Creado: {fecha.Date}", 1,1);
            using ImagenPersistenteService imagenPersistenteService = new ImagenPersistenteService();
            var nuevaRuta = await imagenPersistenteService.CopiarImagenEmbebidaAsync(RutinaActual.ImageSource);
            if (nuevaRuta != null) RutinaActual.ImageSource = nuevaRuta;

            Debug.WriteLine($"Imagen copiada a: {nuevaRuta}");
        }
        else
        {
            RutinaActual = await rutinaService.ObtenerRutinaActualyUI(RutinaId);
        }
        #endregion                     

        #region Comprobar Semana y Dia No Completado        

        var semanaNoCompletada = rutinaService.ObtenerUltimaSemanaNoCompleta(RutinaActual);
        CollectionSemanas.SelectedItem = semanaNoCompletada;

        if (semanaNoCompletada != null)
        {
            var diaNoCompletado = rutinaService.ObtenerUltimoDiaNoCompleto(semanaNoCompletada);

            CollectionDias.SelectedItem =
                diaNoCompletado ??
                semanaNoCompletada.Dias.FirstOrDefault();
        }
        #endregion

        BindingContext = this;
    }
    protected override void OnDisappearing(){}
    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await ManejarSalidaAsync();
        });

        return true;
    }
    private async Task ManejarSalidaAsync()
    {
        if (_saliendo) return;
        _saliendo = true;
        try
        {
            RutinaDia dia = CollectionDias.SelectedItem as RutinaDia;
            if (SerieActual == null)
            {
                bool resul = await DisplayAlert("Finalizar", "¿ Desea Finalizar la rutina y guardarla ?", "Guardar", "Cancelar");
                if (resul && dia != null)
                {
                    TotalTimer.Stop();
                    RestTimer.Stop();
                    ctsTotalTimer.Cancel();
                    await rutinaService.ActivarRutina(RutinaActual);
                    await Shell.Current.Navigation.PopToRootAsync();
                    await Shell.Current.GoToAsync("//MainPage",animate: true);
                }
            }
            else { await DisplayAlert("Termine el Ejercicio", "Termine y Guarde el Ejercicio antes de Finalizar", "OK"); }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error al salir: {ex.Message}");
            await DisplayAlert("Error", $"No se pudo guardar: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            _saliendo = false;
        }

    }

}
public class TupleConverter : IMultiValueConverter
{
    public object? Convert(object[] value, Type targetType, object? parameter, CultureInfo culture)
    {                        
        if (value[0] is RutinaSeries && value[1] is RutinaEjercicio )
        {
            return new ValueTuple<RutinaSeries, RutinaEjercicio>((RutinaSeries)value[0], (RutinaEjercicio)value[1]);
        }
        Debug.WriteLine(value?.GetType());
        return null;
    }

    public object[] ConvertBack(object? value, Type[] targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

}
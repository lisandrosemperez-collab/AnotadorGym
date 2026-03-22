using AnotadorGymApp.Data.DataBase.Services;
using AnotadorGymApp.Data.Models.Entities;
using AnotadorGymApp.Services.RegistroEjercicios;
using Microcharts;
using Microcharts.Maui;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AnotadorGymApp.MetricasPageViews;

public partial class MetricasPage : ContentPage, INotifyPropertyChanged
{
    private readonly EjercicioService ejercicioService;
    private ObservableCollection<EjercicioConMetricas> _ejerciciosConMetricas = new ObservableCollection<EjercicioConMetricas>();
    public ObservableCollection<EjercicioConMetricas> EjerciciosConMetricas
    {
        get => _ejerciciosConMetricas;
        set
        {
            _ejerciciosConMetricas = value;
            OnPropertyChanged(nameof(EjerciciosConMetricas));
        }
    }
    private string filtroTiempoSeleccionado = "Todos";
    public string FiltroTiempoSeleccionado { get => filtroTiempoSeleccionado; set
        {
            filtroTiempoSeleccionado = value;
            OnPropertyChanged(nameof(FiltroTiempoSeleccionado));            
        } 
    }

    #region Ejercicio Buscado

    private string ejercicioBuscado = string.Empty;
    public string EjercicioBuscado { get => ejercicioBuscado; set
        {
            ejercicioBuscado = value;
            OnPropertyChanged(nameof(EjercicioBuscado));
            _ = DebounceFiltro();
        } 
    }
    private CancellationTokenSource _debounceCts;
    private async Task DebounceFiltro()
    {
        try
        {
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            await Task.Delay(300, _debounceCts.Token);
            await FiltrarEjercicios();            
        }
        catch (TaskCanceledException ex) { }
    }
    #endregion

    private ObservableCollection<Ejercicio> ejerciciosFiltrados;
    public ObservableCollection<Ejercicio> EjerciciosFiltrados
    {
        get => ejerciciosFiltrados;
        set
        {
            ejerciciosFiltrados = value;
            OnPropertyChanged();
            InicializarCharts();
        }
    }
    private ICommand selecionarFiltroTiempoCommand;
    public ICommand SeleccionarFiltroTiempoCommand => selecionarFiltroTiempoCommand ??= new Command<string>(async (filtro) =>
    {
        FiltroTiempoSeleccionado = filtro;
        await FiltrarEjercicios();        
    });
    public MetricasPage(EjercicioService ejercicioService)
	{
        this.ejercicioService = ejercicioService;
        InitializeComponent();        
        BindingContext = this;        
    }
    private async Task FiltrarEjercicios()
    {        
        try
        {
            EjerciciosFiltrados = await ejercicioService
                .FiltrarEjercicios(EjercicioBuscado, FiltroTiempoSeleccionado);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error filtrando ejercicios: {ex.Message}");
        }        
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await FiltrarEjercicios();        
    }    
    private void InicializarCharts()
    {
        try
        {                                                
            if (EjerciciosFiltrados == null) return;
                        
            EjerciciosConMetricas = new ObservableCollection<EjercicioConMetricas>(
                EjerciciosFiltrados.Select(e => new EjercicioConMetricas
                {
                    Ejercicio = e,
                    TotalSesiones = e.RegistrosEjercicio?.Count ?? 0,
                })
            );          

            Debug.WriteLine($"✅ Cargados {EjerciciosConMetricas.Count} ejercicios con gráficos");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error mostrando gráfico: {ex.Message}");
        }
    }    
}

public class TimeFilterToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var filtroSeleccionado = value as string;
        var miFiltro = parameter as string;                
        bool soyActivo = (filtroSeleccionado == miFiltro);
        
        string nombreRecurso = soyActivo ?
            "GreenPrimary" :
            "GreenSecondary";

        if (Application.Current != null && Application.Current.Resources.TryGetValue(nombreRecurso, out var colorRecurso))
        {
            return colorRecurso as Color;
        }

        Debug.WriteLine($"⚠️ Recurso '{nombreRecurso}' no encontrado en el tema actual");
        return soyActivo ? Colors.Green : Colors.LightGray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
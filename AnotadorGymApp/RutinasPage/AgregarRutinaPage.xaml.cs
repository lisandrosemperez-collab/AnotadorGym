using AnotadorGymApp.Data.DataBase.Services;
using AnotadorGymApp.Data.Models.DTOs.Rutina;
using AnotadorGymApp.Data.Models.Entities;
using AnotadorGymApp.Data.Models.Interfaces;
using AnotadorGymApp.PopUp;
using AnotadorGymApp.Services;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Input;
namespace AnotadorGymApp.RutinasPage;

[QueryProperty(nameof(RutinaId),"rutinaId")]
public partial class AgregarRutinaPage : ContentPage, INotifyPropertyChanged
{
    protected void OnPropertyChanged(string nombre)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    public event PropertyChangedEventHandler PropertyChanged;
    private int _semanaIndexSeleccionado = 0;
    public int SemanaIndexSeleccionado
    {
        get
        {
            Debug.WriteLine($"🔍 GET SemanaSeleccionada: {_semanaIndexSeleccionado}");
            return _semanaIndexSeleccionado;
        }
        set
        {
            Debug.WriteLine($"🔍 SET SemanaSeleccionada: {_semanaIndexSeleccionado} → {value}");

            if (_semanaIndexSeleccionado == value) return;

            _semanaIndexSeleccionado = value;
            OnPropertyChanged(nameof(SemanaIndexSeleccionado));

            Debug.WriteLine($"📊 SemanaIndex: {value}");            
            // Usar Dispatcher para ejecutar después
            Dispatcher.Dispatch(async () =>
            {
                if (value > 0)
                {
                    await SemanaPicker_SelectedIndexChanged(value);
                    OnPropertyChanged(nameof(RutinaActual.Semanas));
                }
            });
        }
    }
    public List<int> OpcionesSemanas { get; } = new List<int>(Enumerable.Range(1, 8));
    public List<TimeSpan?> OpcionesSegundos { get; } = Enumerable.Range(0,21).Select(Range => (TimeSpan?)TimeSpan.FromSeconds(Range*15)).ToList();    
    public List<TipoSerie> TipoSerieEnum => Enum.GetValues(typeof(TipoSerie)).Cast<TipoSerie>().ToList();    

    private Rutinas rutinaActual;
    public Rutinas RutinaActual
    {
        get => rutinaActual;
        set
        {
            rutinaActual = value;
            OnPropertyChanged(nameof(RutinaActual));
        }
    }
    private int _rutinaId;
    public int RutinaId
    {
        get => _rutinaId;
        set
        {
            Debug.WriteLine($"🔄 QueryProperty SET: {_rutinaId} → {value}");
            _rutinaId = value;
        }
    }
    
    private readonly RutinaService rutinaService;
    private readonly EjercicioService ejercicioService;
    private readonly ImagenPersistenteService imagenPersistenteService;
    public ICommand ItemSeleccionadoCommand { get; }
    private bool _isPopupOpen;
    private bool _saliendo = false;
    
    public AgregarRutinaPage(ImagenPersistenteService imagenPersistenteService,RutinaService rutinaService,EjercicioService ejercicioService)
    {
        InitializeComponent();        
        this.ejercicioService = ejercicioService;
        this.rutinaService = rutinaService;
        this.imagenPersistenteService = imagenPersistenteService;
        ItemSeleccionadoCommand = new Command<object>(OnItemSeleccionado);
    }
    #region Entrys
    private void NombreRutinaEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        RutinaActual.Nombre = e.NewTextValue;
    }
    private void NombreDiaEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry && entry.BindingContext is RutinaDia rutinaDia)
        {
            rutinaDia.NombreRutinaDia = e.NewTextValue;
        }
    }
    #endregion

    #region ELIMINAR Y AGREGAR
    private async void AgregarElemento_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string tipo)
        {
            switch (tipo)
            {
                case "Dia":
                    var itemSemana = button.BindingContext as RutinaSemana;
                    await rutinaService.AgregarRutinaDia(itemSemana, 1);                    
                    break;

                case "Ejercicio":                                        
                    var itemDia = button.BindingContext as RutinaDia;

                    var popup = new BuscarEjerciciosPopUp(ejercicioService);
                    _isPopupOpen = true;
                    this.ShowPopup(popup);
                    var ejercicios = await popup.Result.Task;
                    _isPopupOpen = false;

                    var ejerciciosSeleccionados = ejercicios as List<Ejercicio>;

                    if (ejerciciosSeleccionados.Count > 0)
                    {                        
                        await rutinaService.AgregarRutinaEjercicio(itemDia, ejerciciosSeleccionados);
                    }                    
                    break;

                case "Serie":
                    var itemEjercicio = button.BindingContext as RutinaEjercicio;                    
                    await rutinaService.AgregarRutinaSerie(itemEjercicio);
                    
                    break;
            }            
        }
    }               
    private async void EliminarElemento_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string tipo)
        {
            switch (tipo)
            {
                case "Dia":
                    var dia = button.BindingContext as RutinaDia;
                    await rutinaService.EliminarRutinaDia(dia);
                    break;

                case "Ejercicio":
                    var ejercicio = button.BindingContext as RutinaEjercicio;                    
                    await rutinaService.EliminarRutinaEjercicio(ejercicio);
                    break;

                case "Serie":
                    var serie = button.BindingContext as RutinaSeries;                    
                    await rutinaService.EliminarRutinaSerie(serie);
                    break;
            }
        }
    }
    #endregion

    #region SemanaPicker
    private void AlternarExpandido_Clicked(object sender, EventArgs e)
    {
        if(sender is Button btn && btn.BindingContext is RutinaSemana semana)
        {
            
            if (semana.Seleccionado == true)
            {
                semana.Seleccionado = !semana.Seleccionado;
                btn.Text = "Mostrar";
            }
            else {
                semana.Seleccionado = !semana.Seleccionado;
                btn.Text = "Ocultar";
            }
            
        }
    }
    private async Task SemanaPicker_SelectedIndexChanged(int nuevaCantidadSemanas)
    {
        if (RutinaActual == null || nuevaCantidadSemanas < 0)
            return;

        try
        {
            int cantidadActual = RutinaActual.Semanas?.Count ?? 0;

            if (cantidadActual == nuevaCantidadSemanas)
            {
                Debug.WriteLine("✅ Ya tiene la cantidad correcta, omitiendo");
                return;
            }
            if (nuevaCantidadSemanas < cantidadActual)
            {
                await EliminarSemanasAsync(cantidadActual, nuevaCantidadSemanas);
            }
            else if (nuevaCantidadSemanas > cantidadActual)
            {
                await AgregarSemanasAsync(cantidadActual, nuevaCantidadSemanas);
            }
        }
        catch (Exception ex)
        {
            int cantidadActual = RutinaActual?.Semanas?.Count ?? 0;
            RestablecerPicker(cantidadActual);

            Debug.WriteLine($"❌ Error: {ex.Message}");

            await Application.Current.MainPage.DisplayAlert(
                "Error",
                $"Por favor, intente nuevamente.",
                "OK");
        }
    }
    private async Task EliminarSemanasAsync(int cantidadActual, int nuevaCantidad)
    {
        // Confirmación con el usuario
        bool confirmar = await Application.Current.MainPage.DisplayAlert(
            "Confirmar eliminación",
            $"¿Seguro que querés eliminar las semanas {nuevaCantidad} a {cantidadActual}?",
            "Sí, eliminar", "Cancelar");

        int semanasAEliminar = cantidadActual - nuevaCantidad;

        if (!confirmar)
        {
            RestablecerPicker(cantidadActual);
            return;
        }

        // Ejecutar eliminación
        bool eliminacionExitosa = await rutinaService.EliminarRutinaSemanas(RutinaActual, semanasAEliminar);

        if (eliminacionExitosa)
        {            
            await Application.Current.MainPage.DisplayAlert(
                "Éxito",
                "Semanas eliminadas correctamente",
                "OK");
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                "No se pudieron eliminar las semanas",
                "OK");
            RestablecerPicker(cantidadActual);
        }
    }
    private async Task AgregarSemanasAsync(int cantidadActual, int nuevaCantidad)
    {
        int semanasAAgregar = nuevaCantidad - cantidadActual;

        var semanasNuevas = await rutinaService.AgregarRutinaSemana(RutinaActual,semanasAAgregar);

        if (semanasNuevas.Count > 0)
        {            
            await Application.Current.MainPage.DisplayAlert(
                "Éxito",
                $"{semanasAAgregar} semanas agregadas correctamente",
                "OK");
            OnPropertyChanged(nameof(RutinaActual));
            OnPropertyChanged(nameof(RutinaActual.Semanas));
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                "No se pudieron agregar las semanas",
                "OK");
            RestablecerPicker(cantidadActual);
        }
    }
    private void RestablecerPicker(int cantidadOriginal)
    {
        // Restablecer el picker a su valor anterior
        int indiceAnterior = Math.Max(0, cantidadOriginal - 1);

        // Usar Dispatcher para actualizar la UI
        Dispatcher.Dispatch(() => {
            SemanaIndexSeleccionado = indiceAnterior;
        });
    }
    #endregion

    #region Seleccion Imgen Rutina
    private async void AgregarImagenButton_Clicked(object sender, EventArgs e)
    {
        try
        {            
            PermissionStatus status = await Permissions.RequestAsync<Permissions.Photos>();
            if (status != PermissionStatus.Granted)
            {
                await Application.Current.MainPage.DisplayAlert("Permiso denegado",
                    "Se necesita acceso a la galería para seleccionar una imagen", "OK");
                return;
            }                           

            var options = new PickOptions()
            {
                PickerTitle = "Seleccionar imagen de la rutina",
                FileTypes = FilePickerFileType.Images,
            };

            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                IsBusy = true;
                string rutaArchivo = await imagenPersistenteService.GuardarImagenUsuarioAsync(RutinaActual.Nombre,result);
                RutinaActual.ImageSource = rutaArchivo;
                await rutinaService.GuardarCambiosAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error seleccionando imagen: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error",
                "No se pudo seleccionar la imagen", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
    #endregion

    #region Mostrar/Ocultar CollectionView
    private void OnItemSeleccionado(object item)
    {        
        switch (item)
        {       
            case RutinaSemana semana:                                
                    Seleccionar(semana, RutinaActual.Semanas);                
                break;
            case RutinaDia dia:                                
                    Seleccionar(dia, dia.Semana.Dias);                
                break;
            case RutinaEjercicio ejercicio:                                
                    Seleccionar(ejercicio, ejercicio.Dia.Ejercicios);
                break;
        }
    }
    private void LimpiarSeleccion<T>(IEnumerable<T> lista) where T : ISeleccionable
    {        
        foreach (var item in lista)
        {
            switch (item)
            {
                case RutinaSemana semana:
                    LimpiarSeleccion(semana.Dias);
                    break;

                case RutinaDia dia:
                    LimpiarSeleccion(dia.Ejercicios);
                    break;
            }

            item.Seleccionado = false;
        }
    }
    private void Seleccionar<T>(T item, IEnumerable<T> lista) where T : ISeleccionable
    {        
        // Si ya estaba seleccionado → deseleccionar
        if (item.Seleccionado)
        {
            item.Seleccionado = false;
            LimpiarHijos(item);
            return;
        }

        // Desmarcar hermanos
        foreach (var i in lista)
        {
            if (i.Seleccionado)
            {
                i.Seleccionado = false;
                LimpiarHijos(i);
            }
        }

        // Marcar el actual
        item.Seleccionado = true;
    }
    private void LimpiarHijos(ISeleccionable item)
    {
        switch (item)
        {
            case RutinaSemana semana:
                LimpiarSeleccion(semana.Dias);
                break;

            case RutinaDia dia:
                LimpiarSeleccion(dia.Ejercicios);
                break;
        }
    }
    #endregion
    private async void GuardarRutinaButton_Clicked(object sender, EventArgs e)
    {
        await ManejarSalidaAsync();
    }        
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Si es por abrir un popup, no borrar
        if (_isPopupOpen)
        {
            return;
        }                                    
        BorrarUi();
    }    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Si es por un popup, no recargar
        if (_isPopupOpen)
        {
            _isPopupOpen = false;
            return;
        }        

        #region RutinaId
        try
        {
            if (RutinaId != 0)
            {
                // EDICIÓN RUTINA EXISTENTE
                var rutina = await rutinaService.ObtenerRutinaActualyUI(RutinaId);
                if (rutina != null)
                {
                    RutinaActual = rutina;
                }
                else
                {
                    await DisplayAlert("Error", "No se encontró la rutina", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }
            }
            else
            {
                // RUTINA NUEVA
                var fecha = DateTime.Now;
                RutinaActual = await rutinaService.AgregarRutina($"Rutina Del Usuario Creado: {fecha.Date}", 1, 1);
                using ImagenPersistenteService imagenPersistenteService = new ImagenPersistenteService();
                var nuevaRuta = await imagenPersistenteService.CopiarImagenEmbebidaAsync(RutinaActual.ImageSource);
                if (nuevaRuta != null) RutinaActual.ImageSource = nuevaRuta;

                Debug.WriteLine($"Imagen copiada a: {nuevaRuta}");
                RutinaId = RutinaActual.RutinaId;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex}");
            await DisplayAlert("Error", "No se pudo crear/editar la rutina", "OK");
            await Shell.Current.GoToAsync("..");
        }

        #endregion

        #region Inicializar SemanaPicker
        if (RutinaActual?.Semanas != null)
        {
            int semanasCount = RutinaActual.Semanas.Count;

            // Si no tiene semanas, agregar una por defecto
            if (semanasCount == 0)
            {
                Debug.WriteLine("➕ Rutina sin semanas, agregando primera semana...");
                await rutinaService.AgregarRutinaSemana(rutinaActual,1);
                semanasCount = RutinaActual.Semanas?.Count ?? 0;
            }            

            Debug.WriteLine($"🔧 Inicializando picker: {semanasCount}");
            SemanaIndexSeleccionado = semanasCount;

        }
        #endregion
        
        BindingContext = this;                      
    }
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
            if (RutinaActual == null)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }
            IsBusy = true;

            RutinaActual.Nombre = RutinaActual.Nombre?.Trim();

            if (string.IsNullOrWhiteSpace(RutinaActual.Nombre))
            {
                await DisplayAlert("Error", "La rutina debe tener un nombre", "OK");
                return;
            }

            if (rutinaService.HayCambiosSinGuardar())
            {
                await rutinaService.GuardarCambiosAsync();
                Debug.WriteLine("💾 Cambios finales guardados");
            }

            await Shell.Current.GoToAsync("..", animate: true);
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
    public void BorrarUi()
    {         
        RutinaActual = new Rutinas();

        RutinaId = 0;        
    }
    
}
public class IntToTimeSpanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan tiempo)
        {
            if (tiempo == TimeSpan.Zero) return "Sin descanso";
            return $"{tiempo.Minutes} min {tiempo.Seconds} seg";
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (int.TryParse(value?.ToString(), out int minutos))
            return TimeSpan.FromMinutes(minutos);
        return null;
    }
}
using AnotadorGymApp.Data.DataBase.Services;
using AnotadorGymApp.Data.Models.Entities;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AnotadorGymApp.PopUp;

public partial class BuscarEjerciciosPopUp : Popup
{
    private readonly EjercicioService _ejercicioService;
    public TaskCompletionSource<List<Ejercicio>> Result { get; } = new();
    public List<Ejercicio> Ejercicios { get; set; } = new List<Ejercicio>();
    public ObservableCollection<Ejercicio> FiltradosEjercicios { get; set; } = new ObservableCollection<Ejercicio>();
    public ObservableCollection<Ejercicio> SeleccionadosEjercicios { get; set; } = new ObservableCollection<Ejercicio>();
    public List<int> seleccionadosEjercicios { get; set; } = new List<int>();
    public BuscarEjerciciosPopUp(EjercicioService ejercicioService)
	{
        InitializeComponent();
        _ejercicioService = ejercicioService;        
        this.Loaded += BuscarEjerciciosPopUp_Loaded;
    }   
    private void EjerciciosSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {        
        try
        {
            var resultados = FiltrarEjercicios(e.NewTextValue ?? "");        
            FiltradosEjercicios.Clear();
                        
            foreach (var r in resultados) FiltradosEjercicios.Add(r);                        
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Excepción en TextChanged: {ex}");
        }
    }
    private IEnumerable<Ejercicio> FiltrarEjercicios(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto) || texto.Length < 2)
            return Enumerable.Empty<Ejercicio>();

        return Ejercicios
            .Where(e => e.Nombre.Contains(texto,StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => !e.Nombre.StartsWith(texto, StringComparison.OrdinalIgnoreCase))
            .Take(5);
    }
    private void FiltradosCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Ejercicio seleccionado)
        {
            if (SeleccionadosEjercicios.Any(ej => ej.Nombre == seleccionado.Nombre))
            {
                FiltradosCollectionView.SelectedItem = null;
                return;
            }
            SeleccionadosEjercicios.Add(e.CurrentSelection.FirstOrDefault() as Ejercicio);
            FiltradosCollectionView.SelectedItem = null;
        }
    }
    private void BorrarEjercicio_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Ejercicio ex)
        {
            SeleccionadosEjercicios.Remove(ex);
        }        
    }    
    private async void AceptarButton_Clicked(object sender, EventArgs e)
    {
        var seleccionados = SeleccionadosEjercicios.ToList();                
        try
        {
            Result.SetResult(seleccionados);
            await CloseAsync();
        }catch (Exception ex) {
            Debug.WriteLine(ex);                         
        }
    }
    private async void CancelarButton_Clicked(object sender, EventArgs e)
    {        
        await this.CloseAsync();
    }
    private async void BuscarEjerciciosPopUp_Loaded(object? sender, EventArgs e)
    {
        try
        {
            var ejercicios = await _ejercicioService.ObtenerEjercicios();
            Ejercicios = ejercicios;
            SeleccionadosCollectionView.ItemsSource = SeleccionadosEjercicios;
            FiltradosCollectionView.ItemsSource = FiltradosEjercicios;            
        }
        catch (Exception ex) { Debug.WriteLine($"Error al cargar ejercicios: {ex}"); }
    }    
}
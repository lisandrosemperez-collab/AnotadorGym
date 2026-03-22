using AnotadorGymApp.Data.DataBase.Services;
using AnotadorGymApp.Data.Models.Entities;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;

namespace AnotadorGymApp.RutinasPage;

public partial class PrincipalRutinasPage : ContentPage
{    
    private readonly RutinaService _rutinaService;
    public ObservableCollection<Rutinas> rutinas { get; set; } = new ObservableCollection<Rutinas>();
    public ICommand StarRutinaCommand { get; private set; }
    public ICommand EditRutinaCommand { get; private set;}
    public ICommand EliminarRutinaCommand { get; private set; }   
    public PrincipalRutinasPage(RutinaService rutinaService)
    {
        InitializeComponent();
        _rutinaService = rutinaService;        
        BindingContext = this;
        StarRutinaCommand = new Command<Rutinas>(StarRutina);
        EditRutinaCommand = new Command<Rutinas>(EditRutina);
        EliminarRutinaCommand = new Command<Rutinas>(EliminarRutina);
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();        
        _ = CargarRutinas();
    }
    private async Task CargarRutinas()
    {
        try
        {
            var rutinasList = await _rutinaService.ObtenerRutinas();

            rutinas.Clear();

            foreach (var r in rutinasList)
                rutinas.Add(r);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error:");
            Debug.WriteLine(ex);
        }
    }    
    private async void AñadirRutinaButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"AgregarRutina?rutinaId={0}");                    
    }
    private async void EditRutina(Rutinas rutina)
    {        
        await Shell.Current.GoToAsync($"AgregarRutina?rutinaId={rutina.RutinaId}");
    }    
    private async void EliminarRutina(Rutinas rutina)
    {
        if (rutina != null)
        {
            // Confirmación (opcional pero recomendable)
            bool confirmado = await Application.Current.MainPage.DisplayAlert(
                "Confirmar",
                $"¿Seguro que querés eliminar la rutina '{rutina.Nombre}'?",
                "Sí",
                "No");

            if (confirmado)
            {
                await _rutinaService.EliminarRutinaAsync(rutina);
                rutinas.Remove(rutina);                
            }
        }
    }
    private async void StarRutina(Rutinas rutina)
    {
        var idRutinaAEmpezar = 0;
        if(rutina != null)
        {
            idRutinaAEmpezar = rutina.RutinaId;
        }

        var rutinaActiva = await _rutinaService.ObtenerIdRutinaActiva();

        if(rutinaActiva.Value.iD != 0)
        {
            var eliminar = await Shell.Current.DisplayAlert(
                                        "Ya Estas En Una Rutina",
                                        $"¡CUIDADO! Ya tienes la rutina: \n" +
                                        $"{rutinaActiva.Value.nombre}\n\n" +
                                        "¿Deseas Finalizar esa rutina y Empezar la rutina:\n" +
                                        $"{rutina?.Nombre ?? "Rutina nueva del Usuario"}?\n",
                                        "Si Empezar Rutina Nueva",
                                        "No, Mantener Rutina Actual");

            if (eliminar) await _rutinaService.DesactivarRutina(rutinaActiva.Value.iD);
            else idRutinaAEmpezar = rutinaActiva.Value.iD;
        }

        await Shell.Current.GoToAsync($"ComienzoRutina?rutinaId={idRutinaAEmpezar}");
    }
    private async void EntrenamientoRapido_Clicked(object sender, EventArgs e)
    {
        StarRutina(null);
    }
}

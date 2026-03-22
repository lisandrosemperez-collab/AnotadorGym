#define WINDOWS
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.LifecycleEvents;
using System.Runtime.InteropServices;
using Microcharts.Maui;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.Json;
using AnotadorGymApp.MainPageViews;
using AnotadorGymApp.Services;
using AnotadorGymApp.Data.DataBase.Services;

namespace AnotadorGymApp
{    
    public partial class App : Application
    {                        
        private ConfigService _configService;
        private ImagenPersistenteService _imagenPersistenteService;
        private readonly Page _startPage;
        public App(ConfigService configService,ImagenPersistenteService imagenPersistenteService,SplashPage splashPage)
        {                        
            _configService = configService;
            _imagenPersistenteService = imagenPersistenteService;
            try
            {
                InitializeComponent();
                _configService.AplicarTema();
                _startPage = splashPage;                
                Debug.WriteLine("🚀 App creada - SplashPage iniciada");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en App: {ex}");
                // Fallback seguro
                _startPage = new AppShell();
            }
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            try
            {
                return new Window(_startPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en App: {ex}");
                return new Window(new AppShell());
            }
        }
    }
}

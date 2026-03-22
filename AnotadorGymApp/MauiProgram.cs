using CommunityToolkit.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.Logging;
using Microcharts.Maui;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using System.Diagnostics;
using Microsoft.Maui.Controls.PlatformConfiguration;
using AnotadorGymApp.RutinasPage;
using AnotadorGymApp.MainPageViews;
using AnotadorGymApp.MetricasPageViews;
using AnotadorGymApp.ConfiguracionPage;
using AnotadorGymApp.Services;
using AnotadorGymApp.Data.Initialization.Importers.Abstractions;
using AnotadorGymApp.Data.Models.Sources;
using AnotadorGymApp.Data.Initialization.Importers;
using AnotadorGymApp.Data.DataBase;
using AnotadorGymApp.Data.DataBase.Services;
using AnotadorGymApp.Data.DataBase.Initialization;

namespace AnotadorGymApp
{
    public static class MauiProgram
    {   
        public static MauiApp CreateMauiApp()
        {            
            string ruta = Path.Combine(FileSystem.AppDataDirectory, "GymApp.db");

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMicrocharts()
                .UseMauiCommunityToolkit()
                .Services                
                .AddSingleton<ConfigPage>()
                .AddSingleton<ImagenPersistenteService>()
                .AddSingleton<ConfigService>()
                .AddTransient<SplashPage>()
                .AddScoped<IDataImporter<EjerciciosSource>, EjercicioImporter>()
                .AddScoped<IDataImporter<RutinasSource>, RutinaImporter>()
                .AddScoped<DbInitializer>()
                .AddScoped<RutinaService>()
                .AddScoped<RegistrosService>()
                .AddScoped<EjercicioService>();

            builder.Services.AddDbContext<DataBaseContext>(
                options =>
                {
                    options.UseSqlite($"Data Source={ruta}",sqliteOptions => { sqliteOptions.MigrationsAssembly("AnotadorGymApp.Data"); });                    
                }
            );
            builder.Services.AddDbContextFactory<DataBaseContext>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }        

    }
}

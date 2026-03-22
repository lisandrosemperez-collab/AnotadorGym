using AnotadorGymApp.Data.Models.Sources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Services.AppInitPersistance
{
    public class AppInitPersistence
    {
        public static void GuardarEstadoInicial(
        EjerciciosSource ejercicios,
        RutinasSource rutinas, bool primerArranque,bool imagenesRutinas)
        {
            Preferences.Set(AppInitState.PrimerArranque, primerArranque);

            Preferences.Set(AppInitState.EjerciciosUsaDatosDemo, ejercicios.EsDemo);

            Preferences.Set(AppInitState.RutinasUsaDatosDemo, rutinas.EsDemo);

            Preferences.Set(AppInitState.EjerciciosOrigenDatos, ejercicios.Origen);

            Preferences.Set(AppInitState.RutinasOrigenDatos, rutinas.Origen);

            Preferences.Set(AppInitState.EjerciciosCargadoExitoso, ejercicios.CargadoExitoso);

            Preferences.Set(AppInitState.RutinasCargadoExitoso, rutinas.CargadoExitoso);
            Preferences.Set(AppInitState.ImagenesRutinasCargadas, imagenesRutinas);

            Debug.WriteLine("✅ Estado de inicialización persistido");
        }
        public static AppInitStateSnapshot LeerEstadoInicial()
        {
            return new AppInitStateSnapshot
            {
                PrimerArranque =
                Preferences.Get(AppInitState.PrimerArranque, true),

                EjerciciosCargadoExitoso =
                Preferences.Get(AppInitState.EjerciciosCargadoExitoso, false),

                RutinasCargadoExitoso =
                Preferences.Get(AppInitState.RutinasCargadoExitoso, false),

                EjerciciosUsaDatosDemo =
                Preferences.Get(AppInitState.EjerciciosUsaDatosDemo, false),

                RutinasUsaDatosDemo =
                Preferences.Get(AppInitState.RutinasUsaDatosDemo, false),

                EjerciciosOrigenDatos =
                Preferences.Get(AppInitState.EjerciciosOrigenDatos, ""),

                RutinasOrigenDatos =
                Preferences.Get(AppInitState.RutinasOrigenDatos, ""),

                ImagenesRutinasCargadas =
                Preferences.Get(AppInitState.ImagenesRutinasCargadas, false)
            };
        }
    }    
    
}

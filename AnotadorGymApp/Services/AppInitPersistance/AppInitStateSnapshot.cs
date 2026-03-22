using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Services.AppInitPersistance
{
    public sealed class AppInitStateSnapshot
    {
        public bool PrimerArranque { get; init; }

        public bool EjerciciosCargadoExitoso { get; init; }
        public bool RutinasCargadoExitoso { get; init; }

        public bool EjerciciosUsaDatosDemo { get; init; }
        public bool RutinasUsaDatosDemo { get; init; }

        public string EjerciciosOrigenDatos { get; init; } = "";
        public string RutinasOrigenDatos { get; init; } = "";

        public bool ImagenesRutinasCargadas { get; init; }
    }
}

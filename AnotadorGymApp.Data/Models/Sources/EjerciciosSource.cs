using AnotadorGymApp.Data.Models.DTOs.Ejercicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Data.Models.Sources
{
    public class EjerciciosSource
    {
        public List<EjercicioDTO> Datos { get; init; } = [];
        public bool EsDemo { get; init; }
        public string Origen { get; init; } = "";
        public bool CargadoExitoso { get; set; } = false;
    }
}

using AnotadorGymApp.Data.Models.DTOs;
using AnotadorGymApp.Data.Models.DTOs.Rutina;
using AnotadorGymApp.Data.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Data.Models.Sources
{
    public class RutinasSource
    {
        public List<RutinaDto> Datos { get; init; } = [];
        public bool EsDemo { get; init; }
        public string Origen { get; init; } = "";
        public bool CargadoExitoso { get; set; } = false;
    }
}

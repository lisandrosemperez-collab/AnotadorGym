using AnotadorGymApp.Data.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Data.Models.DTOs.Ejercicios
{
    public class EjercicioDTO
    {         
        public string Nombre { get; set; }        
        public int EjercicioId { get; set; }
        public GrupoMuscularDTO? GrupoMuscular { get; set; }
        public MusculoDTO? MusculoPrimario { get; set; }
        public List<MusculoDTO> MusculosSecundarios { get; set; } = new List<MusculoDTO>();
        public string? Descripcion { get; set; }
    }        
}

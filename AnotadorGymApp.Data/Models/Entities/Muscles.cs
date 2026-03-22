using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Data.Models.Entities
{        
    public class GrupoMuscular
    {
        public ICollection<Ejercicio> Ejercicios { get; set; } = new List<Ejercicio>();
        public int GrupoMuscularId { get; set; }
        public string Nombre { get; set; }
        public GrupoMuscular() { }
        public GrupoMuscular (string nombre)
        {
            Nombre = nombre;
        }        
    }
    public class Musculo
    {        
        public ICollection<Ejercicio> EjerciciosSecundarios { get; set; } = new List<Ejercicio>();
        public ICollection<Ejercicio> EjerciciosPrimarios { get; set; } = new List<Ejercicio>();
        #region Propiedades
        public int MusculoId { get; set; }
        public string Nombre { get; set; }        
        public Musculo(string nombre)
        {
            Nombre = nombre;
        }
        public Musculo() { } 
        #endregion
    }    
}

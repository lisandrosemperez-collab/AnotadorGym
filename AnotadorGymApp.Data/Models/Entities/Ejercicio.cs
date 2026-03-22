using AnotadorGymApp.Data.DataBase.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Data.Models.Entities
{
    
    public class Ejercicio : INotifyPropertyChanged
    {
        #region EF                
        public ICollection<Musculo> MusculosSecundarios { get; set; } = new List<Musculo>();
        public ICollection<RegistroEjercicio> RegistrosEjercicio { get; set; } = new List<RegistroEjercicio>();
        public ICollection<RutinaEjercicio> RutinasEjercicios { get; set; } = new List<RutinaEjercicio>();
        public Musculo MusculoPrimario { get; set; }
        public int MusculoPrimarioId { get; set; }
        public GrupoMuscular GrupoMuscular { get; set; }
        public int GrupoMuscularId { get; set; }             
        public Ejercicio() { }
        #endregion
        
        #region Propiedades
        
        private double? mejor;

        private double? ultimo;

        private double? iniciar;

        private string nombre;

        private int ejercicioId;
        private string descripcion;
        public int EjercicioId
        {
            get { return ejercicioId; }
            set
            {
                if (ejercicioId != value)
                {
                    ejercicioId = value;
                    OnPropertyChanged(nameof(EjercicioId));
                }
            } 
        }        
        public string Nombre
        {
            get { return nombre; }
            set
            {
                if (nombre != value)
                {
                    nombre = value;
                    OnPropertyChanged(nameof(Nombre));
                }
            }
        }
        public double? Mejor
        {
            get { return mejor; }
            set
            {
                if (mejor != value)
                {
                    mejor = value;
                    OnPropertyChanged(nameof(Mejor));                    
                }
            }
        }
        public double? Iniciar
        {
            get { return iniciar; }
            set
            {
                if (iniciar != value)
                {
                    iniciar = value;
                    OnPropertyChanged(nameof(Iniciar));                    
                }
            }
        }
        public double? Ultimo
        {
            get { return ultimo; }
            set
            {
                if (ultimo != value)
                {
                    ultimo = value;
                    OnPropertyChanged(nameof(Ultimo));                    
                }
            }
        }
        public string? Descripcion
        { 
            get { return descripcion; } 
            set 
            { 
                if (descripcion != value)
                { descripcion = value; OnPropertyChanged(nameof(Descripcion)); } 
            } 
        }
        #endregion

        #region UI
        [NotMapped]
        public double? MejorPeso => RegistrosEjercicio?.Select(log => log.PesoMaximo).DefaultIfEmpty(0).Max() ?? 0;
        [NotMapped]
        public double? VolumenDeHoy => RegistrosEjercicio?.FirstOrDefault(log => log.DiaEntrenamiento.Fecha == DateTime.Now.Date)?.VolumenTotal ?? 0;                

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class DiaEntrenamiento : INotifyPropertyChanged
    {
        [NotMapped]
        public double VolumenTotal => RegistroEjercicios.Sum(e => e.VolumenTotal);
        [NotMapped]
        public int EjerciciosTotal => RegistroEjercicios.Count();
        [NotMapped]
        public int? SeriesTotal => RegistroEjercicios.Sum(e => e.TotalSeries);
        public int DiaEntrenamientoId { get ; set ; } 
        public DateTime Fecha { get; set; }
        public ObservableCollection<RegistroEjercicio> RegistroEjercicios { get; set; } = new ObservableCollection<RegistroEjercicio>();

        private double volumen;                                
        public double Volumen { get { return volumen; } set { volumen = value; OnPropertyChanged(nameof(Volumen)); } }

        #region Observable
        public DiaEntrenamiento()
        {
            RegistroEjercicios.CollectionChanged += RegistroEjercicios_CollectionChanged;

            foreach (var e in RegistroEjercicios)
                e.PropertyChanged += Ejercicio_PropertyChanged;
        }
        private void RegistroEjercicios_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (RegistroEjercicio ex in e.NewItems)
                    ex.PropertyChanged += Ejercicio_PropertyChanged;

            if (e.OldItems != null)
                foreach (RegistroEjercicio ex in e.OldItems)
                    ex.PropertyChanged -= Ejercicio_PropertyChanged;

            NotificarCambios();
        }

        private void Ejercicio_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RegistroEjercicio.VolumenTotal) ||
                e.PropertyName == nameof(RegistroEjercicio.TotalSeries) || e.PropertyName == nameof(RegistroEjercicio.PesoMaximo))
            {
                NotificarCambios();
            }
        }

        private void NotificarCambios()
        {
            OnPropertyChanged(nameof(VolumenTotal));
            OnPropertyChanged(nameof(EjerciciosTotal));
            OnPropertyChanged(nameof(SeriesTotal));
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
    public class RegistroEjercicio : INotifyPropertyChanged
    {
        [NotMapped]
        public double PesoMaximo => RegistroSeries
                                        .Where(s => s.Tipo == TipoSerie.Normal || s.Tipo == TipoSerie.Max_Rm)
                                        .Select(s => s.Kilos)
                                        .DefaultIfEmpty(0)
                                        .Max();
        [NotMapped]
        public double VolumenTotal => RegistroSeries.Sum(s => s.Kilos * s.Reps);
        [NotMapped]
        public int? TotalSeries => RegistroSeries?.Count();

        public int RegistroEjercicioId { get; set; }
        public int DiaEntrenamientoId { get; set; }
        public DiaEntrenamiento DiaEntrenamiento { get; set; }
        public int EjercicioId { get; set; }
        public Ejercicio Ejercicio { get; set; }
        public ObservableCollection<RegistroSerie> RegistroSeries { get; set; } = new ObservableCollection<RegistroSerie>();

        #region Observable
        public RegistroEjercicio()
        {
            RegistroSeries.CollectionChanged += RegistroSeries_CollectionChanged;

            foreach (var s in RegistroSeries)
                s.PropertyChanged += Serie_PropertyChanged;
        }

        private void RegistroSeries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (RegistroSerie s in e.NewItems)
                    s.PropertyChanged += Serie_PropertyChanged;

            if (e.OldItems != null)
                foreach (RegistroSerie s in e.OldItems)
                    s.PropertyChanged -= Serie_PropertyChanged;

            NotificarCambios();
        }
        private void Serie_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RegistroSerie.Kilos) ||
                e.PropertyName == nameof(RegistroSerie.Reps) ||
                e.PropertyName == nameof(RegistroSerie.Tipo))
            {
                NotificarCambios();
            }
        }
        private void NotificarCambios()
        {
            OnPropertyChanged(nameof(PesoMaximo));
            OnPropertyChanged(nameof(VolumenTotal));
            OnPropertyChanged(nameof(TotalSeries));
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
    public class RegistroSerie : INotifyPropertyChanged
    {
        #region Entity
        public RegistroSerie() { }  
        public int RegistroSerieId { get; set; }
        public int RegistroEjercicioId { get; set; }
        public RegistroEjercicio RegistroEjercicio { get; set; }
        #endregion

        #region Propiededes

        private double kilos;
        private int reps;                   
        public double Kilos { get { return kilos; } set { kilos = value; OnPropertyChanged(nameof(Kilos)); } }
        public int Reps { get { return reps; } set { reps = value; OnPropertyChanged(nameof(Reps)); } }
        private TipoSerie tipo;
        public TipoSerie Tipo
        {
            get => tipo;
            set
            {
                if (tipo != value)
                {
                    tipo = value;
                    OnPropertyChanged(nameof(Tipo));
                    Debug.WriteLine(Tipo);
                }
            }
        }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
}


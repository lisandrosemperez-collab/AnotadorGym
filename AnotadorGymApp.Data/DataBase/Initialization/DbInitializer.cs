using AnotadorGymApp.Data.DataBase.Services;
using AnotadorGymApp.Data.Initialization.Importers;
using AnotadorGymApp.Data.Initialization.Importers.Abstractions;
using AnotadorGymApp.Data.Models.Entities;
using AnotadorGymApp.Data.Models.Sources;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Data.DataBase.Initialization
{
    public class DbInitializer
    {
        private readonly IDbContextFactory<DataBaseContext> _dbFactory;
        private readonly IDataImporter<EjerciciosSource> _ejercicioImporter;
        private readonly IDataImporter<RutinasSource> _rutinaImporter;
        public DbInitializer(IDbContextFactory<DataBaseContext> dbFactory,
        IDataImporter<EjerciciosSource> ejercicioImporter,
        IDataImporter<RutinasSource> rutinaImporter)
        {
            _dbFactory = dbFactory;
            _ejercicioImporter = ejercicioImporter;
            _rutinaImporter = rutinaImporter;
        }

        public async Task InitializeAsync(
        bool primerArranque,        
        IProgress<double> progress,
        EjerciciosSource ejerciciosSource, 
        RutinasSource rutinasSource,
        List<DiaEntrenamiento> diasEntrenamientoPrueba,
        CancellationToken token)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(token);
            
            await db.Database.MigrateAsync(token);                       

            if (ejerciciosSource != null && ejerciciosSource.Datos.Count > 0)
            {
                await _ejercicioImporter.ImportarAsync(db, ejerciciosSource, progress, token);
            }
                
            if (rutinasSource != null && rutinasSource.Datos.Count > 0)
            {
                await _rutinaImporter.ImportarAsync(db, rutinasSource, progress, token);
            }
            
#if DEBUG
                await new DebugDiaEntrenamientosPrueba().ImportAsync(diasEntrenamientoPrueba,db);
#endif            
        }
    }
}

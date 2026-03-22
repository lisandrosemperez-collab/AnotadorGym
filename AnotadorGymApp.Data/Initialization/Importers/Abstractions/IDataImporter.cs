using AnotadorGymApp.Data.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Data.Initialization.Importers.Abstractions
{
    public interface IDataImporter<in TSource>
    {
        Task ImportarAsync(
        DataBaseContext db,
        TSource source,
        IProgress<double>? progress,
        CancellationToken token);
    }
}

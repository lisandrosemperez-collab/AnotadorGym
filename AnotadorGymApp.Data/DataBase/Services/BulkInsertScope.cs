using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Data.DataBase.Services
{
    public class BulkInsertScope : IAsyncDisposable
    {
        private readonly DataBaseContext _db;
        private readonly DbConnection _connection;
        private readonly IDbContextTransaction _transaction;
        private bool _completed;

        public BulkInsertScope(DataBaseContext db)
        {
            _db = db;
            _connection = db.Database.GetDbConnection();

            if (_connection.State != System.Data.ConnectionState.Open)
                _connection.Open();

            AplicarPragmasAsync().GetAwaiter().GetResult();

            _transaction = db.Database.BeginTransaction();
        }

        private async Task AplicarPragmasAsync()
        {
            try
            {
                Debug.WriteLine("⚙️ Activando optimizaciones SQLite...");

                // Aplicar optimizaciones que funcionan fuera de transacciones
                await _db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = WAL");
                await _db.Database.ExecuteSqlRawAsync("PRAGMA synchronous = NORMAL");
                await _db.Database.ExecuteSqlRawAsync("PRAGMA cache_size = 10000");
                await _db.Database.ExecuteSqlRawAsync("PRAGMA temp_store = MEMORY");

                Debug.WriteLine("✅ SQLite optimizado para inserción masiva");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ No se pudo optimizar SQLite: {ex.Message}");
            }
        }        
        private async Task RestaurarPragmasAsync()
        {
            try
            {
                Debug.WriteLine("⚙️ Restaurando configuración normal de SQLite...");

                // Esperar un momento para asegurar que no hay transacción activa
                await Task.Delay(50);

                await _db.Database.ExecuteSqlRawAsync("PRAGMA synchronous = FULL");
                await _db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = DELETE");
                await _db.Database.ExecuteSqlRawAsync("PRAGMA cache_size = -2000");
                await _db.Database.ExecuteSqlRawAsync("PRAGMA temp_store = DEFAULT");

                Debug.WriteLine("✅ Configuración SQLite restaurada");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Error al restaurar configuración: {ex.Message}");
            }
        }
        public async Task<bool> CommitAsync()
        {
            try
            {
                await _db.SaveChangesAsync();
                await _transaction.CommitAsync();
                _completed = true;
                return _completed;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Error al confirmar transacción: {ex.Message}");
                _completed = false;
                return _completed;                
            }
        }
        public async ValueTask DisposeAsync()
        {                        
            if (!_completed)
            {
                await _transaction.RollbackAsync();
            }                        
            await RestaurarPragmasAsync();
            await _transaction.DisposeAsync();
        }

    }
}

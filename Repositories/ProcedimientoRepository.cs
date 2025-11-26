using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Ordenes.Api.Models;
using Ordenes.Api.Settings;

namespace Ordenes.API.Repositories
{
    public class ProcedimientoRepository
    {
        private readonly IMongoCollection<Procedimiento> _procedimientosCollection;

        public ProcedimientoRepository(IMongoClient mongoClient, MongoDbSettings settings)
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _procedimientosCollection = database.GetCollection<Procedimiento>(settings.ProcedimientosCollectionName);

            CrearIndices();
        }

        private void CrearIndices()
        {
            try
            {
                var indexKeys = Builders<Procedimiento>.IndexKeys
                    .Ascending(p => p.NumeroOrden)
                    .Ascending(p => p.NumeroItem);

                var indexOptions = new CreateIndexOptions { Unique = true };
                var indexModel = new CreateIndexModel<Procedimiento>(indexKeys, indexOptions);

                _procedimientosCollection.Indexes.CreateOne(indexModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear índices para procedimientos: {ex.Message}");
            }
        }

        public async Task<List<Procedimiento>> GetAllAsync() =>
            await _procedimientosCollection.Find(_ => true).ToListAsync();

        public async Task<List<Procedimiento>> GetByNumeroOrdenAsync(int numeroOrden) =>
            await _procedimientosCollection.Find(p => p.NumeroOrden == numeroOrden).ToListAsync();

        public async Task<Procedimiento?> GetByNumeroOrdenItemAsync(int numeroOrden, int numeroItem) =>
            await _procedimientosCollection
                .Find(p => p.NumeroOrden == numeroOrden && p.NumeroItem == numeroItem)
                .FirstOrDefaultAsync();

        public async Task CreateAsync(Procedimiento procedimiento) =>
            await _procedimientosCollection.InsertOneAsync(procedimiento);

        public async Task UpdateAsync(int numeroOrden, int numeroItem, Procedimiento procedimientoActualizado) =>
            await _procedimientosCollection
                .ReplaceOneAsync(p => p.NumeroOrden == numeroOrden && p.NumeroItem == numeroItem, procedimientoActualizado);

        public async Task DeleteAsync(int numeroOrden, int numeroItem) =>
            await _procedimientosCollection.DeleteOneAsync(p => p.NumeroOrden == numeroOrden && p.NumeroItem == numeroItem);

        public async Task DeleteByOrdenAsync(int numeroOrden) =>
            await _procedimientosCollection.DeleteManyAsync(p => p.NumeroOrden == numeroOrden);
    }
}

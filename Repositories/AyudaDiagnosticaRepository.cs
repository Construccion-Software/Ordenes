using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Ordenes.Api.Models;
using Ordenes.Api.Settings;

namespace Ordenes.API.Repositories
{
    public class AyudaDiagnosticaRepository
    {
        private readonly IMongoCollection<AyudaDiagnostica> _ayudasDiagnosticasCollection;

        public AyudaDiagnosticaRepository(IMongoClient mongoClient, MongoDbSettings settings)
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _ayudasDiagnosticasCollection = database.GetCollection<AyudaDiagnostica>(settings.AyudasDiagnosticasCollectionName);

            CrearIndices();
        }

        private void CrearIndices()
        {
            try
            {
                var indexKeys = Builders<AyudaDiagnostica>.IndexKeys
                    .Ascending(a => a.NumeroOrden)
                    .Ascending(a => a.NumeroItem);

                var indexOptions = new CreateIndexOptions { Unique = true };
                var indexModel = new CreateIndexModel<AyudaDiagnostica>(indexKeys, indexOptions);

                _ayudasDiagnosticasCollection.Indexes.CreateOne(indexModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear índices para ayudas diagnósticas: {ex.Message}");
            }
        }

        public async Task<List<AyudaDiagnostica>> GetAllAsync() =>
            await _ayudasDiagnosticasCollection.Find(_ => true).ToListAsync();

        public async Task<List<AyudaDiagnostica>> GetByNumeroOrdenAsync(int numeroOrden) =>
            await _ayudasDiagnosticasCollection.Find(a => a.NumeroOrden == numeroOrden).ToListAsync();

        public async Task<AyudaDiagnostica?> GetByNumeroOrdenItemAsync(int numeroOrden, int numeroItem) =>
            await _ayudasDiagnosticasCollection
                .Find(a => a.NumeroOrden == numeroOrden && a.NumeroItem == numeroItem)
                .FirstOrDefaultAsync();

        public async Task CreateAsync(AyudaDiagnostica ayudaDiagnostica) =>
            await _ayudasDiagnosticasCollection.InsertOneAsync(ayudaDiagnostica);

        public async Task UpdateAsync(int numeroOrden, int numeroItem, AyudaDiagnostica ayudaDiagnosticaActualizada) =>
            await _ayudasDiagnosticasCollection
                .ReplaceOneAsync(a => a.NumeroOrden == numeroOrden && a.NumeroItem == numeroItem, ayudaDiagnosticaActualizada);

        public async Task DeleteAsync(int numeroOrden, int numeroItem) =>
            await _ayudasDiagnosticasCollection.DeleteOneAsync(a => a.NumeroOrden == numeroOrden && a.NumeroItem == numeroItem);

        public async Task DeleteByOrdenAsync(int numeroOrden) =>
            await _ayudasDiagnosticasCollection.DeleteManyAsync(a => a.NumeroOrden == numeroOrden);
    }
}

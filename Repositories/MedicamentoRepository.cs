using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Ordenes.Api.Models;
using Ordenes.Api.Settings;

namespace Ordenes.API.Repositories
{
    public class MedicamentoRepository
    {
        private readonly IMongoCollection<Medicamento> _medicamentosCollection;

        public MedicamentoRepository(IMongoClient mongoClient, MongoDbSettings settings)
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _medicamentosCollection = database.GetCollection<Medicamento>(settings.MedicamentosCollectionName);

            CrearIndices();
        }

        private void CrearIndices()
        {
            try
            {
                var indexKeys = Builders<Medicamento>.IndexKeys
                    .Ascending(m => m.NumeroOrden)
                    .Ascending(m => m.NumeroItem);

                var indexOptions = new CreateIndexOptions { Unique = true };
                var indexModel = new CreateIndexModel<Medicamento>(indexKeys, indexOptions);

                _medicamentosCollection.Indexes.CreateOne(indexModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear índices para medicamentos: {ex.Message}");
            }
        }

        public async Task<List<Medicamento>> GetAllAsync() =>
            await _medicamentosCollection.Find(_ => true).ToListAsync();

        public async Task<List<Medicamento>> GetByNumeroOrdenAsync(int numeroOrden) =>
            await _medicamentosCollection.Find(m => m.NumeroOrden == numeroOrden).ToListAsync();

        public async Task<Medicamento?> GetByNumeroOrdenItemAsync(int numeroOrden, int numeroItem) =>
            await _medicamentosCollection
                .Find(m => m.NumeroOrden == numeroOrden && m.NumeroItem == numeroItem)
                .FirstOrDefaultAsync();

        public async Task CreateAsync(Medicamento medicamento) =>
            await _medicamentosCollection.InsertOneAsync(medicamento);

        public async Task UpdateAsync(int numeroOrden, int numeroItem, Medicamento medicamentoActualizado) =>
            await _medicamentosCollection
                .ReplaceOneAsync(m => m.NumeroOrden == numeroOrden && m.NumeroItem == numeroItem, medicamentoActualizado);

        public async Task DeleteAsync(int numeroOrden, int numeroItem) =>
            await _medicamentosCollection.DeleteOneAsync(m => m.NumeroOrden == numeroOrden && m.NumeroItem == numeroItem);

        public async Task DeleteByOrdenAsync(int numeroOrden) =>
            await _medicamentosCollection.DeleteManyAsync(m => m.NumeroOrden == numeroOrden);
    }
}

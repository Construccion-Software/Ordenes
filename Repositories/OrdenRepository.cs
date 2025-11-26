using MongoDB.Driver;
using Ordenes.Api.Models;
using Ordenes.Api.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ordenes.API.Repositories
{
    public class OrdenRepository
    {
        private readonly IMongoCollection<Orden> _ordenesCollection;

        public OrdenRepository(IMongoClient mongoClient, MongoDbSettings settings)
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _ordenesCollection = database.GetCollection<Orden>(settings.OrdenesCollectionName);

            CrearIndices();
        }

        private void CrearIndices()
        {
            try
            {
                var indexKeysDefinition = Builders<Orden>.IndexKeys
                    .Ascending(o => o.NumeroOrden); 
                var indexOptions = new CreateIndexOptions { Unique = true };
                var indexModel = new CreateIndexModel<Orden>(indexKeysDefinition, indexOptions);

                _ordenesCollection.Indexes.CreateOne(indexModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear índices: {ex.Message}");
             
            }
        }

        public async Task<List<Orden>> GetAllAsync() =>
            await _ordenesCollection.Find(_ => true).ToListAsync();

        public async Task<Orden> GetByNumeroOrdenAsync(int numeroOrden) =>
            await _ordenesCollection.Find(o => o.NumeroOrden == numeroOrden).FirstOrDefaultAsync();

        public async Task<List<Orden>> GetByPacienteAsync(string cedulaPaciente) =>
            await _ordenesCollection.Find(o => o.CedulaPaciente == cedulaPaciente).ToListAsync();

        public async Task CreateAsync(Orden orden) =>
            await _ordenesCollection.InsertOneAsync(orden);

        public async Task UpdateAsync(int numeroOrden, Orden nuevaOrden) =>
            await _ordenesCollection.ReplaceOneAsync(o => o.NumeroOrden == numeroOrden, nuevaOrden);

        public async Task DeleteAsync(int numeroOrden) =>
            await _ordenesCollection.DeleteOneAsync(o => o.NumeroOrden == numeroOrden);
    }
}

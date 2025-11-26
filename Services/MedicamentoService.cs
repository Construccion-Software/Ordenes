using System.Collections.Generic;
using System.Threading.Tasks;
using Ordenes.API.Repositories;
using Ordenes.Api.Models;

namespace Ordenes.Api.Services
{
    public class MedicamentoService
    {
        private readonly MedicamentoRepository _medicamentoRepository;

        public MedicamentoService(
            MedicamentoRepository medicamentoRepository)
        {
            _medicamentoRepository = medicamentoRepository;
        }

        public Task<List<Medicamento>> GetAllAsync() =>
            _medicamentoRepository.GetAllAsync();

        public Task<List<Medicamento>> GetByNumeroOrdenAsync(int numeroOrden) =>
            _medicamentoRepository.GetByNumeroOrdenAsync(numeroOrden);

        public Task<Medicamento?> GetByNumeroOrdenItemAsync(int numeroOrden, int numeroItem) =>
            _medicamentoRepository.GetByNumeroOrdenItemAsync(numeroOrden, numeroItem);
    }
}

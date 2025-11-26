using System.Collections.Generic;
using System.Threading.Tasks;
using Ordenes.API.Repositories;
using Ordenes.Api.Models;

namespace Ordenes.Api.Services
{
    public class ProcedimientoService
    {
        private readonly ProcedimientoRepository _procedimientoRepository;

        public ProcedimientoService(
            ProcedimientoRepository procedimientoRepository)
        {
            _procedimientoRepository = procedimientoRepository;
        }

        public Task<List<Procedimiento>> GetAllAsync() =>
            _procedimientoRepository.GetAllAsync();

        public Task<List<Procedimiento>> GetByNumeroOrdenAsync(int numeroOrden) =>
            _procedimientoRepository.GetByNumeroOrdenAsync(numeroOrden);

        public Task<Procedimiento?> GetByNumeroOrdenItemAsync(int numeroOrden, int numeroItem) =>
            _procedimientoRepository.GetByNumeroOrdenItemAsync(numeroOrden, numeroItem);
    }
}

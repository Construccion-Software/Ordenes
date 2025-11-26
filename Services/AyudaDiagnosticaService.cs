using System.Collections.Generic;
using System.Threading.Tasks;
using Ordenes.API.Repositories;
using Ordenes.Api.Models;

namespace Ordenes.Api.Services
{
    public class AyudaDiagnosticaService
    {
        private readonly AyudaDiagnosticaRepository _ayudaDiagnosticaRepository;
        public AyudaDiagnosticaService(
            AyudaDiagnosticaRepository ayudaDiagnosticaRepository)
        {
            _ayudaDiagnosticaRepository = ayudaDiagnosticaRepository;
        }

        public Task<List<AyudaDiagnostica>> GetAllAsync() =>
            _ayudaDiagnosticaRepository.GetAllAsync();

        public Task<List<AyudaDiagnostica>> GetByNumeroOrdenAsync(int numeroOrden) =>
            _ayudaDiagnosticaRepository.GetByNumeroOrdenAsync(numeroOrden);

        public Task<AyudaDiagnostica?> GetByNumeroOrdenItemAsync(int numeroOrden, int numeroItem) =>
            _ayudaDiagnosticaRepository.GetByNumeroOrdenItemAsync(numeroOrden, numeroItem);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordenes.Api.BusinessRules
{
    public class OrdenValidator
    {
        private readonly IReadOnlyCollection<IValidationRule> _validationRules;

        public OrdenValidator(IEnumerable<IValidationRule> validationRules)
        {
            _validationRules = (validationRules ?? Array.Empty<IValidationRule>()).ToList().AsReadOnly();
        }

        public async Task<Dictionary<string, List<string>>> ValidarAsync(OrdenValidacionContext contexto)
        {
            if (contexto == null)
            {
                throw new ArgumentNullException(nameof(contexto));
            }

            var errores = new Dictionary<string, List<string>>();

            foreach (var regla in _validationRules)
            {
                var resultado = await regla.ValidarAsync(contexto).ConfigureAwait(false);
                foreach (var kvp in resultado)
                {
                    if (!errores.TryGetValue(kvp.Key, out var lista))
                    {
                        lista = new List<string>();
                        errores[kvp.Key] = lista;
                    }

                    lista.AddRange(kvp.Value);
                }
            }

            return errores;
        }

        public async Task<Dictionary<string, List<string>>> ValidarReglaAsync(OrdenValidacionContext contexto, string nombreRegla)
        {
            if (contexto == null)
            {
                throw new ArgumentNullException(nameof(contexto));
            }

            if (string.IsNullOrWhiteSpace(nombreRegla))
            {
                return new Dictionary<string, List<string>>();
            }

            var regla = _validationRules.FirstOrDefault(r => string.Equals(r.NombreRegla, nombreRegla, StringComparison.OrdinalIgnoreCase));
            if (regla == null)
            {
                return new Dictionary<string, List<string>>();
            }

            return await regla.ValidarAsync(contexto).ConfigureAwait(false);
        }

        public IReadOnlyCollection<string> ObtenerNombresReglas() =>
            _validationRules.Select(r => r.NombreRegla).ToList().AsReadOnly();
    }
}

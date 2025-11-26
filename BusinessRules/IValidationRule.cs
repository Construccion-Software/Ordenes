using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ordenes.Api.BusinessRules
{
    public interface IValidationRule
    {
        string NombreRegla { get; }

        Task<Dictionary<string, List<string>>> ValidarAsync(OrdenValidacionContext contexto);
    }
}

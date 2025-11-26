using System;
using System.Collections.Generic;

namespace Ordenes.Api.BusinessRules
{
    public class BusinessRuleValidationException : Exception
    {
        public BusinessRuleValidationException(Dictionary<string, List<string>> errores)
            : base("Se encontraron incumplimientos de reglas de negocio para la orden solicitada.")
        {
            Errores = errores ?? new Dictionary<string, List<string>>();
        }

        public Dictionary<string, List<string>> Errores { get; }
    }
}

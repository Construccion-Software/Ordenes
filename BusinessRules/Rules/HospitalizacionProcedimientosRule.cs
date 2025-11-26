using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordenes.Api.BusinessRules.Rules
{
    public sealed class HospitalizacionProcedimientosRule : IValidationRule
    {
        public string NombreRegla => "Hospitalización como procedimiento";

        public Task<Dictionary<string, List<string>>> ValidarAsync(OrdenValidacionContext contexto)
        {
            var errores = new Dictionary<string, List<string>>();
            var procedimientos = contexto.Procedimientos.ToList();

            if (!procedimientos.Any())
            {
                return Task.FromResult(errores);
            }

            var numeroOrden = contexto.Orden.NumeroOrden;
            var procedimientosHospitalizacion = procedimientos
                .Where(p => EsCatalogoHospitalizacion(p.CatalogoId))
                .ToList();

            if (!procedimientosHospitalizacion.Any())
            {
                return Task.FromResult(errores);
            }

            if (procedimientos.Count < 2)
            {
                AgregarError(errores, "procedimientos", $"La orden {numeroOrden} incluye una hospitalización, pero no detalla procedimientos adicionales (por ejemplo, visitas de enfermería). Añade los procedimientos requeridos.");
            }

            if (!contexto.Medicamentos.Any())
            {
                AgregarError(errores, "medicamentos", $"La orden {numeroOrden} con hospitalización debe registrar los medicamentos y sus indicaciones para la estancia.");
            }

            return Task.FromResult(errores);
        }

        private static bool EsCatalogoHospitalizacion(string catalogoId)
        {
            var normalizado = Normalizar(catalogoId);
            return normalizado.Contains("hospital");
        }

        private static string Normalizar(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return string.Empty;
            }

            var formD = valor.Normalize(NormalizationForm.FormD);
            var chars = formD.Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark);
            return new string(chars.ToArray()).ToLowerInvariant();
        }

        private static void AgregarError(Dictionary<string, List<string>> errores, string clave, string mensaje)
        {
            if (!errores.TryGetValue(clave, out var lista))
            {
                lista = new List<string>();
                errores[clave] = lista;
            }

            lista.Add(mensaje);
        }
    }
}

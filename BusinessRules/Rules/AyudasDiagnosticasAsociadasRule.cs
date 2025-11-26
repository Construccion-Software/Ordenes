using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordenes.Api.BusinessRules.Rules
{
    public sealed class AyudasDiagnosticasAsociadasRule : IValidationRule
    {
        public string NombreRegla => "Ayudas diagnósticas asociadas a la orden";

        public Task<Dictionary<string, List<string>>> ValidarAsync(OrdenValidacionContext contexto)
        {
            var errores = new Dictionary<string, List<string>>();
            var ayudas = contexto.AyudasDiagnosticas.ToList();

            if (!ayudas.Any())
            {
                return Task.FromResult(errores);
            }

            var numeroOrden = contexto.Orden.NumeroOrden;

            foreach (var ayuda in ayudas.Where(a => a.NumeroOrden != numeroOrden))
            {
                AgregarError(errores, "ayudasDiagnosticas", $"La ayuda diagnóstica {FormatearCatalogo(ayuda.CatalogoId)} indica el número de orden {ayuda.NumeroOrden}, pero la orden actual es {numeroOrden}.");
            }

            var duplicados = ayudas.GroupBy(a => a.NumeroItem)
                .Where(g => g.Key > 0 && g.Count() > 1);

            foreach (var grupo in duplicados)
            {
                var detalle = string.Join(", ", grupo.Select(g => FormatearCatalogo(g.CatalogoId)));
                AgregarError(errores, "ayudasDiagnosticas", $"Las ayudas diagnósticas de la orden {numeroOrden} tienen el ítem {grupo.Key} repetido: {detalle}.");
            }

            if (!contexto.Medicamentos.Any() && !contexto.Procedimientos.Any())
            {
                var ordenadas = ayudas.OrderBy(a => a.NumeroItem).ToList();
                if (ordenadas.First().NumeroItem != 1)
                {
                    AgregarError(errores, "ayudasDiagnosticas", $"Las ayudas diagnósticas de la orden {numeroOrden} deben iniciar en el ítem 1.");
                }

                for (var i = 0; i < ordenadas.Count; i++)
                {
                    var esperado = i + 1;
                    var actual = ordenadas[i].NumeroItem;
                    if (actual != esperado)
                    {
                        AgregarError(errores, "ayudasDiagnosticas", $"La numeración de ayudas diagnósticas para la orden {numeroOrden} presenta un salto: se esperaba el ítem {esperado} y se encontró el {actual}.");
                        break;
                    }
                }
            }

            return Task.FromResult(errores);
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

        private static string FormatearCatalogo(string catalogoId) =>
            string.IsNullOrWhiteSpace(catalogoId)
                ? "sin catálogo asignado"
                : $"con catálogo '{catalogoId}'";
    }
}

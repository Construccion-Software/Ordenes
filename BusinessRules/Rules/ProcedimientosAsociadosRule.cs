using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordenes.Api.BusinessRules.Rules
{
    public sealed class ProcedimientosAsociadosRule : IValidationRule
    {
        public string NombreRegla => "Procedimientos asociados a la orden";

        public Task<Dictionary<string, List<string>>> ValidarAsync(OrdenValidacionContext contexto)
        {
            var errores = new Dictionary<string, List<string>>();
            var procedimientos = contexto.Procedimientos.ToList();

            if (!procedimientos.Any())
            {
                return Task.FromResult(errores);
            }

            var numeroOrden = contexto.Orden.NumeroOrden;

            foreach (var procedimiento in procedimientos.Where(p => p.NumeroOrden != numeroOrden))
            {
                AgregarError(errores, "procedimientos", $"El procedimiento {FormatearCatalogo(procedimiento.CatalogoId)} indica el número de orden {procedimiento.NumeroOrden}, pero la orden actual es {numeroOrden}.");
            }

            var duplicados = procedimientos.GroupBy(p => p.NumeroItem)
                .Where(g => g.Key > 0 && g.Count() > 1);

            foreach (var grupo in duplicados)
            {
                var detalle = string.Join(", ", grupo.Select(g => FormatearCatalogo(g.CatalogoId)));
                AgregarError(errores, "procedimientos", $"Los procedimientos de la orden {numeroOrden} tienen el ítem {grupo.Key} repetido: {detalle}.");
            }

            if (!contexto.Medicamentos.Any() && !contexto.AyudasDiagnosticas.Any())
            {
                var ordenados = procedimientos.OrderBy(p => p.NumeroItem).ToList();
                if (ordenados.First().NumeroItem != 1)
                {
                    AgregarError(errores, "procedimientos", $"Los procedimientos de la orden {numeroOrden} deben iniciar en el ítem 1.");
                }

                for (var i = 0; i < ordenados.Count; i++)
                {
                    var esperado = i + 1;
                    var actual = ordenados[i].NumeroItem;
                    if (actual != esperado)
                    {
                        AgregarError(errores, "procedimientos", $"La numeración de procedimientos para la orden {numeroOrden} presenta un salto: se esperaba el ítem {esperado} y se encontró el {actual}.");
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

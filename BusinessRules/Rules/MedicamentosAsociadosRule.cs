using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordenes.Api.BusinessRules.Rules
{
    public sealed class MedicamentosAsociadosRule : IValidationRule
    {
        public string NombreRegla => "Medicamentos asociados a la orden";

        public Task<Dictionary<string, List<string>>> ValidarAsync(OrdenValidacionContext contexto)
        {
            var errores = new Dictionary<string, List<string>>();
            var medicamentos = contexto.Medicamentos.ToList();

            if (!medicamentos.Any())
            {
                return Task.FromResult(errores);
            }

            var numeroOrden = contexto.Orden.NumeroOrden;

            foreach (var medicamento in medicamentos.Where(m => m.NumeroOrden != numeroOrden))
            {
                AgregarError(errores, "medicamentos", $"El medicamento {FormatearCatalogo(medicamento.CatalogoId)} indica el número de orden {medicamento.NumeroOrden}, pero la orden actual es {numeroOrden}.");
            }

            var medicamentosDuplicados = medicamentos.GroupBy(m => m.NumeroItem)
                .Where(g => g.Key > 0 && g.Count() > 1);

            foreach (var grupo in medicamentosDuplicados)
            {
                var detalle = string.Join(", ", grupo.Select(g => FormatearCatalogo(g.CatalogoId)));
                AgregarError(errores, "medicamentos", $"Los medicamentos de la orden {numeroOrden} tienen el ítem {grupo.Key} repetido: {detalle}.");
            }

            if (!contexto.Procedimientos.Any() && !contexto.AyudasDiagnosticas.Any())
            {
                var ordenados = medicamentos.OrderBy(m => m.NumeroItem).ToList();
                if (ordenados.First().NumeroItem != 1)
                {
                    AgregarError(errores, "medicamentos", $"Los medicamentos de la orden {numeroOrden} deben iniciar en el ítem 1.");
                }

                for (var i = 0; i < ordenados.Count; i++)
                {
                    var esperado = i + 1;
                    var actual = ordenados[i].NumeroItem;
                    if (actual != esperado)
                    {
                        AgregarError(errores, "medicamentos", $"La numeración de medicamentos para la orden {numeroOrden} presenta un salto: se esperaba el ítem {esperado} y se encontró el {actual}.");
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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordenes.Api.BusinessRules.Rules
{
    public sealed class ItemsUnicosEnOrdenRule : IValidationRule
    {
        public string NombreRegla => "Ítems únicos por orden";

        public Task<Dictionary<string, List<string>>> ValidarAsync(OrdenValidacionContext contexto)
        {
            var errores = new Dictionary<string, List<string>>();

            var todosLosItems = contexto.Medicamentos.Select(m => new ItemResumen("Medicamento", m.NumeroItem, m.CatalogoId))
                .Concat(contexto.Procedimientos.Select(p => new ItemResumen("Procedimiento", p.NumeroItem, p.CatalogoId)))
                .Concat(contexto.AyudasDiagnosticas.Select(a => new ItemResumen("Ayuda diagnóstica", a.NumeroItem, a.CatalogoId)))
                .ToList();

            if (!todosLosItems.Any())
            {
                return Task.FromResult(errores);
            }

            var numeroOrden = contexto.Orden.NumeroOrden;

            var numerosInvalidos = todosLosItems.Where(i => i.NumeroItem <= 0).ToList();
            foreach (var item in numerosInvalidos)
            {
                AgregarError(errores, "numeroItem", $"El {FormatearDetalle(item.Tipo, item.CatalogoId)} de la orden {numeroOrden} tiene número de ítem {item.NumeroItem}, el valor debe ser mayor o igual a 1.");
            }

            var duplicados = todosLosItems.GroupBy(i => i.NumeroItem)
                .Where(g => g.Key > 0 && g.Count() > 1)
                .ToList();

            foreach (var grupo in duplicados)
            {
                var detalleDuplicados = string.Join(", ", grupo.Select(g => FormatearDetalle(g.Tipo, g.CatalogoId)));
                AgregarError(errores, "numeroItem", $"La orden {numeroOrden} tiene el número de ítem {grupo.Key} repetido en: {detalleDuplicados}.");
            }

            var numerosOrdenados = todosLosItems.Select(i => i.NumeroItem)
                .Where(n => n > 0)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            if (numerosOrdenados.Count > 0 && numerosOrdenados.First() != 1)
            {
                AgregarError(errores, "numeroItem", $"La numeración de ítems de la orden {numeroOrden} debe iniciar en 1.");
            }

            for (var index = 0; index < numerosOrdenados.Count; index++)
            {
                var esperado = index + 1;
                var actual = numerosOrdenados[index];
                if (actual != esperado)
                {
                    AgregarError(errores, "numeroItem", $"La numeración de ítems de la orden {numeroOrden} presenta un salto: se esperaba el ítem {esperado} pero se encontró {actual}.");
                    break;
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

        private static string FormatearDetalle(string tipo, string catalogoId)
        {
            var etiqueta = string.IsNullOrWhiteSpace(catalogoId)
                ? "sin catálogo asignado"
                : $"con catálogo '{catalogoId}'";

            return $"{tipo.ToLower()} {etiqueta}";
        }

        private sealed record ItemResumen(string Tipo, int NumeroItem, string CatalogoId);
    }
}

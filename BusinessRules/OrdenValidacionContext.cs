using System;
using System.Collections.Generic;
using Ordenes.Api.Models;

namespace Ordenes.Api.BusinessRules
{
    public sealed class OrdenValidacionContext
    {
        public OrdenValidacionContext(
            Orden orden,
            IEnumerable<Medicamento> medicamentos,
            IEnumerable<Procedimiento> procedimientos,
            IEnumerable<AyudaDiagnostica> ayudasDiagnosticas)
        {
            Orden = orden ?? throw new ArgumentNullException(nameof(orden));
            Medicamentos = new List<Medicamento>(medicamentos ?? Array.Empty<Medicamento>()).AsReadOnly();
            Procedimientos = new List<Procedimiento>(procedimientos ?? Array.Empty<Procedimiento>()).AsReadOnly();
            AyudasDiagnosticas = new List<AyudaDiagnostica>(ayudasDiagnosticas ?? Array.Empty<AyudaDiagnostica>()).AsReadOnly();
        }

        public Orden Orden { get; }

        public IReadOnlyCollection<Medicamento> Medicamentos { get; }

        public IReadOnlyCollection<Procedimiento> Procedimientos { get; }

        public IReadOnlyCollection<AyudaDiagnostica> AyudasDiagnosticas { get; }
    }
}

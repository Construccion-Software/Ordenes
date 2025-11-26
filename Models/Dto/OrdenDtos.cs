using System;
using System.Collections.Generic;
using Ordenes.Api.Models.Enums;

namespace Ordenes.Api.Models.Dto
{
    public sealed class OrdenCreateRequest
    {
        public int NumeroOrden { get; set; }

        public string CedulaPaciente { get; set; } = string.Empty;

        public string CedulaMedico { get; set; } = string.Empty;

        public DateTime? FechaCreacion { get; set; }

        public EstadoOrden Estado { get; set; } = EstadoOrden.Pendiente;

        public List<OrdenMedicamentoDto> Medicamentos { get; set; } = new List<OrdenMedicamentoDto>();

        public List<OrdenProcedimientoDto> Procedimientos { get; set; } = new List<OrdenProcedimientoDto>();

        public List<OrdenAyudaDiagnosticaDto> AyudasDiagnosticas { get; set; } = new List<OrdenAyudaDiagnosticaDto>();
    }

    public sealed class OrdenDetalleDto
    {
        public int NumeroOrden { get; set; }

        public string CedulaPaciente { get; set; } = string.Empty;

        public string CedulaMedico { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        public EstadoOrden Estado { get; set; }

        public List<OrdenMedicamentoDto> Medicamentos { get; set; } = new List<OrdenMedicamentoDto>();

        public List<OrdenProcedimientoDto> Procedimientos { get; set; } = new List<OrdenProcedimientoDto>();

        public List<OrdenAyudaDiagnosticaDto> AyudasDiagnosticas { get; set; } = new List<OrdenAyudaDiagnosticaDto>();
    }

    public sealed class OrdenMedicamentoDto
    {
        public int NumeroItem { get; set; }

        public string CatalogoId { get; set; } = string.Empty;

        public string Dosis { get; set; } = string.Empty;

        public int DuracionTratamiento { get; set; }
    }

    public sealed class OrdenProcedimientoDto
    {
        public int NumeroItem { get; set; }

        public string CatalogoId { get; set; } = string.Empty;

        public int NumeroVecesRepite { get; set; }

        public string Frecuencia { get; set; } = string.Empty;
    }

    public sealed class OrdenAyudaDiagnosticaDto
    {
        public int NumeroItem { get; set; }

        public string CatalogoId { get; set; } = string.Empty;

        public int Cantidad { get; set; }
    }
}

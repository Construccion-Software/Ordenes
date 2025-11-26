using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using Ordenes.API.Repositories;
using Ordenes.Api.BusinessRules;
using Ordenes.Api.Models;
using Ordenes.Api.Models.Dto;

namespace Ordenes.Api.Services
{
    public class OrdenService
    {
        private readonly OrdenRepository _ordenRepository;
        private readonly MedicamentoRepository _medicamentoRepository;
        private readonly ProcedimientoRepository _procedimientoRepository;
        private readonly AyudaDiagnosticaRepository _ayudaDiagnosticaRepository;
        private readonly OrdenValidator _ordenValidator;

        public OrdenService(
            OrdenRepository ordenRepository,
            MedicamentoRepository medicamentoRepository,
            ProcedimientoRepository procedimientoRepository,
            AyudaDiagnosticaRepository ayudaDiagnosticaRepository,
            OrdenValidator ordenValidator)
        {
            _ordenRepository = ordenRepository;
            _medicamentoRepository = medicamentoRepository;
            _procedimientoRepository = procedimientoRepository;
            _ayudaDiagnosticaRepository = ayudaDiagnosticaRepository;
            _ordenValidator = ordenValidator;
        }

        public async Task<List<OrdenDetalleDto>> GetAllAsync()
        {
            var ordenes = await _ordenRepository.GetAllAsync().ConfigureAwait(false);
            var detalles = await Task.WhenAll(ordenes.Select(ConstruirDetalleAsync)).ConfigureAwait(false);
            return detalles.ToList();
        }

        public async Task<OrdenDetalleDto?> GetByNumeroOrdenAsync(int numeroOrden)
        {
            var orden = await _ordenRepository.GetByNumeroOrdenAsync(numeroOrden).ConfigureAwait(false);
            if (orden == null)
            {
                return null;
            }

            return await ConstruirDetalleAsync(orden).ConfigureAwait(false);
        }

        public async Task<List<OrdenDetalleDto>> GetByPacienteAsync(string cedulaPaciente)
        {
            var ordenes = await _ordenRepository.GetByPacienteAsync(cedulaPaciente).ConfigureAwait(false);
            var detalles = await Task.WhenAll(ordenes.Select(ConstruirDetalleAsync)).ConfigureAwait(false);
            return detalles.ToList();
        }

        public async Task<OrdenDetalleDto> CreateAsync(OrdenCreateRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var orden = ConstruirOrden(request);

            var medicamentos = request.Medicamentos
                .Select(m => ConstruirMedicamento(orden.NumeroOrden, m))
                .ToList();

            var procedimientos = request.Procedimientos
                .Select(p => ConstruirProcedimiento(orden.NumeroOrden, p))
                .ToList();

            var ayudas = request.AyudasDiagnosticas
                .Select(a => ConstruirAyudaDiagnostica(orden.NumeroOrden, a))
                .ToList();

            var errores = await ValidarNuevaOrdenAsync(orden).ConfigureAwait(false);
            ValidarCatalogos(orden.NumeroOrden, medicamentos, procedimientos, ayudas, errores);

            var erroresReglas = await _ordenValidator
                .ValidarAsync(new OrdenValidacionContext(orden, medicamentos, procedimientos, ayudas))
                .ConfigureAwait(false);

            CombinarErrores(errores, erroresReglas);

            if (errores.Count > 0)
            {
                throw new BusinessRuleValidationException(errores);
            }

            await _ordenRepository.CreateAsync(orden).ConfigureAwait(false);

            try
            {
                foreach (var medicamento in medicamentos)
                {
                    await _medicamentoRepository.CreateAsync(medicamento).ConfigureAwait(false);
                }

                foreach (var procedimiento in procedimientos)
                {
                    await _procedimientoRepository.CreateAsync(procedimiento).ConfigureAwait(false);
                }

                foreach (var ayuda in ayudas)
                {
                    await _ayudaDiagnosticaRepository.CreateAsync(ayuda).ConfigureAwait(false);
                }
            }
            catch
            {
                await _medicamentoRepository.DeleteByOrdenAsync(orden.NumeroOrden).ConfigureAwait(false);
                await _procedimientoRepository.DeleteByOrdenAsync(orden.NumeroOrden).ConfigureAwait(false);
                await _ayudaDiagnosticaRepository.DeleteByOrdenAsync(orden.NumeroOrden).ConfigureAwait(false);
                await _ordenRepository.DeleteAsync(orden.NumeroOrden).ConfigureAwait(false);
                throw;
            }

            return MapearDetalle(orden, medicamentos, procedimientos, ayudas);
        }

        public async Task<bool> UpdateAsync(int numeroOrden, Orden ordenActualizada)
        {
            if (ordenActualizada == null)
            {
                throw new ArgumentNullException(nameof(ordenActualizada));
            }

            var existente = await _ordenRepository.GetByNumeroOrdenAsync(numeroOrden).ConfigureAwait(false);
            if (existente == null)
            {
                return false;
            }

            ordenActualizada.Id = existente.Id;
            ordenActualizada.NumeroOrden = numeroOrden;

            await _ordenRepository.UpdateAsync(numeroOrden, ordenActualizada).ConfigureAwait(false);
            return true;
        }

        private async Task<Dictionary<string, List<string>>> ValidarNuevaOrdenAsync(Orden orden)
        {
            var errores = new Dictionary<string, List<string>>();

            var existente = await _ordenRepository.GetByNumeroOrdenAsync(orden.NumeroOrden).ConfigureAwait(false);
            if (existente != null)
            {
                AgregarError(errores, "numeroOrden", $"La orden con número {orden.NumeroOrden} ya existe.");
            }

            ValidarCedula("cedulaPaciente", orden.CedulaPaciente, errores);
            ValidarCedula("cedulaMedico", orden.CedulaMedico, errores);

            return errores;
        }

        private async Task<OrdenDetalleDto> ConstruirDetalleAsync(Orden orden)
        {
            var medicamentosTask = _medicamentoRepository.GetByNumeroOrdenAsync(orden.NumeroOrden);
            var procedimientosTask = _procedimientoRepository.GetByNumeroOrdenAsync(orden.NumeroOrden);
            var ayudasTask = _ayudaDiagnosticaRepository.GetByNumeroOrdenAsync(orden.NumeroOrden);

            await Task.WhenAll(medicamentosTask, procedimientosTask, ayudasTask).ConfigureAwait(false);

            return MapearDetalle(
                orden,
                medicamentosTask.Result,
                procedimientosTask.Result,
                ayudasTask.Result);
        }

        private static Orden ConstruirOrden(OrdenCreateRequest request)
        {
            return new Orden
            {
                Id = ObjectId.GenerateNewId(),
                NumeroOrden = request.NumeroOrden,
                CedulaPaciente = request.CedulaPaciente?.Trim() ?? string.Empty,
                CedulaMedico = request.CedulaMedico?.Trim() ?? string.Empty,
                FechaCreacion = (request.FechaCreacion ?? DateTime.UtcNow).ToUniversalTime(),
                Estado = request.Estado
            };
        }

        private static Medicamento ConstruirMedicamento(int numeroOrden, OrdenMedicamentoDto dto)
        {
            return new Medicamento
            {
                Id = ObjectId.GenerateNewId(),
                NumeroOrden = numeroOrden,
                NumeroItem = dto.NumeroItem,
                CatalogoId = dto.CatalogoId?.Trim() ?? string.Empty,
                Dosis = dto.Dosis?.Trim() ?? string.Empty,
                DuracionTratamiento = dto.DuracionTratamiento
            };
        }

        private static Procedimiento ConstruirProcedimiento(int numeroOrden, OrdenProcedimientoDto dto)
        {
            return new Procedimiento
            {
                Id = ObjectId.GenerateNewId(),
                NumeroOrden = numeroOrden,
                NumeroItem = dto.NumeroItem,
                CatalogoId = dto.CatalogoId?.Trim() ?? string.Empty,
                NumeroVecesRepite = dto.NumeroVecesRepite,
                Frecuencia = dto.Frecuencia?.Trim() ?? string.Empty
            };
        }

        private static AyudaDiagnostica ConstruirAyudaDiagnostica(int numeroOrden, OrdenAyudaDiagnosticaDto dto)
        {
            return new AyudaDiagnostica
            {
                Id = ObjectId.GenerateNewId(),
                NumeroOrden = numeroOrden,
                NumeroItem = dto.NumeroItem,
                CatalogoId = dto.CatalogoId?.Trim() ?? string.Empty,
                Cantidad = dto.Cantidad
            };
        }

        private static OrdenDetalleDto MapearDetalle(
            Orden orden,
            IEnumerable<Medicamento> medicamentos,
            IEnumerable<Procedimiento> procedimientos,
            IEnumerable<AyudaDiagnostica> ayudas)
        {
            return new OrdenDetalleDto
            {
                NumeroOrden = orden.NumeroOrden,
                CedulaPaciente = orden.CedulaPaciente ?? string.Empty,
                CedulaMedico = orden.CedulaMedico ?? string.Empty,
                FechaCreacion = orden.FechaCreacion,
                Estado = orden.Estado,
                Medicamentos = medicamentos
                    .OrderBy(m => m.NumeroItem)
                    .Select(MapearMedicamentoDto)
                    .ToList(),
                Procedimientos = procedimientos
                    .OrderBy(p => p.NumeroItem)
                    .Select(MapearProcedimientoDto)
                    .ToList(),
                AyudasDiagnosticas = ayudas
                    .OrderBy(a => a.NumeroItem)
                    .Select(MapearAyudaDiagnosticaDto)
                    .ToList()
            };
        }

        private static OrdenMedicamentoDto MapearMedicamentoDto(Medicamento medicamento) =>
            new OrdenMedicamentoDto
            {
                NumeroItem = medicamento.NumeroItem,
                CatalogoId = medicamento.CatalogoId,
                Dosis = medicamento.Dosis,
                DuracionTratamiento = medicamento.DuracionTratamiento
            };

        private static OrdenProcedimientoDto MapearProcedimientoDto(Procedimiento procedimiento) =>
            new OrdenProcedimientoDto
            {
                NumeroItem = procedimiento.NumeroItem,
                CatalogoId = procedimiento.CatalogoId,
                NumeroVecesRepite = procedimiento.NumeroVecesRepite,
                Frecuencia = procedimiento.Frecuencia
            };

        private static OrdenAyudaDiagnosticaDto MapearAyudaDiagnosticaDto(AyudaDiagnostica ayuda) =>
            new OrdenAyudaDiagnosticaDto
            {
                NumeroItem = ayuda.NumeroItem,
                CatalogoId = ayuda.CatalogoId,
                Cantidad = ayuda.Cantidad
            };

        private static void ValidarCedula(string campo, string? valor, Dictionary<string, List<string>> errores)
        {
            var normalizada = valor?.Trim() ?? string.Empty;
            var etiqueta = campo == "cedulaPaciente" ? "La cédula del paciente" : "La cédula del médico";

            if (string.IsNullOrEmpty(normalizada))
            {
                AgregarError(errores, campo, $"{etiqueta} es obligatoria.");
                return;
            }

            if (normalizada.Length > 10)
            {
                AgregarError(errores, campo, $"{etiqueta} debe tener máximo 10 caracteres.");
            }
        }

        private static void ValidarCatalogos(
            int numeroOrden,
            IEnumerable<Medicamento> medicamentos,
            IEnumerable<Procedimiento> procedimientos,
            IEnumerable<AyudaDiagnostica> ayudas,
            Dictionary<string, List<string>> errores)
        {
            foreach (var medicamento in medicamentos.Where(m => string.IsNullOrWhiteSpace(m.CatalogoId)))
            {
                AgregarError(errores, "catalogoId", $"El medicamento con ítem {medicamento.NumeroItem} en la orden {numeroOrden} debe indicar un catalogoId válido.");
            }

            foreach (var procedimiento in procedimientos.Where(p => string.IsNullOrWhiteSpace(p.CatalogoId)))
            {
                AgregarError(errores, "catalogoId", $"El procedimiento con ítem {procedimiento.NumeroItem} en la orden {numeroOrden} debe indicar un catalogoId válido.");
            }

            foreach (var ayuda in ayudas.Where(a => string.IsNullOrWhiteSpace(a.CatalogoId)))
            {
                AgregarError(errores, "catalogoId", $"La ayuda diagnóstica con ítem {ayuda.NumeroItem} en la orden {numeroOrden} debe indicar un catalogoId válido.");
            }
        }

        private static void CombinarErrores(Dictionary<string, List<string>> destino, Dictionary<string, List<string>> origen)
        {
            foreach (var kvp in origen)
            {
                if (!destino.TryGetValue(kvp.Key, out var lista))
                {
                    lista = new List<string>();
                    destino[kvp.Key] = lista;
                }

                lista.AddRange(kvp.Value);
            }
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

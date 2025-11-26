using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ordenes.Api.Models;
using Ordenes.Api.Services;

namespace Ordenes.Api.Controllers
{
    [ApiController]
    [Route("api/ordenes/{numeroOrden:int}/medicamentos")]
    public class MedicamentosController : ControllerBase
    {
        private readonly MedicamentoService _medicamentoService;

        public MedicamentosController(MedicamentoService medicamentoService)
        {
            _medicamentoService = medicamentoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Medicamento>>> GetPorOrden(int numeroOrden)
        {
            var medicamentos = await _medicamentoService.GetByNumeroOrdenAsync(numeroOrden);
            return Ok(medicamentos);
        }

        [HttpGet("{numeroItem:int}")]
        public async Task<ActionResult<Medicamento>> GetPorOrdenItem(int numeroOrden, int numeroItem)
        {
            var medicamento = await _medicamentoService.GetByNumeroOrdenItemAsync(numeroOrden, numeroItem);
            if (medicamento == null)
            {
                return NotFound();
            }

            return Ok(medicamento);
        }

        [HttpGet("/api/medicamentos")]
        public async Task<ActionResult<List<Medicamento>>> GetTodos()
        {
            var medicamentos = await _medicamentoService.GetAllAsync();
            return Ok(medicamentos);
        }
    }
}

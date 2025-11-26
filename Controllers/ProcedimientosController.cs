using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ordenes.Api.Models;
using Ordenes.Api.Services;

namespace Ordenes.Api.Controllers
{
    [ApiController]
    [Route("api/ordenes/{numeroOrden:int}/procedimientos")]
    public class ProcedimientosController : ControllerBase
    {
        private readonly ProcedimientoService _procedimientoService;

        public ProcedimientosController(ProcedimientoService procedimientoService)
        {
            _procedimientoService = procedimientoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Procedimiento>>> GetPorOrden(int numeroOrden)
        {
            var procedimientos = await _procedimientoService.GetByNumeroOrdenAsync(numeroOrden);
            return Ok(procedimientos);
        }

        [HttpGet("{numeroItem:int}")]
        public async Task<ActionResult<Procedimiento>> GetPorOrdenItem(int numeroOrden, int numeroItem)
        {
            var procedimiento = await _procedimientoService.GetByNumeroOrdenItemAsync(numeroOrden, numeroItem);
            if (procedimiento == null)
            {
                return NotFound();
            }

            return Ok(procedimiento);
        }

        [HttpGet("/api/procedimientos")]
        public async Task<ActionResult<List<Procedimiento>>> GetTodos()
        {
            var procedimientos = await _procedimientoService.GetAllAsync();
            return Ok(procedimientos);
        }
    }
}

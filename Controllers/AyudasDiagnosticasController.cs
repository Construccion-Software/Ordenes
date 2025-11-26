using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ordenes.Api.Models;
using Ordenes.Api.Services;

namespace Ordenes.Api.Controllers
{
    [ApiController]
    [Route("api/ordenes/{numeroOrden:int}/ayudas-diagnosticas")]
    public class AyudasDiagnosticasController : ControllerBase
    {
        private readonly AyudaDiagnosticaService _ayudaDiagnosticaService;

        public AyudasDiagnosticasController(AyudaDiagnosticaService ayudaDiagnosticaService)
        {
            _ayudaDiagnosticaService = ayudaDiagnosticaService;
        }

        [HttpGet]
        public async Task<ActionResult<List<AyudaDiagnostica>>> GetPorOrden(int numeroOrden)
        {
            var ayudas = await _ayudaDiagnosticaService.GetByNumeroOrdenAsync(numeroOrden);
            return Ok(ayudas);
        }

        [HttpGet("{numeroItem:int}")]
        public async Task<ActionResult<AyudaDiagnostica>> GetPorOrdenItem(int numeroOrden, int numeroItem)
        {
            var ayuda = await _ayudaDiagnosticaService.GetByNumeroOrdenItemAsync(numeroOrden, numeroItem);
            if (ayuda == null)
            {
                return NotFound();
            }

            return Ok(ayuda);
        }

        [HttpGet("/api/ayudas-diagnosticas")]
        public async Task<ActionResult<List<AyudaDiagnostica>>> GetTodas()
        {
            var ayudas = await _ayudaDiagnosticaService.GetAllAsync();
            return Ok(ayudas);
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ordenes.Api.BusinessRules;
using Ordenes.Api.Models;
using Ordenes.Api.Models.Dto;
using Ordenes.Api.Services;

namespace Ordenes.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdenesController : ControllerBase
    {
        private readonly OrdenService _ordenService;

        public OrdenesController(OrdenService ordenService)
        {
            _ordenService = ordenService;
        }

        [HttpGet]
        public async Task<ActionResult<List<OrdenDetalleDto>>> GetTodas()
        {
            var ordenes = await _ordenService.GetAllAsync();
            return Ok(ordenes);
        }

        [HttpGet("{numeroOrden:int}")]
        public async Task<ActionResult<OrdenDetalleDto>> GetPorNumero(int numeroOrden)
        {
            var orden = await _ordenService.GetByNumeroOrdenAsync(numeroOrden);
            if (orden == null)
            {
                return NotFound();
            }

            return Ok(orden);
        }

        [HttpGet("paciente/{cedulaPaciente}")]
        public async Task<ActionResult<List<OrdenDetalleDto>>> GetPorPaciente(string cedulaPaciente)
        {
            var ordenes = await _ordenService.GetByPacienteAsync(cedulaPaciente);
            return Ok(ordenes);
        }

        [HttpPost]
        public async Task<ActionResult> CrearOrden([FromBody] OrdenCreateRequest orden)
        {
            try
            {
                var creada = await _ordenService.CreateAsync(orden);
                return CreatedAtAction(nameof(GetPorNumero), new { numeroOrden = creada.NumeroOrden }, creada);
            }
            catch (BusinessRuleValidationException ex)
            {
                var respuesta = new ValidationErrorResponse
                {
                    TraceId = HttpContext.TraceIdentifier,
                    Errors = ex.Errores
                };

                return Conflict(respuesta);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Ocurrió un error inesperado al crear la orden.",
                    traceId = HttpContext.TraceIdentifier,
                    detalle = ex.Message
                });
            }
        }

        [HttpPut("{numeroOrden:int}")]
        public async Task<IActionResult> ActualizarOrden(int numeroOrden, [FromBody] Orden orden)
        {
            var actualizada = await _ordenService.UpdateAsync(numeroOrden, orden);
            if (!actualizada)
            {
                return NotFound();
            }

            return NoContent();
        }

    }
}

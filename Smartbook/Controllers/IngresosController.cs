using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smartbook.LogicaDeNegocio.Services;
using Smartbook.LogicaDeNegocio.Dtos;

namespace Smartbook.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class IngresosController : ControllerBase
{
    private readonly IIngresoService _ingresoService;
    private readonly ILogger<IngresosController> _logger;

    public IngresosController(IIngresoService ingresoService, ILogger<IngresosController> logger)
    {
        _ingresoService = ingresoService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<IngresoDto>> Create([FromBody] IngresoCreateDto dto)
    {
        try
        {
            var ingreso = await _ingresoService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = ingreso.Id }, ingreso);
        }
        catch (Smartbook.LogicaDeNegocio.Exceptions.BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IngresoDto>> GetById(int id)
    {
        var ingreso = await _ingresoService.GetByIdAsync(id);
        if (ingreso == null)
        {
            return NotFound();
        }
        return Ok(ingreso);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<IngresoResumenDto>>> Search(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? lote,
        [FromQuery] int? libroId)
    {
        var ingresos = await _ingresoService.SearchAsync(desde, hasta, lote, libroId);
        return Ok(ingresos);
    }
}


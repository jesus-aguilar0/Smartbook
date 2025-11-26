using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smartbook.LogicaDeNegocio.Services;
using Smartbook.LogicaDeNegocio.Dtos;

namespace Smartbook.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VentasController : ControllerBase
{
    private readonly IVentaService _ventaService;
    private readonly ILogger<VentasController> _logger;

    public VentasController(IVentaService ventaService, ILogger<VentasController> logger)
    {
        _ventaService = ventaService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<VentaDto>> Create([FromBody] VentaCreateDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var usuarioId))
            {
                return Unauthorized(new { message = "Usuario no válido." });
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var venta = await _ventaService.CreateAsync(dto, usuarioId, baseUrl);
            return CreatedAtAction(nameof(GetById), new { id = venta.Id }, venta);
        }
        catch (Smartbook.LogicaDeNegocio.Exceptions.BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VentaDto>> GetById(int id)
    {
        var venta = await _ventaService.GetByIdAsync(id);
        if (venta == null)
        {
            return NotFound();
        }
        return Ok(venta);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VentaResumenDto>>> Search(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? clienteIdentificacion,
        [FromQuery] int? libroId)
    {
        var ventas = await _ventaService.SearchAsync(desde, hasta, clienteIdentificacion, libroId);
        return Ok(ventas);
    }
}


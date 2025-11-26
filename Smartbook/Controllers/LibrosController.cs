using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smartbook.LogicaDeNegocio.Services;
using Smartbook.LogicaDeNegocio.Dtos;

namespace Smartbook.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LibrosController : ControllerBase
{
    private readonly ILibroService _libroService;
    private readonly ILogger<LibrosController> _logger;

    public LibrosController(ILibroService libroService, ILogger<LibrosController> logger)
    {
        _libroService = libroService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LibroDto>> Create([FromBody] LibroCreateDto dto)
    {
        try
        {
            var libro = await _libroService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = libro.Id }, libro);
        }
        catch (Smartbook.LogicaDeNegocio.Exceptions.BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LibroDto>> GetById(int id)
    {
        var libro = await _libroService.GetByIdAsync(id);
        if (libro == null)
        {
            return NotFound();
        }
        return Ok(libro);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LibroResumenDto>>> Search(
        [FromQuery] string? nombre,
        [FromQuery] string? nivel,
        [FromQuery] int? tipo,
        [FromQuery] string? editorial,
        [FromQuery] string? edicion)
    {
        var libros = await _libroService.SearchAsync(nombre, nivel, tipo, editorial, edicion);
        return Ok(libros);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LibroDto>> Update(int id, [FromBody] LibroUpdateDto dto)
    {
        try
        {
            var libro = await _libroService.UpdateAsync(id, dto);
            return Ok(libro);
        }
        catch (Smartbook.LogicaDeNegocio.Exceptions.BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Sincronizar stock del libro con inventarios reales
    /// </summary>
    /// <param name="id">ID del libro</param>
    /// <returns>Mensaje de confirmación</returns>
    [HttpPost("{id}/sincronizar-stock")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> SincronizarStock(int id)
    {
        try
        {
            await _libroService.SincronizarStockAsync(id);
            return Ok(new { message = "Stock sincronizado correctamente." });
        }
        catch (Smartbook.LogicaDeNegocio.Exceptions.BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smartbook.LogicaDeNegocio.Services;
using Smartbook.LogicaDeNegocio.Dtos;

namespace Smartbook.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventariosController : ControllerBase
{
    private readonly IInventarioService _inventarioService;

    public InventariosController(IInventarioService inventarioService)
    {
        _inventarioService = inventarioService;
    }

    /// <summary>
    /// Obtener inventarios por lote
    /// </summary>
    /// <param name="lote">Número de lote</param>
    /// <returns>Lista de inventarios del lote</returns>
    [HttpGet("lote/{lote}")]
    public async Task<ActionResult<IEnumerable<InventarioDto>>> GetByLote(string lote)
    {
        var inventarios = await _inventarioService.GetByLoteAsync(lote);
        return Ok(inventarios);
    }

  
    /// Obtener inventarios por ID de libro
    
    /// <param name="libroId">ID del libro</param>
    /// <returns>Lista de inventarios del libro</returns>
    [HttpGet("{libroId}")]
    public async Task<ActionResult<IEnumerable<InventarioDto>>> GetByLibroId(int libroId)
    {
        var inventarios = await _inventarioService.GetByLibroIdAsync(libroId);
        return Ok(inventarios);
    }
}


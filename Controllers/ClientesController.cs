using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smartbook.LogicaDeNegocio.Services;
using Smartbook.LogicaDeNegocio.Dtos;

namespace Smartbook.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(IClienteService clienteService, ILogger<ClientesController> logger)
    {
        _clienteService = clienteService;
        _logger = logger;
    }

    /// <summary>
    /// Crear un nuevo cliente
    /// </summary>
    /// <remarks>
    /// Crea un nuevo cliente en el sistema.
    /// 
    /// **Formato de fecha:** Debe ser ISO 8601 (YYYY-MM-DD o YYYY-MM-DDTHH:mm:ss)
    /// Ejemplo: "2006-02-10" o "2006-02-10T00:00:00"
    /// </remarks>
    /// <param name="dto">Datos del cliente a crear</param>
    /// <returns>Cliente creado</returns>
    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Create([FromBody] ClienteCreateDto? dto)
    {
        try
        {
            // Validar que el DTO no sea null
            if (dto == null)
            {
                _logger.LogWarning("Intento de crear cliente con DTO null");
                return BadRequest(new { message = "El cuerpo de la solicitud no puede estar vacío." });
            }

            _logger.LogInformation("Creando cliente con identificación: {Identificacion}", dto.Identificacion);
            var cliente = await _clienteService.CreateAsync(dto);
            _logger.LogInformation("Cliente creado exitosamente con ID: {Id}, Identificación: {Identificacion}", cliente.Id, cliente.Identificacion);
            return CreatedAtAction(nameof(GetByIdentificacion), new { identificacion = cliente.Identificacion }, cliente);
        }
        catch (Smartbook.LogicaDeNegocio.Exceptions.BusinessException ex)
        {
            _logger.LogWarning(ex, "Error de negocio al crear cliente: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear cliente");
            return StatusCode(500, new { message = "Ha ocurrido un error al procesar la solicitud." });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteResumenDto>>> Search([FromQuery] string? nombres)
    {
        try
        {
            _logger.LogInformation("Buscando clientes. Filtro nombres: {Nombres}", nombres ?? "ninguno");
            var clientes = await _clienteService.SearchAsync(nombres);
            _logger.LogInformation("Se encontraron {Count} clientes", clientes.Count());
            return Ok(clientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar clientes");
            return StatusCode(500, new { message = "Ha ocurrido un error al procesar la solicitud." });
        }
    }

    [HttpGet("id/{id}")]
    public async Task<ActionResult<ClienteDto>> GetById(int id)
    {
        try
        {
            _logger.LogInformation("Buscando cliente por ID: {Id}", id);
            var cliente = await _clienteService.GetByIdAsync(id);
            if (cliente == null)
            {
                _logger.LogWarning("Cliente con ID {Id} no encontrado", id);
                return NotFound(new { message = $"Cliente con ID {id} no encontrado." });
            }
            _logger.LogInformation("Cliente encontrado: ID {Id}, Identificación: {Identificacion}", cliente.Id, cliente.Identificacion);
            return Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cliente por ID: {Id}", id);
            return StatusCode(500, new { message = "Ha ocurrido un error al procesar la solicitud." });
        }
    }

    [HttpGet("{identificacion}")]
    public async Task<ActionResult<ClienteDto>> GetByIdentificacion(string identificacion)
    {
        try
        {
            _logger.LogInformation("Buscando cliente por identificación: {Identificacion}", identificacion);
            var cliente = await _clienteService.GetByIdentificacionAsync(identificacion);
            if (cliente == null)
            {
                _logger.LogWarning("Cliente con identificación {Identificacion} no encontrado", identificacion);
                return NotFound(new { message = $"Cliente con identificación {identificacion} no encontrado." });
            }
            _logger.LogInformation("Cliente encontrado: ID {Id}, Identificación: {Identificacion}", cliente.Id, cliente.Identificacion);
            return Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cliente por identificación: {Identificacion}", identificacion);
            return StatusCode(500, new { message = "Ha ocurrido un error al procesar la solicitud." });
        }
    }

    [HttpPut("{identificacion}")]
    public async Task<ActionResult<ClienteDto>> Update(string identificacion, [FromBody] ClienteUpdateDto dto)
    {
        try
        {
            if (dto == null)
            {
                _logger.LogWarning("Intento de actualizar cliente con DTO null. Identificación: {Identificacion}", identificacion);
                return BadRequest(new { message = "El cuerpo de la solicitud no puede estar vacío." });
            }

            _logger.LogInformation("Actualizando cliente con identificación: {Identificacion}", identificacion);
            var cliente = await _clienteService.UpdateAsync(identificacion, dto);
            _logger.LogInformation("Cliente actualizado exitosamente: ID {Id}, Identificación: {Identificacion}", cliente.Id, cliente.Identificacion);
            return Ok(cliente);
        }
        catch (Smartbook.LogicaDeNegocio.Exceptions.BusinessException ex)
        {
            _logger.LogWarning(ex, "Error de negocio al actualizar cliente: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar cliente con identificación: {Identificacion}", identificacion);
            return StatusCode(500, new { message = "Ha ocurrido un error al procesar la solicitud." });
        }
    }
}


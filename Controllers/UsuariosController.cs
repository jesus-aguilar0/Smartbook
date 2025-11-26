using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smartbook.LogicaDeNegocio.Services;
using Smartbook.LogicaDeNegocio.Dtos;
using Smartbook.Entidades.Enums;

namespace Smartbook.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(IUsuarioService usuarioService, ILogger<UsuariosController> logger)
    {
        _usuarioService = usuarioService;
        _logger = logger;
    }

    /// <summary>
    /// Crear un nuevo usuario
    /// </summary>
    /// <remarks>
    /// Crea un nuevo usuario en el sistema. Al crear la cuenta:
    /// 1. Se genera un token de confirmación válido por 1 hora
    /// 2. Se envía un correo electrónico con un enlace para confirmar el correo
    /// 3. El usuario debe hacer clic en el enlace para activar su cuenta
    /// 4. Después de confirmar, se envía otro correo confirmando la creación exitosa
    /// 
    /// **Requisitos:**
    /// - Solo usuarios con rol Admin pueden crear usuarios
    /// - El correo debe ser institucional (@cecar.edu.co)
    /// - La contraseña debe tener al menos 8 caracteres
    /// </remarks>
    /// <param name="dto">Datos del usuario a crear</param>
    /// <returns>Usuario creado (sin confirmar aún)</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UsuarioDto>> Create([FromBody] UsuarioCreateDto dto)
    {
        try
        {
            // Verificar que el usuario esté autenticado y tenga el rol Admin
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized(new { message = "No está autenticado. Por favor, inicie sesión." });
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin")
            {
                _logger.LogWarning("Intento de crear usuario sin permisos de Admin. Usuario: {UserId}, Rol: {Rol}", 
                    User.FindFirstValue("UserId"), userRole);
                return Forbid("Solo los administradores pueden crear usuarios.");
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var usuario = await _usuarioService.CreateAsync(dto, baseUrl);
            _logger.LogInformation("Usuario creado por Admin {AdminId}: {Email}", 
                User.FindFirstValue("UserId"), usuario.Email);
            return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
        }
        catch (Smartbook.LogicaDeNegocio.Exceptions.BusinessException ex)
        {
            _logger.LogWarning("Error al crear usuario: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Iniciar sesión y obtener token JWT
    /// </summary>
    /// <remarks>
    /// Este endpoint permite autenticarse y obtener un token JWT que se usará para acceder a los demás endpoints protegidos.
    /// 
    /// **Credenciales por defecto:**
    /// - Email: admin@cecar.edu.co
    /// - Contraseña: AdminCDI123!
    /// 
    /// **Pasos para usar el token:**
    /// 1. Copia el token de la respuesta
    /// 2. En Swagger, haz clic en el botón "Authorize" 🔓
    /// 3. Pega el token (sin escribir "Bearer")
    /// 4. Haz clic en "Authorize" y luego "Close"
    /// 5. Ahora puedes usar todos los endpoints protegidos
    /// </remarks>
    /// <param name="dto">Credenciales de acceso (email y contraseña)</param>
    /// <returns>Token JWT y información del usuario</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
    {
        try
        {
            var response = await _usuarioService.LoginAsync(dto);
            return Ok(response);
        }
        catch (Smartbook.LogicaDeNegocio.Exceptions.BusinessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Confirmar correo electrónico
    /// </summary>
    /// <remarks>
    /// Confirma el correo electrónico del usuario usando el token recibido por email.
    /// 
    /// **Flujo:**
    /// 1. El usuario recibe un correo con un enlace al crear su cuenta
    /// 2. Al hacer clic en el enlace, se llama a este endpoint
    /// 3. Si el token es válido y no ha expirado (1 hora), se confirma el correo
    /// 4. Se envía un correo de confirmación de cuenta creada exitosamente
    /// 
    /// **Nota:** El token expira después de 1 hora. Si expira, se debe solicitar un nuevo enlace.
    /// </remarks>
    /// <param name="token">Token de confirmación recibido por correo electrónico</param>
    /// <returns>Mensaje de confirmación exitosa</returns>
    [HttpGet("confirmar-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
    {
        try
        {
            // Validar que el token no esté vacío
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { message = "El token de confirmación es requerido." });
            }

            // Decodificar el token si viene URL-encoded
            var decodedToken = Uri.UnescapeDataString(token);
            
            // Log para debugging (solo en desarrollo)
            _logger.LogInformation("Intento de confirmación de email con token: {TokenLength} caracteres", decodedToken.Length);

            await _usuarioService.ConfirmEmailAsync(decodedToken);
            return Ok(new { message = "Correo electrónico confirmado exitosamente. Su cuenta ha sido activada." });
        }
        catch (Smartbook.LogicaDeNegocio.Exceptions.BusinessException ex)
        {
            _logger.LogWarning("Error al confirmar email: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al confirmar email");
            return StatusCode(500, new { message = "Error interno al procesar la confirmación de correo." });
        }
    }

    /// <summary>
    /// Solicitar restablecimiento de contraseña
    /// </summary>
    /// <remarks>
    /// Solicita el restablecimiento de contraseña. Si el correo existe en el sistema:
    /// 1. Se genera un token de restablecimiento válido por 1 hora
    /// 2. Se envía un correo electrónico con el token y las instrucciones
    /// 3. El usuario debe usar el token para restablecer su contraseña
    /// 4. Después de restablecer, se envía un correo de notificación
    /// 
    /// **Seguridad:** Por seguridad, siempre se devuelve el mismo mensaje, incluso si el correo no existe.
    /// </remarks>
    /// <param name="dto">Email del usuario que solicita el restablecimiento</param>
    /// <returns>Mensaje de confirmación (siempre el mismo por seguridad)</returns>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestPasswordReset([FromBody] ResetPasswordDto dto)
    {
        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            await _usuarioService.RequestPasswordResetAsync(dto, baseUrl);
            return Ok(new { message = "Si el correo existe, se ha enviado un enlace para restablecer la contraseña." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al solicitar reset de contraseña");
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    /// <summary>
    /// Confirmar restablecimiento de contraseña
    /// </summary>
    /// <remarks>
    /// Restablece la contraseña del usuario usando el token recibido por correo electrónico.
    /// 
    /// **Flujo:**
    /// 1. El usuario solicita restablecimiento de contraseña
    /// 2. Recibe un correo con el token y las instrucciones
    /// 3. Usa este endpoint con el token y la nueva contraseña
    /// 4. Si el token es válido y no ha expirado (1 hora), se restablece la contraseña
    /// 5. Se envía un correo de notificación confirmando el restablecimiento
    /// 
    /// **Validaciones:**
    /// - El token debe ser válido y no haber expirado (1 hora)
    /// - La nueva contraseña debe tener al menos 8 caracteres
    /// - La nueva contraseña debe ser diferente a la actual
    /// </remarks>
    /// <param name="dto">Token y nueva contraseña</param>
    /// <returns>Mensaje de confirmación de restablecimiento exitoso</returns>
    [HttpPost("confirmar-reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ConfirmPasswordResetDto dto)
    {
        try
        {
            await _usuarioService.ResetPasswordAsync(dto);
            return Ok(new { message = "Contraseña restablecida exitosamente." });
        }
        catch (Smartbook.LogicaDeNegocio.Exceptions.BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UsuarioResumenDto>>> Search([FromQuery] string? nombres, [FromQuery] int? rol)
    {
        var usuarios = await _usuarioService.SearchAsync(nombres, rol);
        return Ok(usuarios);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<UsuarioDto>> GetById(int id)
    {
        var usuario = await _usuarioService.GetByIdAsync(id);
        if (usuario == null)
        {
            return NotFound();
        }
        return Ok(usuario);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UsuarioDto>> Update(int id, [FromBody] UsuarioUpdateDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized(new { message = "Usuario no válido." });
            }
            var usuario = await _usuarioService.UpdateAsync(id, dto, currentUserId);
            return Ok(usuario);
        }
        catch (Smartbook.LogicaDeNegocio.Exceptions.BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}


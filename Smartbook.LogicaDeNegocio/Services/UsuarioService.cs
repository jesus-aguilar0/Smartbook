using System.Security.Cryptography;
using Mapster;
using Microsoft.Extensions.Logging;
using Smartbook.LogicaDeNegocio.Extensions;
using Smartbook.LogicaDeNegocio.Dtos;
using Smartbook.Entidades;
using Smartbook.Entidades.Enums;
using Smartbook.LogicaDeNegocio.Exceptions;
using Smartbook.LogicaDeNegocio.Validators;
using Smartbook.Persistencia.Repositories;

namespace Smartbook.LogicaDeNegocio.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IJwtService jwtService,
        IEmailService emailService,
        ILogger<UsuarioService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<UsuarioDto> CreateAsync(UsuarioCreateDto dto, string baseUrl)
    {
        // Validar identificación
        var identificacionLimpia = ValidationHelper.CleanIdentification(dto.Identificacion);
        if (string.IsNullOrWhiteSpace(identificacionLimpia))
        {
            throw new BusinessException("La identificación es requerida.");
        }

        if (!ValidationHelper.IsValidColombianId(identificacionLimpia))
        {
            throw new BusinessException("La identificación no es válida. Debe ser una cédula colombiana válida (6-10 dígitos, solo números).");
        }

        // Validar nombres
        if (string.IsNullOrWhiteSpace(dto.Nombres))
        {
            throw new BusinessException("Los nombres son requeridos.");
        }

        if (!ValidationHelper.IsValidName(dto.Nombres))
        {
            throw new BusinessException("Los nombres solo pueden contener letras, espacios y los caracteres especiales permitidos (guiones, apóstrofes, puntos). Mínimo 2 caracteres, máximo 200.");
        }

        // Validar email
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new BusinessException("El correo electrónico es requerido.");
        }

        if (!ValidationHelper.IsValidInstitutionalEmail(dto.Email))
        {
            throw new BusinessException("Solo se aceptan correos institucionales de CECAR (@cecar.edu.co o @cecar).");
        }

        // Validar contraseña
        if (string.IsNullOrWhiteSpace(dto.Contrasena))
        {
            throw new BusinessException("La contraseña es requerida.");
        }

        if (!ValidationHelper.IsValidPassword(dto.Contrasena))
        {
            throw new BusinessException("La contraseña no cumple con los requisitos. Debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial.");
        }

        // Validar duplicados
        if (await _unitOfWork.Usuarios.ExistsAsync(u => u.Identificacion == identificacionLimpia))
        {
            throw new BusinessException("Ya existe un usuario con esta identificación.");
        }

        if (await _unitOfWork.Usuarios.ExistsAsync(u => u.Email == dto.Email.ToLowerInvariant().Trim()))
        {
            throw new BusinessException("Ya existe un usuario con este correo electrónico.");
        }

        var usuario = dto.Adapt<Usuario>();
        usuario.Identificacion = identificacionLimpia;
        usuario.Nombres = dto.Nombres.RemoveAccents().Sanitize();
        usuario.Email = dto.Email.ToLowerInvariant().Trim();
        usuario.ContrasenaHash = _passwordService.HashPassword(dto.Contrasena);
        usuario.EmailConfirmado = false;
        usuario.Activo = true;

        // Generar token de confirmación
        usuario.TokenConfirmacion = GenerateToken();
        usuario.TokenConfirmacionExpiracion = DateTime.UtcNow.AddHours(1);

        await _unitOfWork.Usuarios.AddAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        // Enviar correo de confirmación
        await _emailService.SendEmailConfirmationAsync(usuario.Email, usuario.TokenConfirmacion, baseUrl);

        _logger.LogInformation("Usuario creado: {Email}", usuario.Email);

        return usuario.Adapt<UsuarioDto>();
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var usuario = await _unitOfWork.Usuarios.GetByEmailAsync(dto.Email);
        if (usuario == null || !_passwordService.VerifyPassword(dto.Contrasena, usuario.ContrasenaHash))
        {
            throw new BusinessException("Credenciales inválidas.");
        }

        if (!usuario.EmailConfirmado)
        {
            throw new BusinessException("Debe confirmar su correo electrónico antes de iniciar sesión.");
        }

        if (!usuario.Activo)
        {
            throw new BusinessException("Su cuenta está inactiva. Contacte al administrador.");
        }

        var token = _jwtService.GenerateToken(usuario);

        _logger.LogInformation("Usuario autenticado: {Email}", usuario.Email);

        return new LoginResponseDto
        {
            Token = token,
            Expiracion = DateTime.UtcNow.AddHours(1),
            Usuario = usuario.Adapt<UsuarioResumenDto>()
        };
    }

    public async Task ConfirmEmailAsync(string token)
    {
        // Validar que el token no esté vacío
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new BusinessException("El token de confirmación es requerido.");
        }

        // Limpiar el token (remover espacios y caracteres especiales)
        var cleanToken = token.Trim();

        // Verificar si el token parece ser un JWT (tiene puntos, lo cual es característico de JWT)
        if (cleanToken.Contains('.') && cleanToken.Split('.').Length == 3)
        {
            throw new BusinessException("El token proporcionado es un token de autenticación (JWT). Debe usar el token de confirmación de email que recibió por correo electrónico al crear su cuenta.");
        }

        var usuario = await _unitOfWork.Usuarios.GetByTokenConfirmacionAsync(cleanToken);
        if (usuario == null)
        {
            // Verificar si el token existe pero está expirado (búsqueda más eficiente)
            var usuarioExpirado = await _unitOfWork.Usuarios.FirstOrDefaultAsync(u => u.TokenConfirmacion == cleanToken);
            
            if (usuarioExpirado != null && usuarioExpirado.TokenConfirmacionExpiracion.HasValue && 
                usuarioExpirado.TokenConfirmacionExpiracion.Value <= DateTime.UtcNow)
            {
                throw new BusinessException("El enlace de confirmación ha expirado. El enlace es válido por 1 hora. Por favor, solicite un nuevo enlace de confirmación.");
            }
            
            throw new BusinessException("Token de confirmación inválido. Asegúrese de usar el token que recibió por correo electrónico al crear su cuenta, no el token de autenticación (JWT).");
        }

        // Verificar que el token no haya expirado (doble verificación)
        if (usuario.TokenConfirmacionExpiracion.HasValue && usuario.TokenConfirmacionExpiracion.Value <= DateTime.UtcNow)
        {
            throw new BusinessException("El enlace de confirmación ha expirado. El enlace es válido por 1 hora. Por favor, solicite un nuevo enlace de confirmación.");
        }

        // Actualizar propiedades usando actualización directa en la base de datos
        // Esto asegura que los cambios se reflejen correctamente, especialmente para campos nullable
        var usuarioId = usuario.Id;
        usuario.EmailConfirmado = true;
        usuario.TokenConfirmacion = null;
        usuario.TokenConfirmacionExpiracion = null;

        // Forzar actualización explícita marcando todas las propiedades como modificadas
        await _unitOfWork.Usuarios.UpdateAsync(usuario);
        var rowsAffected = await _unitOfWork.SaveChangesAsync();
        
        // Verificar que se actualizó correctamente
        if (rowsAffected == 0)
        {
            _logger.LogWarning("No se actualizaron filas al confirmar email para usuario {UsuarioId}. Reintentando con actualización directa...", usuarioId);
            
            // Obtener el usuario nuevamente y actualizar
            var usuarioParaActualizar = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId);
            if (usuarioParaActualizar != null)
            {
                usuarioParaActualizar.EmailConfirmado = true;
                usuarioParaActualizar.TokenConfirmacion = null;
                usuarioParaActualizar.TokenConfirmacionExpiracion = null;
                await _unitOfWork.Usuarios.UpdateAsync(usuarioParaActualizar);
                await _unitOfWork.SaveChangesAsync();
            }
        }
        
        _logger.LogInformation("Email confirmado exitosamente para usuario {UsuarioId}. EmailConfirmado: {EmailConfirmado}, TokenConfirmacion: {TokenConfirmacion}", 
            usuarioId, usuario.EmailConfirmado, usuario.TokenConfirmacion == null ? "null" : "no null");

        // Enviar correo de confirmación de cuenta creada (después de confirmar el email)
        // Si falla el envío del correo, no debe afectar la confirmación del email
        try
        {
            await _emailService.SendAccountCreatedConfirmationAsync(usuario.Email, usuario.Nombres);
            _logger.LogInformation("Correo de bienvenida enviado exitosamente a: {Email}", usuario.Email);
        }
        catch (Exception ex)
        {
            // Log el error pero no fallar la confirmación del email
            _logger.LogError(ex, "Error al enviar correo de bienvenida a {Email}. El email ya fue confirmado correctamente.", usuario.Email);
        }

        _logger.LogInformation("Email confirmado: {Email}", usuario.Email);
    }

    public async Task RequestPasswordResetAsync(ResetPasswordDto dto, string baseUrl)
    {
        var usuario = await _unitOfWork.Usuarios.GetByEmailAsync(dto.Email);
        if (usuario == null)
        {
            // No revelar que el email no existe por seguridad
            return;
        }

        usuario.TokenResetPassword = GenerateToken();
        usuario.TokenResetPasswordExpiracion = DateTime.UtcNow.AddHours(1);

        await _unitOfWork.Usuarios.UpdateAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        await _emailService.SendPasswordResetAsync(usuario.Email, usuario.TokenResetPassword, baseUrl);

        _logger.LogInformation("Solicitud de reset de contraseña: {Email}", usuario.Email);
    }

    public async Task ResetPasswordAsync(ConfirmPasswordResetDto dto)
    {
        var usuario = await _unitOfWork.Usuarios.GetByTokenResetPasswordAsync(dto.Token);
        if (usuario == null)
        {
            // Verificar si el token existe pero está expirado (búsqueda más eficiente)
            var usuarioExpirado = await _unitOfWork.Usuarios.FirstOrDefaultAsync(u => u.TokenResetPassword == dto.Token);
            
            if (usuarioExpirado != null && usuarioExpirado.TokenResetPasswordExpiracion.HasValue && 
                usuarioExpirado.TokenResetPasswordExpiracion.Value <= DateTime.UtcNow)
            {
                throw new BusinessException("El enlace de restablecimiento de contraseña ha expirado. El enlace es válido por 1 hora. Por favor, solicite un nuevo restablecimiento.");
            }
            
            throw new BusinessException("Token de restablecimiento inválido.");
        }

        // Verificar que el token no haya expirado (doble verificación)
        if (usuario.TokenResetPasswordExpiracion.HasValue && usuario.TokenResetPasswordExpiracion.Value <= DateTime.UtcNow)
        {
            throw new BusinessException("El enlace de restablecimiento de contraseña ha expirado. El enlace es válido por 1 hora. Por favor, solicite un nuevo restablecimiento.");
        }

        if (string.IsNullOrWhiteSpace(dto.NuevaContrasena))
        {
            throw new BusinessException("La nueva contraseña es requerida.");
        }

        if (!ValidationHelper.IsValidPassword(dto.NuevaContrasena))
        {
            throw new BusinessException("La contraseña no cumple con los requisitos. Debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial.");
        }

        // Validar que la nueva contraseña sea diferente a la anterior
        if (_passwordService.VerifyPassword(dto.NuevaContrasena, usuario.ContrasenaHash))
        {
            throw new BusinessException("La nueva contraseña debe ser diferente a la contraseña actual.");
        }

        usuario.ContrasenaHash = _passwordService.HashPassword(dto.NuevaContrasena);
        usuario.TokenResetPassword = null;
        usuario.TokenResetPasswordExpiracion = null;

        await _unitOfWork.Usuarios.UpdateAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        // Enviar correo de notificación de restablecimiento de contraseña
        await _emailService.SendPasswordResetConfirmationAsync(usuario.Email, usuario.Nombres);

        _logger.LogInformation("Contraseña restablecida: {Email}", usuario.Email);
    }

    public async Task<IEnumerable<UsuarioResumenDto>> SearchAsync(string? nombres, int? rol)
    {
        var usuarios = await _unitOfWork.Usuarios.SearchAsync(nombres, rol);
        return usuarios.Adapt<IEnumerable<UsuarioResumenDto>>();
    }

    public async Task<UsuarioDto> UpdateAsync(int id, UsuarioUpdateDto dto, int currentUserId)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
        if (usuario == null)
        {
            throw new BusinessException("Usuario no encontrado.");
        }

        var currentUser = await _unitOfWork.Usuarios.GetByIdAsync(currentUserId);
        if (currentUser == null || currentUser.Rol != Rol.Admin)
        {
            throw new BusinessException("Solo los administradores pueden editar usuarios.");
        }

        // Si se está cambiando el rol a Admin, solo otro Admin puede hacerlo
        if (dto.Rol.HasValue && dto.Rol.Value == Rol.Admin && currentUser.Rol != Rol.Admin)
        {
            throw new BusinessException("Solo los administradores pueden otorgar permisos de administrador.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Nombres))
        {
            if (!ValidationHelper.IsValidName(dto.Nombres))
            {
                throw new BusinessException("Los nombres solo pueden contener letras, espacios y los caracteres especiales permitidos (guiones, apóstrofes, puntos). Mínimo 2 caracteres, máximo 200.");
            }

            usuario.Nombres = dto.Nombres.RemoveAccents().Sanitize();
        }

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            if (!ValidationHelper.IsValidInstitutionalEmail(dto.Email))
            {
                throw new BusinessException("Solo se aceptan correos institucionales de CECAR (@cecar.edu.co o @cecar).");
            }

            var emailNormalizado = dto.Email.ToLowerInvariant().Trim();
            if (usuario.Email != emailNormalizado && await _unitOfWork.Usuarios.ExistsAsync(u => u.Email == emailNormalizado))
            {
                throw new BusinessException("Ya existe un usuario con este correo electrónico.");
            }

            usuario.Email = emailNormalizado;
        }

        if (dto.Rol.HasValue)
        {
            usuario.Rol = dto.Rol.Value;
        }

        if (dto.Activo.HasValue)
        {
            usuario.Activo = dto.Activo.Value;
        }

        await _unitOfWork.Usuarios.UpdateAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Usuario actualizado: {Id}", usuario.Id);

        return usuario.Adapt<UsuarioDto>();
    }

    public async Task<UsuarioDto?> GetByIdAsync(int id)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
        return usuario?.Adapt<UsuarioDto>();
    }

    private static string GenerateToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

}


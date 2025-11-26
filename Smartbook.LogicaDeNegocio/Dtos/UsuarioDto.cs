using Smartbook.Entidades.Enums;

namespace Smartbook.LogicaDeNegocio.Dtos;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Identificacion { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Rol Rol { get; set; }
    public bool EmailConfirmado { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}

public class UsuarioCreateDto
{
    public string Identificacion { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Rol Rol { get; set; }
}

public class UsuarioUpdateDto
{
    public string Nombres { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Rol? Rol { get; set; }
    public bool? Activo { get; set; }
}

public class UsuarioResumenDto
{
    public int Id { get; set; }
    public string Identificacion { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Rol Rol { get; set; }
    public bool Activo { get; set; }
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
    public UsuarioResumenDto Usuario { get; set; } = null!;
}

public class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public class ConfirmPasswordResetDto
{
    public string Token { get; set; } = string.Empty;
    public string NuevaContrasena { get; set; } = string.Empty;
}

public class ConfirmEmailDto
{
    public string Token { get; set; } = string.Empty;
}


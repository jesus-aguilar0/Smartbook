using Smartbook.Entidades.Enums;

namespace Smartbook.Entidades;

public class Usuario : BaseEntity
{
    public string Identificacion { get; set; } = string.Empty;
    public string ContrasenaHash { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Rol Rol { get; set; }
    public bool EmailConfirmado { get; set; }
    public bool Activo { get; set; } = true;
    public string? TokenConfirmacion { get; set; }
    public DateTime? TokenConfirmacionExpiracion { get; set; }
    public string? TokenResetPassword { get; set; }
    public DateTime? TokenResetPasswordExpiracion { get; set; }

    // Navigation properties
    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}


namespace Smartbook.Entidades;

public class Cliente : BaseEntity
{
    public string Identificacion { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Celular { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }

    // Navigation properties
    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}


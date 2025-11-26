namespace Smartbook.Entidades;

public class Venta : BaseEntity
{
    public string NumeroReciboPago { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int ClienteId { get; set; }
    public int UsuarioId { get; set; }
    public string? Observaciones { get; set; }
    public decimal Total { get; set; }

    // Navigation properties
    public virtual Cliente Cliente { get; set; } = null!;
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
}


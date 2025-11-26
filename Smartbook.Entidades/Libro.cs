using Smartbook.Entidades.Enums;

namespace Smartbook.Entidades;

public class Libro : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
    public int Stock { get; set; }
    public TipoLibro Tipo { get; set; }
    public string Editorial { get; set; } = string.Empty;
    public string Edicion { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
    public virtual ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
}


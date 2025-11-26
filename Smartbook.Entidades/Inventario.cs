namespace Smartbook.Entidades;

public class Inventario : BaseEntity
{
    public int LibroId { get; set; }
    public string Lote { get; set; } = string.Empty;
    public int UnidadesDisponibles { get; set; }
    public int UnidadesVendidas { get; set; }

    // Navigation properties
    public virtual Libro Libro { get; set; } = null!;
}


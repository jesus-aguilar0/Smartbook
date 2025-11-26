namespace Smartbook.Entidades;

public class Ingreso : BaseEntity
{
    public DateTime Fecha { get; set; }
    public int LibroId { get; set; }
    public string Lote { get; set; } = string.Empty;
    public int Unidades { get; set; }
    public decimal ValorCompra { get; set; }
    public decimal ValorVentaPublico { get; set; }

    // Navigation properties
    public virtual Libro Libro { get; set; } = null!;
}


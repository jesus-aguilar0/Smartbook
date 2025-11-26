namespace Smartbook.Entidades;

public class DetalleVenta : BaseEntity
{
    public int VentaId { get; set; }
    public int LibroId { get; set; }
    public string Lote { get; set; } = string.Empty;
    public int Unidades { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Subtotal { get; set; }

    // Navigation properties
    public virtual Venta Venta { get; set; } = null!;
    public virtual Libro Libro { get; set; } = null!;
}


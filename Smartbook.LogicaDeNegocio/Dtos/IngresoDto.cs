namespace Smartbook.LogicaDeNegocio.Dtos;

public class IngresoDto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int LibroId { get; set; }
    public string LibroNombre { get; set; } = string.Empty;
    public string Lote { get; set; } = string.Empty;
    public int Unidades { get; set; }
    public decimal ValorCompra { get; set; }
    public decimal ValorVentaPublico { get; set; }
    public decimal Total { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class IngresoCreateDto
{
    public int LibroId { get; set; }
    public int Unidades { get; set; }
    public decimal ValorCompra { get; set; }
    public decimal ValorVentaPublico { get; set; }
}

public class IngresoResumenDto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string LibroNombre { get; set; } = string.Empty;
    public string Lote { get; set; } = string.Empty;
    public int Unidades { get; set; }
    public decimal Total { get; set; }
}


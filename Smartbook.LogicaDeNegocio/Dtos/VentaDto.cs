namespace Smartbook.LogicaDeNegocio.Dtos;

public class VentaDto
{
    public int Id { get; set; }
    public string NumeroReciboPago { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteIdentificacion { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public decimal Total { get; set; }
    public List<DetalleVentaDto> Detalles { get; set; } = new();
    public DateTime FechaCreacion { get; set; }
}

public class VentaCreateDto
{
    public string NumeroReciboPago { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string? Observaciones { get; set; }
    public List<DetalleVentaCreateDto> Detalles { get; set; } = new();
}

public class DetalleVentaDto
{
    public int Id { get; set; }
    public int LibroId { get; set; }
    public string LibroNombre { get; set; } = string.Empty;
    public string Lote { get; set; } = string.Empty;
    public int Unidades { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class DetalleVentaCreateDto
{
    public int LibroId { get; set; }
    public string Lote { get; set; } = string.Empty;
    public int Unidades { get; set; }
}

public class VentaResumenDto
{
    public int Id { get; set; }
    public string NumeroReciboPago { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public decimal Total { get; set; }
}


namespace Smartbook.LogicaDeNegocio.Dtos;

public class InventarioDto
{
    public int LibroId { get; set; }
    public string LibroNombre { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Editorial { get; set; } = string.Empty;
    public string Edicion { get; set; } = string.Empty;
    public string Lote { get; set; } = string.Empty;
    public int UnidadesDisponibles { get; set; }
    public int UnidadesVendidas { get; set; }
    public int TotalUnidades { get; set; }
}


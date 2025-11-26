using Smartbook.Entidades.Enums;

namespace Smartbook.LogicaDeNegocio.Dtos;

public class LibroDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
    public int Stock { get; set; }
    public TipoLibro Tipo { get; set; }
    public string Editorial { get; set; } = string.Empty;
    public string Edicion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}

public class LibroCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
    public TipoLibro Tipo { get; set; }
    public string Editorial { get; set; } = string.Empty;
    public string Edicion { get; set; } = string.Empty;
    public int Stock { get; set; } = 0;
}

public class LibroUpdateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
    public TipoLibro Tipo { get; set; }
    public string Editorial { get; set; } = string.Empty;
    public string Edicion { get; set; } = string.Empty;
    public int? Stock { get; set; }
}

public class LibroResumenDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
    public int Stock { get; set; }
    public TipoLibro Tipo { get; set; }
    public string Editorial { get; set; } = string.Empty;
    public string Edicion { get; set; } = string.Empty;
}


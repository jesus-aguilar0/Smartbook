namespace Smartbook.LogicaDeNegocio.Dtos;

public class ClienteDto
{
    public int Id { get; set; }
    public string Identificacion { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Celular { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}

public class ClienteCreateDto
{
    public string Identificacion { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Celular { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
}

public class ClienteUpdateDto
{
    public string Nombres { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Celular { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
}

public class ClienteResumenDto
{
    public int Id { get; set; }
    public string Identificacion { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}


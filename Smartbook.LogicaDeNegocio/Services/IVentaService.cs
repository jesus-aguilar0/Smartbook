using Smartbook.LogicaDeNegocio.Dtos;

namespace Smartbook.LogicaDeNegocio.Services;

public interface IVentaService
{
    Task<VentaDto> CreateAsync(VentaCreateDto dto, int usuarioId, string baseUrl);
    Task<VentaDto?> GetByIdAsync(int id);
    Task<IEnumerable<VentaResumenDto>> SearchAsync(DateTime? desde, DateTime? hasta, string? clienteIdentificacion, int? libroId);
}


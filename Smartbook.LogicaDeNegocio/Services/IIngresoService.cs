using Smartbook.LogicaDeNegocio.Dtos;

namespace Smartbook.LogicaDeNegocio.Services;

public interface IIngresoService
{
    Task<IngresoDto> CreateAsync(IngresoCreateDto dto);
    Task<IngresoDto?> GetByIdAsync(int id);
    Task<IEnumerable<IngresoResumenDto>> SearchAsync(DateTime? desde, DateTime? hasta, string? lote, int? libroId);
}


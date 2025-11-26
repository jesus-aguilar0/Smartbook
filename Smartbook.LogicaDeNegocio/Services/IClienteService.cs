using Smartbook.LogicaDeNegocio.Dtos;

namespace Smartbook.LogicaDeNegocio.Services;

public interface IClienteService
{
    Task<ClienteDto> CreateAsync(ClienteCreateDto dto);
    Task<ClienteDto?> GetByIdAsync(int id);
    Task<ClienteDto?> GetByIdentificacionAsync(string identificacion);
    Task<IEnumerable<ClienteResumenDto>> SearchAsync(string? nombres);
    Task<ClienteDto> UpdateAsync(string identificacion, ClienteUpdateDto dto);
}


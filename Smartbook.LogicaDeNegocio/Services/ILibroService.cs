using Smartbook.LogicaDeNegocio.Dtos;

namespace Smartbook.LogicaDeNegocio.Services;

public interface ILibroService
{
    Task<LibroDto> CreateAsync(LibroCreateDto dto);
    Task<LibroDto?> GetByIdAsync(int id);
    Task<IEnumerable<LibroResumenDto>> SearchAsync(string? nombre, string? nivel, int? tipo, string? editorial, string? edicion);
    Task<LibroDto> UpdateAsync(int id, LibroUpdateDto dto);
    Task SincronizarStockAsync(int libroId);
}


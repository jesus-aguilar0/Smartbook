using Smartbook.LogicaDeNegocio.Dtos;

namespace Smartbook.LogicaDeNegocio.Services;

public interface IInventarioService
{
    Task<IEnumerable<InventarioDto>> GetByLoteAsync(string lote);
    Task<IEnumerable<InventarioDto>> GetByLibroIdAsync(int libroId);
}


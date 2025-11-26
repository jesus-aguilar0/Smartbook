using Smartbook.Entidades;

namespace Smartbook.Persistencia.Repositories;

public interface IInventarioRepository : IRepository<Inventario>
{
    Task<IEnumerable<Inventario>> GetByLoteAsync(string lote);
    Task<Inventario?> GetByLibroAndLoteAsync(int libroId, string lote);
    Task<IEnumerable<Inventario>> GetByLibroIdAsync(int libroId);
    Task<IEnumerable<Inventario>> GetByLibroIdWithStockAsync(int libroId);
    Task<Inventario?> GetAvailableLoteForBookAsync(int libroId, int unidadesRequeridas);
    Task<int> GetStockTotalByLibroIdAsync(int libroId);
}


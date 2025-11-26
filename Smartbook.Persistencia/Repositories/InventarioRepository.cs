using Microsoft.EntityFrameworkCore;
using Smartbook.Entidades;
using Smartbook.Persistencia.Data;

namespace Smartbook.Persistencia.Repositories;

public class InventarioRepository : Repository<Inventario>, IInventarioRepository
{
    public InventarioRepository(SmartbookDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Inventario>> GetByLoteAsync(string lote)
    {
        return await _dbSet
            .Include(i => i.Libro)
            .Where(i => i.Lote == lote)
            .ToListAsync();
    }

    public async Task<Inventario?> GetByLibroAndLoteAsync(int libroId, string lote)
    {
        return await _dbSet
            .Include(i => i.Libro)
            .FirstOrDefaultAsync(i => i.LibroId == libroId && i.Lote == lote);
    }

    public async Task<IEnumerable<Inventario>> GetByLibroIdAsync(int libroId)
    {
        return await _dbSet
            .Include(i => i.Libro)
            .Where(i => i.LibroId == libroId)
            .OrderByDescending(i => i.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inventario>> GetByLibroIdWithStockAsync(int libroId)
    {
        return await _dbSet
            .Include(i => i.Libro)
            .Where(i => i.LibroId == libroId && i.UnidadesDisponibles > 0)
            .OrderByDescending(i => i.FechaCreacion)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene el stock total disponible de un libro sumando todas las unidades disponibles de todos los lotes.
    /// No incluye las unidades vendidas, solo las disponibles para venta.
    /// </summary>
    public async Task<int> GetStockTotalByLibroIdAsync(int libroId)
    {
        return await _dbSet
            .Where(i => i.LibroId == libroId)
            .SumAsync(i => i.UnidadesDisponibles);
    }

    public async Task<Inventario?> GetAvailableLoteForBookAsync(int libroId, int unidadesRequeridas)
    {
        return await _dbSet
            .Include(i => i.Libro)
            .Where(i => i.LibroId == libroId && i.UnidadesDisponibles >= unidadesRequeridas)
            .OrderByDescending(i => i.FechaCreacion)
            .FirstOrDefaultAsync();
    }
}


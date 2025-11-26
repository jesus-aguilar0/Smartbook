using Microsoft.EntityFrameworkCore;
using Smartbook.Entidades;
using Smartbook.Persistencia.Data;

namespace Smartbook.Persistencia.Repositories;

public class IngresoRepository : Repository<Ingreso>, IIngresoRepository
{
    public IngresoRepository(SmartbookDbContext context) : base(context)
    {
    }

    public override async Task<Ingreso?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(i => i.Libro)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<Ingreso>> SearchAsync(DateTime? desde, DateTime? hasta, string? lote, int? libroId)
    {
        var query = _dbSet.Include(i => i.Libro).AsQueryable();

        if (desde.HasValue)
        {
            query = query.Where(i => i.Fecha >= desde.Value);
        }

        if (hasta.HasValue)
        {
            query = query.Where(i => i.Fecha <= hasta.Value);
        }

        if (!string.IsNullOrWhiteSpace(lote))
        {
            query = query.Where(i => i.Lote == lote);
        }

        if (libroId.HasValue)
        {
            query = query.Where(i => i.LibroId == libroId.Value);
        }

        return await query.OrderByDescending(i => i.Fecha).ToListAsync();
    }

    public async Task<string> GetNextLoteNumberAsync(int year)
    {
        // Buscar si existe el período 1 del año actual
        var lote1 = await _dbSet
            .Where(i => i.Lote == $"{year}-1")
            .FirstOrDefaultAsync();

        // Buscar si existe el período 2 del año actual
        var lote2 = await _dbSet
            .Where(i => i.Lote == $"{year}-2")
            .FirstOrDefaultAsync();

        // Si no existe ningún lote del año actual, retornar período 1
        if (lote1 == null && lote2 == null)
        {
            return $"{year}-1";
        }

        // Si existe período 1 pero no período 2, retornar período 2
        if (lote1 != null && lote2 == null)
        {
            return $"{year}-2";
        }

        // Si existe período 2, retornar período 1 del siguiente año
        if (lote2 != null)
        {
            return $"{year + 1}-1";
        }

        // Por defecto, retornar período 1 del año actual
        return $"{year}-1";
    }
}


using Microsoft.EntityFrameworkCore;
using Smartbook.Entidades;
using Smartbook.Persistencia.Data;

namespace Smartbook.Persistencia.Repositories;

public class ClienteRepository : Repository<Cliente>, IClienteRepository
{
    public ClienteRepository(SmartbookDbContext context) : base(context)
    {
    }

    public async Task<Cliente?> GetByIdentificacionAsync(string identificacion)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Identificacion == identificacion);
    }

    public async Task<IEnumerable<Cliente>> SearchAsync(string? nombres)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(nombres))
        {
            query = query.Where(c => c.Nombres.Contains(nombres));
        }

        return await query.ToListAsync();
    }
}


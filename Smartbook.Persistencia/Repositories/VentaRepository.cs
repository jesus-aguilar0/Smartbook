using Microsoft.EntityFrameworkCore;
using Smartbook.Entidades;
using Smartbook.Persistencia.Data;

namespace Smartbook.Persistencia.Repositories;

public class VentaRepository : Repository<Venta>, IVentaRepository
{
    public VentaRepository(SmartbookDbContext context) : base(context)
    {
    }

    public async Task<Venta?> GetWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(v => v.Cliente)
            .Include(v => v.Usuario)
            .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.Libro)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<IEnumerable<Venta>> SearchAsync(DateTime? desde, DateTime? hasta, string? clienteIdentificacion, int? libroId)
    {
        var query = _dbSet
            .Include(v => v.Cliente)
            .Include(v => v.Usuario)
            .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.Libro)
            .AsQueryable();

        if (desde.HasValue)
        {
            query = query.Where(v => v.Fecha >= desde.Value);
        }

        if (hasta.HasValue)
        {
            query = query.Where(v => v.Fecha <= hasta.Value);
        }

        if (!string.IsNullOrWhiteSpace(clienteIdentificacion))
        {
            query = query.Where(v => v.Cliente.Identificacion == clienteIdentificacion);
        }

        if (libroId.HasValue)
        {
            query = query.Where(v => v.DetallesVenta.Any(d => d.LibroId == libroId.Value));
        }

        return await query.OrderByDescending(v => v.Fecha).ToListAsync();
    }
}


using Microsoft.EntityFrameworkCore;
using Smartbook.Entidades;
using Smartbook.Entidades.Enums;
using Smartbook.Persistencia.Data;

namespace Smartbook.Persistencia.Repositories;

public class LibroRepository : Repository<Libro>, ILibroRepository
{
    public LibroRepository(SmartbookDbContext context) : base(context)
    {
    }

    public async Task<Libro?> GetByUniqueKeyAsync(string nombre, string nivel, TipoLibro tipo, string edicion)
    {
        return await _dbSet.FirstOrDefaultAsync(l => 
            l.Nombre == nombre && 
            l.Nivel == nivel && 
            l.Tipo == tipo && 
            l.Edicion == edicion);
    }

    public async Task<IEnumerable<Libro>> SearchAsync(string? nombre, string? nivel, TipoLibro? tipo, string? editorial, string? edicion)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            query = query.Where(l => l.Nombre.Contains(nombre));
        }

        if (!string.IsNullOrWhiteSpace(nivel))
        {
            query = query.Where(l => l.Nivel == nivel);
        }

        if (tipo.HasValue)
        {
            query = query.Where(l => l.Tipo == tipo.Value);
        }

        if (!string.IsNullOrWhiteSpace(editorial))
        {
            query = query.Where(l => l.Editorial.Contains(editorial));
        }

        if (!string.IsNullOrWhiteSpace(edicion))
        {
            query = query.Where(l => l.Edicion == edicion);
        }

        return await query.ToListAsync();
    }
}


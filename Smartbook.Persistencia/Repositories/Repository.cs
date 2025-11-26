using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Smartbook.Entidades;
using Smartbook.Persistencia.Data;

namespace Smartbook.Persistencia.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly SmartbookDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(SmartbookDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        // Usar FirstOrDefaultAsync en lugar de FindAsync para mejor compatibilidad con MySQL
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual Task UpdateAsync(T entity)
    {
        // Verificar si la entidad ya está siendo rastreada
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            // Si no está siendo rastreada, usar Update que marca todas las propiedades como modificadas
            _dbSet.Update(entity);
        }
        else
        {
            // Si ya está siendo rastreada, marcar como modificada
            entry.State = EntityState.Modified;
            
            // Obtener las propiedades de la clave primaria para excluirlas
            var primaryKey = entry.Metadata.FindPrimaryKey();
            var primaryKeyProperties = primaryKey?.Properties.Select(p => p.Name).ToHashSet() ?? new HashSet<string>();
            
            // Marcar explícitamente todas las propiedades como modificadas, excepto las de la clave primaria
            // Esto es importante para propiedades nullable que se establecen a null
            foreach (var property in entry.Properties)
            {
                // No marcar como modificada si es parte de la clave primaria
                if (!primaryKeyProperties.Contains(property.Metadata.Name))
                {
                    property.IsModified = true;
                }
            }
        }
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }
}


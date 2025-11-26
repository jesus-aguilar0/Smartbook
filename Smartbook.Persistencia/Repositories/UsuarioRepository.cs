using Microsoft.EntityFrameworkCore;
using Smartbook.Entidades;
using Smartbook.Persistencia.Data;

namespace Smartbook.Persistencia.Repositories;

public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(SmartbookDbContext context) : base(context)
    {
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<Usuario?> GetByIdentificacionAsync(string identificacion)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Identificacion == identificacion);
    }

    public async Task<Usuario?> GetByTokenConfirmacionAsync(string token)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.TokenConfirmacion == token && 
            u.TokenConfirmacionExpiracion.HasValue && 
            u.TokenConfirmacionExpiracion.Value > DateTime.UtcNow);
    }

    public async Task<Usuario?> GetByTokenResetPasswordAsync(string token)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.TokenResetPassword == token && 
            u.TokenResetPasswordExpiracion.HasValue && 
            u.TokenResetPasswordExpiracion.Value > DateTime.UtcNow);
    }

    public async Task<IEnumerable<Usuario>> SearchAsync(string? nombres, int? rol)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(nombres))
        {
            query = query.Where(u => u.Nombres.Contains(nombres));
        }

        if (rol.HasValue)
        {
            query = query.Where(u => (int)u.Rol == rol.Value);
        }

        return await query.ToListAsync();
    }
}


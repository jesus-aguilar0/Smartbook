using Smartbook.Entidades;

namespace Smartbook.Persistencia.Repositories;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetByEmailAsync(string email);
    Task<Usuario?> GetByIdentificacionAsync(string identificacion);
    Task<Usuario?> GetByTokenConfirmacionAsync(string token);
    Task<Usuario?> GetByTokenResetPasswordAsync(string token);
    Task<IEnumerable<Usuario>> SearchAsync(string? nombres, int? rol);
}


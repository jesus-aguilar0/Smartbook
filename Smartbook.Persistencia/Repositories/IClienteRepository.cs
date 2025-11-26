using Smartbook.Entidades;

namespace Smartbook.Persistencia.Repositories;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente?> GetByIdentificacionAsync(string identificacion);
    Task<IEnumerable<Cliente>> SearchAsync(string? nombres);
}


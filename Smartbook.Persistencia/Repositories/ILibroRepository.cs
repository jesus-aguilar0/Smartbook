using Smartbook.Entidades;
using Smartbook.Entidades.Enums;

namespace Smartbook.Persistencia.Repositories;

public interface ILibroRepository : IRepository<Libro>
{
    Task<Libro?> GetByUniqueKeyAsync(string nombre, string nivel, TipoLibro tipo, string edicion);
    Task<IEnumerable<Libro>> SearchAsync(string? nombre, string? nivel, TipoLibro? tipo, string? editorial, string? edicion);
}


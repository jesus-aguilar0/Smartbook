using Smartbook.Entidades;

namespace Smartbook.Persistencia.Repositories;

public interface IIngresoRepository : IRepository<Ingreso>
{
    Task<IEnumerable<Ingreso>> SearchAsync(DateTime? desde, DateTime? hasta, string? lote, int? libroId);
    Task<string> GetNextLoteNumberAsync(int year);
}


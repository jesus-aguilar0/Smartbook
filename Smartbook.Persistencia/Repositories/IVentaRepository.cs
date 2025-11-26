using Smartbook.Entidades;

namespace Smartbook.Persistencia.Repositories;

public interface IVentaRepository : IRepository<Venta>
{
    Task<Venta?> GetWithDetailsAsync(int id);
    Task<IEnumerable<Venta>> SearchAsync(DateTime? desde, DateTime? hasta, string? clienteIdentificacion, int? libroId);
}


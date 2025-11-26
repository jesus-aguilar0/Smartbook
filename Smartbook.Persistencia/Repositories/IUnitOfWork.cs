namespace Smartbook.Persistencia.Repositories;

public interface IUnitOfWork : IDisposable
{
    IClienteRepository Clientes { get; }
    IUsuarioRepository Usuarios { get; }
    ILibroRepository Libros { get; }
    IIngresoRepository Ingresos { get; }
    IVentaRepository Ventas { get; }
    IInventarioRepository Inventarios { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}


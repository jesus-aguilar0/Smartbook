using Microsoft.EntityFrameworkCore.Storage;
using Smartbook.Persistencia.Data;

namespace Smartbook.Persistencia.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly SmartbookDbContext _context;
    private IDbContextTransaction? _transaction;

    private IClienteRepository? _clientes;
    private IUsuarioRepository? _usuarios;
    private ILibroRepository? _libros;
    private IIngresoRepository? _ingresos;
    private IVentaRepository? _ventas;
    private IInventarioRepository? _inventarios;

    public UnitOfWork(SmartbookDbContext context)
    {
        _context = context;
    }

    public IClienteRepository Clientes => _clientes ??= new ClienteRepository(_context);
    public IUsuarioRepository Usuarios => _usuarios ??= new UsuarioRepository(_context);
    public ILibroRepository Libros => _libros ??= new LibroRepository(_context);
    public IIngresoRepository Ingresos => _ingresos ??= new IngresoRepository(_context);
    public IVentaRepository Ventas => _ventas ??= new VentaRepository(_context);
    public IInventarioRepository Inventarios => _inventarios ??= new InventarioRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}


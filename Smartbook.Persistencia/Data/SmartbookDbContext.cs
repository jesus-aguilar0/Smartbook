using Microsoft.EntityFrameworkCore;
using Smartbook.Entidades;

namespace Smartbook.Persistencia.Data;

public class SmartbookDbContext : DbContext
{
    public SmartbookDbContext(DbContextOptions<SmartbookDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Libro> Libros { get; set; }
    public DbSet<Ingreso> Ingresos { get; set; }
    public DbSet<Venta> Ventas { get; set; }
    public DbSet<DetalleVenta> DetallesVenta { get; set; }
    public DbSet<Inventario> Inventarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cliente configurations
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Identificacion).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Celular).IsUnique();
            entity.Property(e => e.Identificacion).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Nombres).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Celular).IsRequired().HasMaxLength(10);
        });

        // Usuario configurations
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Identificacion).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Identificacion).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ContrasenaHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Nombres).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TokenConfirmacion).HasMaxLength(500);
            entity.Property(e => e.TokenResetPassword).HasMaxLength(500);
            // Mapear booleanos a TINYINT(1) para MySQL
            entity.Property(e => e.EmailConfirmado).HasColumnType("tinyint(1)");
            entity.Property(e => e.Activo).HasColumnType("tinyint(1)");
        });

        // Libro configurations
        modelBuilder.Entity<Libro>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Nombre, e.Nivel, e.Tipo, e.Edicion }).IsUnique();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Nivel).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Editorial).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Edicion).IsRequired().HasMaxLength(20);
        });

        // Ingreso configurations
        modelBuilder.Entity<Ingreso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Lote, e.LibroId });
            entity.Property(e => e.Lote).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ValorCompra).HasPrecision(18, 2);
            entity.Property(e => e.ValorVentaPublico).HasPrecision(18, 2);
            entity.HasOne(e => e.Libro)
                .WithMany(l => l.Ingresos)
                .HasForeignKey(e => e.LibroId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Ignorar FechaActualizacion si la columna no existe en la BD
            // TODO: Remover si la columna existe en la base de datos
            entity.Ignore(e => e.FechaActualizacion);
        });

        // Venta configurations
        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.NumeroReciboPago);
            entity.Property(e => e.NumeroReciboPago).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.Observaciones).HasMaxLength(500);
            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Ventas)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.Ventas)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Ignorar FechaActualizacion porque la tabla Ventas no tiene esta columna
            entity.Ignore(e => e.FechaActualizacion);
        });

        // DetalleVenta configurations
        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Lote).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ValorUnitario).HasPrecision(18, 2);
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.HasOne(e => e.Venta)
                .WithMany(v => v.DetallesVenta)
                .HasForeignKey(e => e.VentaId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Libro)
                .WithMany(l => l.DetallesVenta)
                .HasForeignKey(e => e.LibroId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Inventario configurations
        modelBuilder.Entity<Inventario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.LibroId, e.Lote }).IsUnique();
            entity.Property(e => e.Lote).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.Libro)
                .WithMany(l => l.Inventarios)
                .HasForeignKey(e => e.LibroId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.FechaCreacion = DateTime.UtcNow;
                    // No establecer FechaActualizacion en entidades nuevas
                    break;
                case EntityState.Modified:
                    // Actualizar FechaActualizacion solo si la entidad tiene esta propiedad
                    // Las tablas que NO tienen FechaActualizacion: Ventas, Ingresos
                    // Las tablas que SÍ tienen FechaActualizacion: Clientes, Usuarios, Libros, DetallesVenta, Inventarios
                    if (entry.Entity.GetType().Name != "Venta" && entry.Entity.GetType().Name != "Ingreso")
                    {
                        entry.Entity.FechaActualizacion = DateTime.UtcNow;
                    }
                    break;
            }
        }
    }
}


using Mapster;
using Smartbook.LogicaDeNegocio.Extensions;
using Smartbook.LogicaDeNegocio.Dtos;
using Smartbook.Entidades;
using Smartbook.Entidades.Enums;

namespace Smartbook.LogicaDeNegocio.Mapping;

public static class MappingConfig
{
    public static void ConfigureMappings()
    {
        // Cliente mappings
        TypeAdapterConfig<Cliente, ClienteDto>.NewConfig()
            .Map(dest => dest, src => src);

        TypeAdapterConfig<ClienteCreateDto, Cliente>.NewConfig()
            .Map(dest => dest.Nombres, src => src.Nombres.RemoveAccents().Sanitize())
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.FechaCreacion)
            .Ignore(dest => dest.FechaActualizacion)
            .Ignore(dest => dest.Ventas);

        TypeAdapterConfig<ClienteUpdateDto, Cliente>.NewConfig()
            .Map(dest => dest.Nombres, src => src.Nombres.RemoveAccents().Sanitize())
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Identificacion)
            .Ignore(dest => dest.FechaCreacion)
            .Ignore(dest => dest.Ventas);

        TypeAdapterConfig<Cliente, ClienteResumenDto>.NewConfig()
            .Map(dest => dest, src => src);

        // Usuario mappings
        TypeAdapterConfig<Usuario, UsuarioDto>.NewConfig()
            .Map(dest => dest, src => src);

        TypeAdapterConfig<UsuarioCreateDto, Usuario>.NewConfig()
            .Map(dest => dest.Nombres, src => src.Nombres.RemoveAccents().Sanitize())
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.ContrasenaHash)
            .Ignore(dest => dest.EmailConfirmado)
            .Ignore(dest => dest.Activo)
            .Ignore(dest => dest.TokenConfirmacion)
            .Ignore(dest => dest.TokenConfirmacionExpiracion)
            .Ignore(dest => dest.TokenResetPassword)
            .Ignore(dest => dest.TokenResetPasswordExpiracion)
            .Ignore(dest => dest.FechaCreacion)
            .Ignore(dest => dest.FechaActualizacion)
            .Ignore(dest => dest.Ventas);

        TypeAdapterConfig<UsuarioUpdateDto, Usuario>.NewConfig()
            .Map(dest => dest.Nombres, src => src.Nombres.RemoveAccents().Sanitize())
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Identificacion)
            .Ignore(dest => dest.ContrasenaHash)
            .Ignore(dest => dest.EmailConfirmado)
            .Ignore(dest => dest.TokenConfirmacion)
            .Ignore(dest => dest.TokenConfirmacionExpiracion)
            .Ignore(dest => dest.TokenResetPassword)
            .Ignore(dest => dest.TokenResetPasswordExpiracion)
            .Ignore(dest => dest.FechaCreacion)
            .Ignore(dest => dest.Ventas);

        TypeAdapterConfig<Usuario, UsuarioResumenDto>.NewConfig()
            .Map(dest => dest, src => src);

        // Libro mappings
        TypeAdapterConfig<Libro, LibroDto>.NewConfig()
            .Map(dest => dest, src => src);

        TypeAdapterConfig<LibroCreateDto, Libro>.NewConfig()
            .Map(dest => dest.Nombre, src => src.Nombre.Sanitize())
            .Map(dest => dest.Nivel, src => src.Nivel.Sanitize())
            .Map(dest => dest.Editorial, src => src.Editorial.Sanitize())
            .Map(dest => dest.Edicion, src => src.Edicion.Sanitize())
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Stock)
            .Ignore(dest => dest.FechaCreacion)
            .Ignore(dest => dest.FechaActualizacion)
            .Ignore(dest => dest.Ingresos)
            .Ignore(dest => dest.DetallesVenta)
            .Ignore(dest => dest.Inventarios);

        TypeAdapterConfig<LibroUpdateDto, Libro>.NewConfig()
            .Map(dest => dest.Nombre, src => src.Nombre.Sanitize())
            .Map(dest => dest.Nivel, src => src.Nivel.Sanitize())
            .Map(dest => dest.Editorial, src => src.Editorial.Sanitize())
            .Map(dest => dest.Edicion, src => src.Edicion.Sanitize())
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Stock)
            .Ignore(dest => dest.FechaCreacion)
            .Ignore(dest => dest.Ingresos)
            .Ignore(dest => dest.DetallesVenta)
            .Ignore(dest => dest.Inventarios);

        TypeAdapterConfig<Libro, LibroResumenDto>.NewConfig()
            .Map(dest => dest, src => src);

        // Ingreso mappings
        TypeAdapterConfig<Ingreso, IngresoDto>.NewConfig()
            .Map(dest => dest.LibroNombre, src => src.Libro != null ? src.Libro.Nombre : string.Empty)
            .Map(dest => dest.Total, src => src.Unidades * src.ValorCompra);

        TypeAdapterConfig<IngresoCreateDto, Ingreso>.NewConfig()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Fecha)
            .Ignore(dest => dest.Lote)
            .Ignore(dest => dest.FechaCreacion)
            .Ignore(dest => dest.Libro);

        TypeAdapterConfig<Ingreso, IngresoResumenDto>.NewConfig()
            .Map(dest => dest.LibroNombre, src => src.Libro != null ? src.Libro.Nombre : string.Empty)
            .Map(dest => dest.Total, src => src.Unidades * src.ValorCompra);

        // Venta mappings
        TypeAdapterConfig<Venta, VentaDto>.NewConfig()
            .Map(dest => dest.ClienteNombre, src => src.Cliente != null ? src.Cliente.Nombres : string.Empty)
            .Map(dest => dest.ClienteIdentificacion, src => src.Cliente != null ? src.Cliente.Identificacion : string.Empty)
            .Map(dest => dest.UsuarioNombre, src => src.Usuario != null ? src.Usuario.Nombres : string.Empty)
            .Map(dest => dest.Detalles, src => src.DetallesVenta);

        TypeAdapterConfig<DetalleVenta, DetalleVentaDto>.NewConfig()
            .Map(dest => dest.LibroNombre, src => src.Libro != null ? src.Libro.Nombre : string.Empty);

        TypeAdapterConfig<DetalleVentaCreateDto, DetalleVenta>.NewConfig()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.VentaId)
            .Ignore(dest => dest.ValorUnitario)
            .Ignore(dest => dest.Subtotal)
            .Ignore(dest => dest.FechaCreacion)
            .Ignore(dest => dest.FechaActualizacion)
            .Ignore(dest => dest.Venta)
            .Ignore(dest => dest.Libro);

        // Inventario mappings
        TypeAdapterConfig<Inventario, InventarioDto>.NewConfig()
            .Map(dest => dest.LibroNombre, src => src.Libro != null ? src.Libro.Nombre : string.Empty)
            .Map(dest => dest.Nivel, src => src.Libro != null ? src.Libro.Nivel : string.Empty)
            .Map(dest => dest.Tipo, src => src.Libro != null ? src.Libro.Tipo.ToString() : string.Empty)
            .Map(dest => dest.Editorial, src => src.Libro != null ? src.Libro.Editorial : string.Empty)
            .Map(dest => dest.Edicion, src => src.Libro != null ? src.Libro.Edicion : string.Empty)
            .Map(dest => dest.TotalUnidades, src => src.UnidadesDisponibles + src.UnidadesVendidas);
    }
}


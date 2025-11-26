using Mapster;
using Microsoft.Extensions.Logging;
using Smartbook.LogicaDeNegocio.Dtos;
using Smartbook.LogicaDeNegocio.Exceptions;
using Smartbook.Persistencia.Repositories;

namespace Smartbook.LogicaDeNegocio.Services;

public class IngresoService : IIngresoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IngresoService> _logger;

    public IngresoService(IUnitOfWork unitOfWork, ILogger<IngresoService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IngresoDto> CreateAsync(IngresoCreateDto dto)
    {
        if (dto.ValorCompra <= 0)
        {
            throw new BusinessException("El valor de compra debe ser mayor que 0.");
        }

        if (dto.ValorVentaPublico <= 0)
        {
            throw new BusinessException("El valor de venta al público debe ser mayor que 0.");
        }

        if (dto.Unidades <= 0)
        {
            throw new BusinessException("Las unidades deben ser mayores que 0.");
        }

        var libro = await _unitOfWork.Libros.GetByIdAsync(dto.LibroId);
        if (libro == null)
        {
            throw new BusinessException("Libro no encontrado.");
        }

        // Generar número de lote
        var year = DateTime.Now.Year;
        var lote = await _unitOfWork.Ingresos.GetNextLoteNumberAsync(year);

        var ingreso = dto.Adapt<Smartbook.Entidades.Ingreso>();
        ingreso.Fecha = DateTime.UtcNow;
        ingreso.Lote = lote;

        await _unitOfWork.Ingresos.AddAsync(ingreso);

        // Actualizar stock del libro
        libro.Stock += dto.Unidades;

        // Crear o actualizar inventario por lote
        var inventario = await _unitOfWork.Inventarios.GetByLibroAndLoteAsync(dto.LibroId, lote);
        if (inventario == null)
        {
            inventario = new Smartbook.Entidades.Inventario
            {
                LibroId = dto.LibroId,
                Lote = lote,
                UnidadesDisponibles = dto.Unidades,
                UnidadesVendidas = 0
            };
            await _unitOfWork.Inventarios.AddAsync(inventario);
        }
        else
        {
            inventario.UnidadesDisponibles += dto.Unidades;
            await _unitOfWork.Inventarios.UpdateAsync(inventario);
        }

        await _unitOfWork.Libros.UpdateAsync(libro);
        await _unitOfWork.SaveChangesAsync();

        // Sincronizar stock del libro con inventarios reales después de crear el ingreso
        var stockReal = await _unitOfWork.Inventarios.GetStockTotalByLibroIdAsync(dto.LibroId);
        if (libro.Stock != stockReal)
        {
            _logger.LogWarning("Sincronizando stock después de ingreso. Libro {LibroId}, Stock calculado: {StockCalculado}, Stock real: {StockReal}",
                dto.LibroId, libro.Stock, stockReal);
            libro.Stock = stockReal;
            await _unitOfWork.Libros.UpdateAsync(libro);
            await _unitOfWork.SaveChangesAsync();
        }

        _logger.LogInformation("Ingreso creado: Lote {Lote}, Libro {LibroId}, Unidades {Unidades}", lote, dto.LibroId, dto.Unidades);

        // Recargar el ingreso con el Libro incluido para asegurar que el mapeo funcione
        var ingresoCompleto = await _unitOfWork.Ingresos.GetByIdAsync(ingreso.Id);
        if (ingresoCompleto == null)
        {
            // Si no se puede recargar, asignar manualmente el Libro
            ingreso.Libro = libro;
            ingresoCompleto = ingreso;
        }

        var ingresoDto = ingresoCompleto.Adapt<IngresoDto>();
        // Asegurar que LibroNombre se mapee correctamente
        if (ingresoCompleto.Libro != null)
        {
            ingresoDto.LibroNombre = ingresoCompleto.Libro.Nombre;
        }
        return ingresoDto;
    }

    public async Task<IngresoDto?> GetByIdAsync(int id)
    {
        var ingreso = await _unitOfWork.Ingresos.GetByIdAsync(id);
        if (ingreso == null) return null;

        var dto = ingreso.Adapt<IngresoDto>();
        // Asegurar que LibroNombre se mapee correctamente
        if (ingreso.Libro != null)
        {
            dto.LibroNombre = ingreso.Libro.Nombre;
        }
        return dto;
    }

    public async Task<IEnumerable<IngresoResumenDto>> SearchAsync(DateTime? desde, DateTime? hasta, string? lote, int? libroId)
    {
        var ingresos = await _unitOfWork.Ingresos.SearchAsync(desde, hasta, lote, libroId);
        var dtos = new List<IngresoResumenDto>();
        
        foreach (var ingreso in ingresos)
        {
            var dto = ingreso.Adapt<IngresoResumenDto>();
            // Asegurar que LibroNombre se mapee correctamente
            if (ingreso.Libro != null)
            {
                dto.LibroNombre = ingreso.Libro.Nombre;
            }
            dtos.Add(dto);
        }
        
        return dtos;
    }
}


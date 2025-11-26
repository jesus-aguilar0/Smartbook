using Mapster;
using Microsoft.Extensions.Logging;
using Smartbook.LogicaDeNegocio.Extensions;
using Smartbook.LogicaDeNegocio.Dtos;
using Smartbook.Entidades.Enums;
using Smartbook.LogicaDeNegocio.Exceptions;
using Smartbook.Persistencia.Repositories;

namespace Smartbook.LogicaDeNegocio.Services;

public class LibroService : ILibroService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LibroService> _logger;

    public LibroService(IUnitOfWork unitOfWork, ILogger<LibroService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<LibroDto> CreateAsync(LibroCreateDto dto)
    {
        var tipoLibro = (TipoLibro)dto.Tipo;
        var libroExistente = await _unitOfWork.Libros.GetByUniqueKeyAsync(
            dto.Nombre.Sanitize(),
            dto.Nivel.Sanitize(),
            tipoLibro,
            dto.Edicion.Sanitize()
        );

        if (libroExistente != null)
        {
            throw new BusinessException("Ya existe un libro con el mismo nombre, nivel, tipo y edición.");
        }

        // Validar que el stock no sea negativo
        if (dto.Stock < 0)
        {
            throw new BusinessException("El stock no puede ser negativo.");
        }

        var libro = dto.Adapt<Smartbook.Entidades.Libro>();
        libro.Nombre = dto.Nombre.Sanitize();
        libro.Nivel = dto.Nivel.Sanitize();
        libro.Editorial = dto.Editorial.Sanitize();
        libro.Edicion = dto.Edicion.Sanitize();
        libro.Stock = dto.Stock; // Usar el stock proporcionado

        await _unitOfWork.Libros.AddAsync(libro);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Libro creado: {Nombre}", libro.Nombre);

        return libro.Adapt<LibroDto>();
    }

    public async Task<LibroDto?> GetByIdAsync(int id)
    {
        var libro = await _unitOfWork.Libros.GetByIdAsync(id);
        if (libro == null) return null;

        // Sincronizar stock del libro con inventarios reales
        var stockReal = await _unitOfWork.Inventarios.GetStockTotalByLibroIdAsync(id);
        if (libro.Stock != stockReal)
        {
            _logger.LogWarning("Stock desincronizado para libro {LibroId} ({Nombre}). Stock en Libros: {StockLibro}, Stock real en Inventarios: {StockReal}. Sincronizando...", 
                id, libro.Nombre, libro.Stock, stockReal);
            libro.Stock = stockReal;
            await _unitOfWork.Libros.UpdateAsync(libro);
            await _unitOfWork.SaveChangesAsync();
        }

        return libro.Adapt<LibroDto>();
    }

    public async Task<IEnumerable<LibroResumenDto>> SearchAsync(string? nombre, string? nivel, int? tipo, string? editorial, string? edicion)
    {
        TipoLibro? tipoLibro = tipo.HasValue ? (TipoLibro?)tipo.Value : null;
        var libros = await _unitOfWork.Libros.SearchAsync(nombre, nivel, tipoLibro, editorial, edicion);
        
        // Sincronizar stock de todos los libros encontrados
        var librosList = libros.ToList();
        bool necesitaGuardar = false;
        
        foreach (var libro in librosList)
        {
            var stockReal = await _unitOfWork.Inventarios.GetStockTotalByLibroIdAsync(libro.Id);
            if (libro.Stock != stockReal)
            {
                _logger.LogInformation("Sincronizando stock en búsqueda para libro {LibroId} ({Nombre}). Stock anterior: {StockAnterior}, Stock real: {StockReal}", 
                    libro.Id, libro.Nombre, libro.Stock, stockReal);
                libro.Stock = stockReal;
                await _unitOfWork.Libros.UpdateAsync(libro);
                necesitaGuardar = true;
            }
        }
        
        if (necesitaGuardar)
        {
            await _unitOfWork.SaveChangesAsync();
        }
        
        return librosList.Adapt<IEnumerable<LibroResumenDto>>();
    }

    public async Task<LibroDto> UpdateAsync(int id, LibroUpdateDto dto)
    {
        var libro = await _unitOfWork.Libros.GetByIdAsync(id);
        if (libro == null)
        {
            throw new BusinessException("Libro no encontrado.");
        }

        var tipoLibro = (TipoLibro)dto.Tipo;
        var libroExistente = await _unitOfWork.Libros.GetByUniqueKeyAsync(
            dto.Nombre.Sanitize(),
            dto.Nivel.Sanitize(),
            tipoLibro,
            dto.Edicion.Sanitize()
        );

        if (libroExistente != null && libroExistente.Id != id)
        {
            throw new BusinessException("Ya existe un libro con el mismo nombre, nivel, tipo y edición.");
        }

        libro.Nombre = dto.Nombre.Sanitize();
        libro.Nivel = dto.Nivel.Sanitize();
        libro.Tipo = tipoLibro;
        libro.Editorial = dto.Editorial.Sanitize();
        libro.Edicion = dto.Edicion.Sanitize();
        
        // Actualizar stock si se proporciona
        if (dto.Stock.HasValue)
        {
            if (dto.Stock.Value < 0)
            {
                throw new BusinessException("El stock no puede ser negativo.");
            }
            libro.Stock = dto.Stock.Value;
        }

        await _unitOfWork.Libros.UpdateAsync(libro);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Libro actualizado: {Id}", libro.Id);

        return libro.Adapt<LibroDto>();
    }

    public async Task SincronizarStockAsync(int libroId)
    {
        var libro = await _unitOfWork.Libros.GetByIdAsync(libroId);
        if (libro == null)
        {
            throw new BusinessException("Libro no encontrado.");
        }

        var stockReal = await _unitOfWork.Inventarios.GetStockTotalByLibroIdAsync(libroId);
        if (libro.Stock != stockReal)
        {
            _logger.LogInformation("Sincronizando stock para libro {LibroId} ({Nombre}). Stock anterior: {StockAnterior}, Stock real: {StockReal}", 
                libroId, libro.Nombre, libro.Stock, stockReal);
            libro.Stock = stockReal;
            await _unitOfWork.Libros.UpdateAsync(libro);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}


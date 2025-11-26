using Mapster;
using Smartbook.LogicaDeNegocio.Dtos;
using Smartbook.Persistencia.Repositories;

namespace Smartbook.LogicaDeNegocio.Services;

public class InventarioService : IInventarioService
{
    private readonly IUnitOfWork _unitOfWork;

    public InventarioService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<InventarioDto>> GetByLoteAsync(string lote)
    {
        var inventarios = await _unitOfWork.Inventarios.GetByLoteAsync(lote);
        var dtos = new List<InventarioDto>();
        
        foreach (var inventario in inventarios)
        {
            var dto = inventario.Adapt<InventarioDto>();
            // Asegurar que los campos del Libro se mapeen correctamente
            if (inventario.Libro != null)
            {
                dto.LibroNombre = inventario.Libro.Nombre;
                dto.Nivel = inventario.Libro.Nivel;
                dto.Tipo = inventario.Libro.Tipo.ToString();
                dto.Editorial = inventario.Libro.Editorial;
                dto.Edicion = inventario.Libro.Edicion;
            }
            dtos.Add(dto);
        }
        
        return dtos;
    }

    public async Task<IEnumerable<InventarioDto>> GetByLibroIdAsync(int libroId)
    {
        var inventarios = await _unitOfWork.Inventarios.GetByLibroIdAsync(libroId);
        var dtos = new List<InventarioDto>();
        
        foreach (var inventario in inventarios)
        {
            var dto = inventario.Adapt<InventarioDto>();
            // Asegurar que los campos del Libro se mapeen correctamente
            if (inventario.Libro != null)
            {
                dto.LibroNombre = inventario.Libro.Nombre;
                dto.Nivel = inventario.Libro.Nivel;
                dto.Tipo = inventario.Libro.Tipo.ToString();
                dto.Editorial = inventario.Libro.Editorial;
                dto.Edicion = inventario.Libro.Edicion;
            }
            dtos.Add(dto);
        }
        
        return dtos;
    }
}


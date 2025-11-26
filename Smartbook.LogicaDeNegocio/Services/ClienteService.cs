using Mapster;
using Microsoft.Extensions.Logging;
using Smartbook.LogicaDeNegocio.Extensions;
using Smartbook.LogicaDeNegocio.Dtos;
using Smartbook.LogicaDeNegocio.Exceptions;
using Smartbook.LogicaDeNegocio.Validators;
using Smartbook.Persistencia.Repositories;

namespace Smartbook.LogicaDeNegocio.Services;

public class ClienteService : IClienteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClienteService> _logger;

    public ClienteService(IUnitOfWork unitOfWork, ILogger<ClienteService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ClienteDto> CreateAsync(ClienteCreateDto dto)
    {
        // Validar identificación
        var identificacionLimpia = ValidationHelper.CleanIdentification(dto.Identificacion);
        if (string.IsNullOrWhiteSpace(identificacionLimpia))
        {
            throw new BusinessException("La identificación es requerida.");
        }

        if (!ValidationHelper.IsValidColombianId(identificacionLimpia))
        {
            throw new BusinessException("La identificación no es válida. Debe ser una cédula colombiana válida (6-10 dígitos, solo números).");
        }

        // Validar nombres
        if (string.IsNullOrWhiteSpace(dto.Nombres))
        {
            throw new BusinessException("Los nombres son requeridos.");
        }

        if (!ValidationHelper.IsValidName(dto.Nombres))
        {
            throw new BusinessException("Los nombres solo pueden contener letras, espacios y los caracteres especiales permitidos (guiones, apóstrofes, puntos). Mínimo 2 caracteres, máximo 200.");
        }

        // Validar email
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new BusinessException("El correo electrónico es requerido.");
        }

        if (!ValidationHelper.IsValidEmail(dto.Email))
        {
            throw new BusinessException("El formato del correo electrónico no es válido.");
        }

        // Validar celular
        var celularLimpio = ValidationHelper.CleanPhoneNumber(dto.Celular);
        if (string.IsNullOrWhiteSpace(celularLimpio))
        {
            throw new BusinessException("El número de celular es requerido.");
        }

        if (!ValidationHelper.IsValidColombianPhone(celularLimpio))
        {
            throw new BusinessException("El número de celular no es válido. Debe tener 10 dígitos y comenzar con 3 (formato colombiano).");
        }

        // Validar fecha de nacimiento
        if (!ValidationHelper.IsValidBirthDate(dto.FechaNacimiento))
        {
            throw new BusinessException("La fecha de nacimiento no es válida. No puede ser una fecha futura ni anterior a 1900.");
        }

        var edad = ValidationHelper.CalculateAge(dto.FechaNacimiento);
        if (edad < 14)
        {
            throw new BusinessException("No se pueden registrar clientes menores de 14 años.");
        }

        // Validar duplicados
        if (await _unitOfWork.Clientes.ExistsAsync(c => c.Identificacion == identificacionLimpia))
        {
            throw new BusinessException("Ya existe un cliente con esta identificación.");
        }

        if (await _unitOfWork.Clientes.ExistsAsync(c => c.Email == dto.Email.ToLowerInvariant().Trim()))
        {
            throw new BusinessException("Ya existe un cliente con este correo electrónico.");
        }

        if (await _unitOfWork.Clientes.ExistsAsync(c => c.Celular == celularLimpio))
        {
            throw new BusinessException("Ya existe un cliente con este número de celular.");
        }

        var cliente = dto.Adapt<Smartbook.Entidades.Cliente>();
        cliente.Identificacion = identificacionLimpia;
        cliente.Nombres = dto.Nombres.RemoveAccents().Sanitize();
        cliente.Email = dto.Email.ToLowerInvariant().Trim();
        cliente.Celular = celularLimpio;

        await _unitOfWork.Clientes.AddAsync(cliente);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Cliente creado: {Identificacion}", cliente.Identificacion);

        return cliente.Adapt<ClienteDto>();
    }

    public async Task<ClienteDto?> GetByIdAsync(int id)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
        return cliente?.Adapt<ClienteDto>();
    }

    public async Task<ClienteDto?> GetByIdentificacionAsync(string identificacion)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdentificacionAsync(identificacion);
        return cliente?.Adapt<ClienteDto>();
    }

    public async Task<IEnumerable<ClienteResumenDto>> SearchAsync(string? nombres)
    {
        var clientes = await _unitOfWork.Clientes.SearchAsync(nombres);
        return clientes.Adapt<IEnumerable<ClienteResumenDto>>();
    }

    public async Task<ClienteDto> UpdateAsync(string identificacion, ClienteUpdateDto dto)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdentificacionAsync(identificacion);
        if (cliente == null)
        {
            throw new BusinessException("Cliente no encontrado.");
        }

        // Validar nombres
        if (string.IsNullOrWhiteSpace(dto.Nombres))
        {
            throw new BusinessException("Los nombres son requeridos.");
        }

        if (!ValidationHelper.IsValidName(dto.Nombres))
        {
            throw new BusinessException("Los nombres solo pueden contener letras, espacios y los caracteres especiales permitidos (guiones, apóstrofes, puntos). Mínimo 2 caracteres, máximo 200.");
        }

        // Validar email
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new BusinessException("El correo electrónico es requerido.");
        }

        if (!ValidationHelper.IsValidEmail(dto.Email))
        {
            throw new BusinessException("El formato del correo electrónico no es válido.");
        }

        // Validar celular
        var celularLimpio = ValidationHelper.CleanPhoneNumber(dto.Celular);
        if (string.IsNullOrWhiteSpace(celularLimpio))
        {
            throw new BusinessException("El número de celular es requerido.");
        }

        if (!ValidationHelper.IsValidColombianPhone(celularLimpio))
        {
            throw new BusinessException("El número de celular no es válido. Debe tener 10 dígitos y comenzar con 3 (formato colombiano).");
        }

        // Validar fecha de nacimiento
        if (!ValidationHelper.IsValidBirthDate(dto.FechaNacimiento))
        {
            throw new BusinessException("La fecha de nacimiento no es válida. No puede ser una fecha futura ni anterior a 1900.");
        }

        var edad = ValidationHelper.CalculateAge(dto.FechaNacimiento);
        if (edad < 14)
        {
            throw new BusinessException("No se pueden registrar clientes menores de 14 años.");
        }

        // Validar email único si cambió
        var emailNormalizado = dto.Email.ToLowerInvariant().Trim();
        if (cliente.Email != emailNormalizado && await _unitOfWork.Clientes.ExistsAsync(c => c.Email == emailNormalizado))
        {
            throw new BusinessException("Ya existe un cliente con este correo electrónico.");
        }

        // Validar celular único si cambió
        if (cliente.Celular != celularLimpio && await _unitOfWork.Clientes.ExistsAsync(c => c.Celular == celularLimpio))
        {
            throw new BusinessException("Ya existe un cliente con este número de celular.");
        }

        cliente.Nombres = dto.Nombres.RemoveAccents().Sanitize();
        cliente.Email = emailNormalizado;
        cliente.Celular = celularLimpio;
        cliente.FechaNacimiento = dto.FechaNacimiento;

        await _unitOfWork.Clientes.UpdateAsync(cliente);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Cliente actualizado: {Identificacion}", cliente.Identificacion);

        return cliente.Adapt<ClienteDto>();
    }

}


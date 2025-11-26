using Smartbook.LogicaDeNegocio.Dtos;

namespace Smartbook.LogicaDeNegocio.Services;

public interface IUsuarioService
{
    Task<UsuarioDto> CreateAsync(UsuarioCreateDto dto, string baseUrl);
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
    Task ConfirmEmailAsync(string token);
    Task RequestPasswordResetAsync(ResetPasswordDto dto, string baseUrl);
    Task ResetPasswordAsync(ConfirmPasswordResetDto dto);
    Task<IEnumerable<UsuarioResumenDto>> SearchAsync(string? nombres, int? rol);
    Task<UsuarioDto> UpdateAsync(int id, UsuarioUpdateDto dto, int currentUserId);
    Task<UsuarioDto?> GetByIdAsync(int id);
}


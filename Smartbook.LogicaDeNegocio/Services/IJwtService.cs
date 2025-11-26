using Smartbook.Entidades;

namespace Smartbook.LogicaDeNegocio.Services;

public interface IJwtService
{
    string GenerateToken(Usuario usuario);
}


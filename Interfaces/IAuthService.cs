using ApiAuth.DTOs;
using ApiAuth.Responses;

namespace ApiAuth.Interfaces;

public interface IAuthService
{
    Task<bool> RegistrarAsync(RegisterDto dto);
    Task<string?> LoginAsync(LoginDto dto); // Se você alterou o Login para retornar ambos os tokens, adapte o tipo de retorno aqui
    
    // Adicione estes dois:
    Task<object?> RefreshTokenAsync(RefreshTokenDto dto);
    Task<bool> LogoutAsync(RefreshTokenDto dto);
}
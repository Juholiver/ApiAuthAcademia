using ApiAuth.DTOs;
using ApiAuth.Responses;

namespace ApiAuth.Interfaces;

public interface IAuthService
{
    Task<bool> RegistrarAsync(RegisterDto dto);
    
    
    Task<object?> RefreshTokenAsync(RefreshTokenDto dto);
    Task<bool> LogoutAsync(RefreshTokenDto dto);
}
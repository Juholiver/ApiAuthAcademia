using ApiAuth.DTOs;

namespace ApiAuth.Interfaces;

public interface IAuthService
{
    Task<bool> RegistrarAsync(RegisterDto dto);

    Task<string?> LoginAsync(LoginDto dto);
}
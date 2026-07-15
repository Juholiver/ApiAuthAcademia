using ApiAuth.Models;

namespace ApiAuth.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> ObterPorTokenAsync(string token);
    Task CriarAsync(RefreshToken refreshToken);
    Task SalvarAsync();
}
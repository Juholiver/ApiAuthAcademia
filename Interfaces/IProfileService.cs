using ApiAuth.Dtos;

namespace ApiAuth.Services;

public interface IProfileService
{
    Task<ProfileResponseDto?> ObterPerfilPorIdAsync(int id);
}
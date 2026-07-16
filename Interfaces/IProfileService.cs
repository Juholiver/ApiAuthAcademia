using ApiAuth.Dtos;
using ApiAuth.DTOs;

namespace ApiAuth.Services;

public interface IProfileService
{
    Task<ProfileResponseDto?> ObterPerfilPorIdAsync(int id);

    Task<bool> AtualizarPerfilAsync(int usuarioId, UpdateProfileDto dto);
    Task<bool> DeletarPerfilAsync(int usuarioId);
}
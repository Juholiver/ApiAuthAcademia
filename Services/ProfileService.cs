using ApiAuth.Dtos;
using ApiAuth.DTOs;
using ApiAuth.Interfaces;
using ApiAuth.Models;

namespace ApiAuth.Services;

public class ProfileService : IProfileService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ProfileService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    // --- CORREÇÃO DO NOME DO MÉTODO AQUI ---
    public async Task<ProfileResponseDto?> ObterPerfilPorIdAsync(int usuarioId)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuario == null) return null;

        return new ProfileResponseDto
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email
        };
    }

    public async Task<bool> AtualizarPerfilAsync(int usuarioId, UpdateProfileDto dto)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuario == null) return false;

        if (usuario.Email != dto.Email)
        {
            if (await _usuarioRepository.EmailExisteAsync(dto.Email))
                return false; 
        }

        usuario.Nome = dto.Nome;
        usuario.Email = dto.Email;

        _usuarioRepository.Atualizar(usuario);
        await _usuarioRepository.SalvarAsync();

        return true;
    }

    public async Task<bool> DeletarPerfilAsync(int usuarioId)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuario == null) return false;

        _usuarioRepository.Excluir(usuario);
        await _usuarioRepository.SalvarAsync();

        return true;
    }
}
using ApiAuth.Dtos;
using ApiAuth.Interfaces;


namespace ApiAuth.Services;

public class ProfileService : IProfileService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ProfileService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<ProfileResponseDto?> ObterPerfilPorIdAsync(int id)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(id);
        
        if (usuario == null)
            return null;

        return new ProfileResponseDto
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            CriadoEm = usuario.CriadoEm
        };
    }
}
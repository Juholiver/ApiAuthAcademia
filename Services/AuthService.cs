using ApiAuth.DTOs;
using ApiAuth.Interfaces;
using ApiAuth.Models;

namespace ApiAuth.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _repository;
    private readonly TokenService _tokenService;

    public AuthService(
        IUsuarioRepository repository,
        TokenService tokenService)
    {
        _repository = repository;
        _tokenService = tokenService;
    }

    public async Task<bool> RegistrarAsync(RegisterDto dto)
    {
        if (await _repository.EmailExisteAsync(dto.Email))
            return false;

        Usuario usuario = new()
        {
            Nome = dto.Nome,
            Email = dto.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha)
        };

        await _repository.CriarAsync(usuario);

        await _repository.SalvarAsync();

        return true;
    }

    public async Task<string?> LoginAsync(LoginDto dto)
    {
        var usuario =
            await _repository.ObterPorEmailAsync(dto.Email);

        if (usuario == null)
            return null;

        bool senhaValida =
            BCrypt.Net.BCrypt.Verify(
                dto.Senha,
                usuario.SenhaHash);

        if (!senhaValida)
            return null;

        return _tokenService.GerarToken(usuario);
    }
}
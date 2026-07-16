using ApiAuth.DTOs;
using ApiAuth.Interfaces;
using ApiAuth.Models;

namespace ApiAuth.Services;

/// <summary>
/// Serviço responsável pelas operações de autenticação:
/// - Registrar usuário (hash de senha)
/// - Login (validação de credenciais e geração de access token)
/// - Refresh token (validação, revogação e rotação)
/// - Logout (revogação do refresh token)
///
/// Mantenha a lógica de persistência em repositórios e a geração de tokens
/// encapsulada em <see cref="TokenService"/> para facilitar testes.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRefreshTokenRepository _tokenRepository; // <-- Injetado aqui
    private readonly TokenService _tokenService;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IRefreshTokenRepository tokenRepository, // <-- Adicionado ao construtor
        TokenService tokenService)
    {
        _usuarioRepository = usuarioRepository;
        _tokenRepository = tokenRepository;
        _tokenService = tokenService;
    }

    public async Task<bool> RegistrarAsync(RegisterDto dto)
    {
        // Verifica duplicidade de e-mail antes de criar
        if (await _usuarioRepository.EmailExisteAsync(dto.Email))
            return false;

        Usuario usuario = new()
        {
            Nome = dto.Nome,
            Email = dto.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha)
        };

        await _usuarioRepository.CriarAsync(usuario);
        await _usuarioRepository.SalvarAsync();

        return true;
    }

    public async Task<string?> LoginAsync(LoginDto dto)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(dto.Email);

        if (usuario == null)
            return null;

        bool senhaValida = BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash);

        if (!senhaValida)
            return null;

        // Retorna apenas o access token. O refresh token é tratado separadamente.
        return _tokenService.GerarToken(usuario);
    }

    // --- REFRESH TOKEN SEM DBCOUNTEXT DIRETO ---
    public async Task<object?> RefreshTokenAsync(RefreshTokenDto dto)
    {
        if (string.IsNullOrEmpty(dto.RefreshToken))
            return null;

        // 1. Busca usando o repositório correto
        var tokenExistente = await _tokenRepository.ObterPorTokenAsync(dto.RefreshToken);

        // 2. Validações
        if (tokenExistente == null || 
            tokenExistente.ExpiraEm < DateTime.UtcNow || 
            tokenExistente.Revogado)
        {
            return null;
        }

        // 3. Buscar usuário pelo ID (ajuste o método se o seu repositório de usuário tiver outro nome, ex: ObterPorIdAsync)
        var usuario = await _usuarioRepository.ObterPorIdAsync(tokenExistente.UsuarioId);
        if (usuario == null)
            return null;

        // 4. Gerar novos tokens
        var novoAccessToken = _tokenService.GerarToken(usuario);
        var novoRefreshTokenString = _tokenService.GerarRefreshToken();

        // 5. Revogar token atual e persistir novo (rotacionamento)
        tokenExistente.Revogado = true;

        RefreshToken novoRefreshToken = new()
        {
            Token = novoRefreshTokenString,
            UsuarioId = usuario.Id,
            ExpiraEm = DateTime.UtcNow.AddDays(30),
            Revogado = false
        };

        await _tokenRepository.CriarAsync(novoRefreshToken);
        await _tokenRepository.SalvarAsync();

        return new
        {
            accessToken = novoAccessToken,
            refreshToken = novoRefreshTokenString
        };
    }

    // --- LOGOUT SEM DBCONTEXT DIRETO ---
    public async Task<bool> LogoutAsync(RefreshTokenDto dto)
    {
        if (string.IsNullOrEmpty(dto.RefreshToken))
            return false;

        var tokenExistente = await _tokenRepository.ObterPorTokenAsync(dto.RefreshToken);

        if (tokenExistente == null)
            return false;

        tokenExistente.Revogado = true;
        await _tokenRepository.SalvarAsync();

        return true;
    }
}
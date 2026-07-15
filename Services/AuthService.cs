using ApiAuth.Data;          // <-- Importante para achar o AppDbContext
using ApiAuth.DTOs;
using ApiAuth.Interfaces;
using ApiAuth.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiAuth.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _repository;
    private readonly TokenService _tokenService;
    private readonly AppDbContext _context; // <-- Injetado aqui para gerenciar os RefreshTokens

    public AuthService(
        IUsuarioRepository repository,
        TokenService tokenService,
        AppDbContext context) // <-- Adicionado no construtor
    {
        _repository = repository;
        _tokenService = tokenService;
        _context = context;
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
        var usuario = await _repository.ObterPorEmailAsync(dto.Email);

        if (usuario == null)
            return null;

        bool senhaValida = BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash);

        if (!senhaValida)
            return null;

        return _tokenService.GerarToken(usuario);
    }

    // --- IMPLEMENTAÇÃO DO REFRESH TOKEN ---
    public async Task<object?> RefreshTokenAsync(RefreshTokenDto dto)
    {
        if (string.IsNullOrEmpty(dto.RefreshToken))
            return null;

        // 1. Buscar o Refresh Token no banco usando o DbContext
        var tokenExistente = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == dto.RefreshToken);

        // 2. Validar existência, expiração e se foi revogado
        if (tokenExistente == null || 
            tokenExistente.ExpiraEm < DateTime.UtcNow || 
            tokenExistente.Revogado)
        {
            return null;
        }

        // 3. Buscar o usuário
        var usuario = await _repository.ObterPorIdAsync(tokenExistente.UsuarioId); 
        // Nota: Se você não tiver ObterPorIdAsync no seu repository, pode usar:
        // var usuario = await _context.Usuarios.FindAsync(tokenExistente.UsuarioId);
        
        if (usuario == null)
            return null;

        // 4. Gerar novos tokens
        var novoAccessToken = _tokenService.GerarToken(usuario);
        var novoRefreshTokenString = _tokenService.GerarRefreshToken();

        // 5. Revogar o token antigo
        tokenExistente.Revogado = true;

        // 6. Cadastrar o novo Refresh Token
        RefreshToken novoRefreshToken = new()
        {
            Token = novoRefreshTokenString,
            UsuarioId = usuario.Id,
            ExpiraEm = DateTime.UtcNow.AddDays(30),
            Revogado = false
        };

        _context.RefreshTokens.Add(novoRefreshToken);
        await _context.SaveChangesAsync();

        return new
        {
            accessToken = novoAccessToken,
            refreshToken = novoRefreshTokenString
        };
    }

    // --- IMPLEMENTAÇÃO DO LOGOUT ---
    public async Task<bool> LogoutAsync(RefreshTokenDto dto)
    {
        if (string.IsNullOrEmpty(dto.RefreshToken))
            return false;

        var tokenExistente = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == dto.RefreshToken);

        if (tokenExistente == null)
            return false;

        tokenExistente.Revogado = true;
        await _context.SaveChangesAsync();

        return true;
    }
}
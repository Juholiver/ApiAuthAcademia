using ApiAuth.Data;
using ApiAuth.Interfaces;
using ApiAuth.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiAuth.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> ObterPorTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task CriarAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
    }

    public async Task SalvarAsync()
    {
        await _context.SaveChangesAsync();
    }
}
using ApiAuth.Data;
using ApiAuth.Interfaces;
using ApiAuth.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiAuth.Repositories;

public class TreinoRepository : ITreinoRepository
{
    private readonly AppDbContext _context;

    public TreinoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Treino>> ObterTodosPorUsuarioIdAsync(int usuarioId)
    {
        return await _context.Treinos
            .Where(t => t.UsuarioId == usuarioId)
            .OrderBy(t => t.Divisao) // Ordena por A, B, C...
            .ToListAsync();
    }

    public async Task<Treino?> ObterPorIdAsync(int id)
    {
        return await _context.Treinos.FindAsync(id);
    }

    public async Task CriarAsync(Treino treino)
    {
        await _context.Treinos.AddAsync(treino);
    }

    public void Excluir(Treino treino)
    {
        _context.Treinos.Remove(treino);
    }

    public async Task SalvarAsync()
    {
        await _context.SaveChangesAsync();
    }
}
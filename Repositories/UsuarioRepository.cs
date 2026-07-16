using ApiAuth.Data;
using ApiAuth.Interfaces;
using ApiAuth.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiAuth.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<Usuario?> ObterPorIdAsync(int id)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> EmailExisteAsync(string email)
    {
        return await _context.Usuarios
            .AnyAsync(x => x.Email == email);
    }

    public async Task CriarAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
    }

    public async Task SalvarAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Atualizar(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
    }

    public void Excluir(Usuario usuario)
    {
        _context.Usuarios.Remove(usuario);
    }
}
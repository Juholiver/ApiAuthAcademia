using ApiAuth.Models;

namespace ApiAuth.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorEmailAsync(string email);

    Task<Usuario?> ObterPorIdAsync(int id);

    Task<bool> EmailExisteAsync(string email);

    Task CriarAsync(Usuario usuario);

    Task SalvarAsync();
}
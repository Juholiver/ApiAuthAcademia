using ApiAuth.Models;

namespace ApiAuth.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(int id);
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<bool> EmailExisteAsync(string email);
    Task CriarAsync(Usuario usuario);
    
    // Adicione estes:
    void Atualizar(Usuario usuario);
    void Excluir(Usuario usuario);
    Task SalvarAsync();
}
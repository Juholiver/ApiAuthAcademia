using ApiAuth.Models;

namespace ApiAuth.Interfaces;

public interface ITreinoRepository
{
    Task<List<Treino>> ObterTodosPorUsuarioIdAsync(int usuarioId);
    Task<Treino?> ObterPorIdAsync(int id);
    Task CriarAsync(Treino treino);
    void Excluir(Treino treino);
    Task SalvarAsync();
}
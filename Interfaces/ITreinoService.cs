using ApiAuth.DTOs;

namespace ApiAuth.Interfaces;

public interface ITreinoService
{
    // Retorna os treinos agrupados por divisão (A, B, C) para facilitar no frontend!
    Task<Dictionary<string, List<TreinoResponseDto>>> ObterFichaAgrupadaAsync(int usuarioId);
    Task<bool> AdicionarExercicioAsync(int usuarioId, TreinoCriarDto dto);
    Task<bool> RemoverExercicioAsync(int usuarioId, int treinoId);
}
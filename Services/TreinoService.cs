using ApiAuth.DTOs;
using ApiAuth.Interfaces;
using ApiAuth.Models;

namespace ApiAuth.Services;

public class TreinoService : ITreinoService
{
    private readonly ITreinoRepository _treinoRepository;

    public TreinoService(ITreinoRepository treinoRepository)
    {
        _treinoRepository = treinoRepository;
    }

    public async Task<Dictionary<string, List<TreinoResponseDto>>> ObterFichaAgrupadaAsync(int usuarioId)
    {
        var treinos = await _treinoRepository.ObterTodosPorUsuarioIdAsync(usuarioId);

        // Agrupa os treinos por Divisão (A, B, C...) usando LINQ
        return treinos
            .GroupBy(t => t.Divisao.ToUpper())
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.Select(t => new TreinoResponseDto
                {
                    Id = t.Id,
                    ExercicioId = t.ExercicioId, // <-- Adicionado
                    Divisao = t.Divisao.ToUpper(),
                    NomeExercicio = t.NomeExercicio,
                    Series = t.Series,
                    Repeticoes = t.Repeticoes,
                    Carga = t.Carga,
                    Descanso = t.Descanso
                }).ToList()
            );
    }

    public async Task<bool> AdicionarExercicioAsync(int usuarioId, TreinoCriarDto dto)
    {
        Treino novoExercicio = new()
        {
            UsuarioId = usuarioId,
            ExercicioId = dto.ExercicioId,
            Divisao = dto.Divisao.ToUpper(),
            NomeExercicio = dto.NomeExercicio,
            Series = dto.Series,
            Repeticoes = dto.Repeticoes,
            Carga = dto.Carga,
            Descanso = dto.Descanso
        };

        await _treinoRepository.CriarAsync(novoExercicio);
        await _treinoRepository.SalvarAsync();
        return true;
    }

    public async Task<bool> RemoverExercicioAsync(int usuarioId, int treinoId)
    {
        var treino = await _treinoRepository.ObterPorIdAsync(treinoId);
        
        // Garante que o treino existe E pertence ao usuário logado
        if (treino == null || treino.UsuarioId != usuarioId)
            return false;

        _treinoRepository.Excluir(treino);
        await _treinoRepository.SalvarAsync();
        return true;
    }
}
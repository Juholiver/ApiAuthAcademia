using System.Security.Claims;
using ApiAuth.DTOs;
using ApiAuth.Interfaces;
using ApiAuth.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiAuth.Controllers;

/// <summary>
/// Endpoints para gerenciar a ficha de treinos do usuário.
/// - `GET` retorna a ficha agrupada por divisão (A/B/C)
/// - `POST` adiciona exercício
/// - `DELETE` remove exercício se pertencer ao usuário autenticado
/// Requer `Authorization: Bearer {token}`.
/// </summary>
[Authorize]
[ApiController]
[Route("api/treinos")]
public class TreinoController : ControllerBase
{
    private readonly ITreinoService _treinoService;

    public TreinoController(ITreinoService treinoService)
    {
        _treinoService = treinoService;
    }

    // Retorna a ficha inteira agrupada por A, B, C
    [HttpGet]
    public async Task<IActionResult> ObterFicha()
    {
        var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(usuarioIdStr, out int usuarioId))
            return Unauthorized();

        var ficha = await _treinoService.ObterFichaAgrupadaAsync(usuarioId);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Ficha de treinos recuperada.",
            Data = ficha
        });
    }

    // Adiciona um exercício à ficha
    [HttpPost]
    public async Task<IActionResult> AdicionarExercicio(TreinoCriarDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(usuarioIdStr, out int usuarioId))
            return Unauthorized();

        await _treinoService.AdicionarExercicioAsync(usuarioId, dto);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Exercício adicionado com sucesso."
        });
    }

    // Remove um exercício da ficha
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> RemoverExercicio(int id)
    {
        var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(usuarioIdStr, out int usuarioId))
            return Unauthorized();

        var sucesso = await _treinoService.RemoverExercicioAsync(usuarioId, id);

        if (!sucesso)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Exercício não encontrado ou não pertence a você."
            });
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Exercício removido com sucesso da sua ficha."
        });
    }
}
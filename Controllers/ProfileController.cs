using System.Security.Claims;
using ApiAuth.DTOs;
using ApiAuth.Interfaces;
using ApiAuth.Responses;
using ApiAuth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiAuth.Controllers;

/// <summary>
/// Endpoints para gerenciar o perfil do usuário autenticado.
/// Requer o header `Authorization: Bearer {token}`.
/// </summary>
[Authorize]
[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<IActionResult> ObterPerfil()
    {
        var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(usuarioIdStr, out int usuarioId))
            return Unauthorized();

        // Alterado aqui também para bater com a interface:
        var perfil = await _profileService.ObterPerfilPorIdAsync(usuarioId);
        if (perfil == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Perfil não encontrado." });

        return Ok(new ApiResponse<object> { Success = true, Message = "Perfil recuperado.", Data = perfil });
    }

    // --- ENDPOINT UPDATE (PUT) ---
    [HttpPut]
    public async Task<IActionResult> AtualizarPerfil(UpdateProfileDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(usuarioIdStr, out int usuarioId))
            return Unauthorized();

        bool sucesso = await _profileService.AtualizarPerfilAsync(usuarioId, dto);

        if (!sucesso)
        {
            return BadRequest(new ApiResponse<object> 
            { 
                Success = false, 
                Message = "Não foi possível atualizar o perfil. Verifique se o e-mail já está em uso." 
            });
        }

        return Ok(new ApiResponse<object> 
        { 
            Success = true, 
            Message = "Perfil atualizado com sucesso." 
        });
    }

    // --- ENDPOINT DELETE (DELETE) ---
    [HttpDelete]
    public async Task<IActionResult> DeletarPerfil()
    {
        var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(usuarioIdStr, out int usuarioId))
            return Unauthorized();

        bool sucesso = await _profileService.DeletarPerfilAsync(usuarioId);

        if (!sucesso)
        {
            return NotFound(new ApiResponse<object> 
            { 
                Success = false, 
                Message = "Usuário não encontrado para exclusão." 
            });
        }

        return Ok(new ApiResponse<object> 
        { 
            Success = true, 
            Message = "Perfil e conta excluídos com sucesso." 
        });
    }
}
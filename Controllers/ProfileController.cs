using ApiAuth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace ApiAuth.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        // Busca explicitamente pelo "sub" ou pelo NameIdentifier padrão
        var idClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                    ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (idClaim == null)
        {
            // Debug de claims se o ID não for encontrado
            var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}");
            Console.WriteLine("\n--- CLAIMS RECEBIDAS NO CONTROLLER ---");
            Console.WriteLine(string.Join("\n", claims));
            Console.WriteLine("--------------------------------------\n");
            
            return Unauthorized();
        }

        if (!int.TryParse(idClaim.Value, out int usuarioId))
            return Unauthorized();

        var perfil = await _profileService.ObterPerfilPorIdAsync(usuarioId);

        if (perfil == null)
            return NotFound();

        return Ok(perfil);
    }
}
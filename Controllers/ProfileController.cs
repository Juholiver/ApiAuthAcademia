using ApiAuth.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;

namespace ApiAuth.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProfileController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        // Busca explicitamente pelo "sub" ou pelo NameIdentifier padrão
        var idClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) 
                    ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (idClaim == null)
        {
            // Se cair aqui, vamos debugar listando todas as claims que chegaram
            var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}");
            Console.WriteLine("\n--- CLAIMS RECEBIDAS NO CONTROLLER ---");
            Console.WriteLine(string.Join("\n", claims));
            Console.WriteLine("--------------------------------------\n");
            
            return Unauthorized();
        }

        if (!int.TryParse(idClaim.Value, out int usuarioId))
            return Unauthorized();

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId);

        if (usuario == null)
            return NotFound();

        return Ok(new
        {
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.CriadoEm
        });
    }
}
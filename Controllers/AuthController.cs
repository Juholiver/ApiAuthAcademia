using ApiAuth.Data;
using ApiAuth.DTOs;
using ApiAuth.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiAuth.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        bool emailExiste = await _context.Usuarios
            .AnyAsync(u => u.Email == dto.Email);

        if (emailExiste)
            return BadRequest(new
            {
                mensagem = "E-mail já cadastrado."
            });

        var usuario = new Usuario
        {
            Nome = dto.Nome,
            Email = dto.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha)
        };

        _context.Usuarios.Add(usuario);

        await _context.SaveChangesAsync();

        return Created("", new
        {
            mensagem = "Usuário cadastrado com sucesso."
        });
    }
}
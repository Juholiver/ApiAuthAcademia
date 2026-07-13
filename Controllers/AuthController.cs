using ApiAuth.Data;
using ApiAuth.DTOs;
using ApiAuth.Models;
using ApiAuth.Services;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiAuth.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    private readonly TokenService _tokenService;

    public AuthController(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
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

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (usuario == null)
            return Unauthorized("Email ou senha inválidos.");

        bool senhaCorreta =
            BCrypt.Net.BCrypt.Verify(
                dto.Senha,
                usuario.SenhaHash);

        if (!senhaCorreta)
            return Unauthorized("Email ou senha inválidos.");

        var token = _tokenService.GerarToken(usuario);

        return Ok(new
        {
            token
        });
    }
}
using ApiAuth.DTOs;
using ApiAuth.Interfaces;
using ApiAuth.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ApiAuth.Controllers;

/// <summary>
/// Endpoints de autenticação: register, login, refresh e logout.
/// - `register` e `login` são públicos.
/// - `refresh`/`logout` operam com refresh tokens passados no corpo.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        bool sucesso = await _authService.RegistrarAsync(dto);

        if (!sucesso)
        {
            return BadRequest(new
            {
                mensagem = "E-mail já cadastrado."
            });
        }

        return Created("",
            new ApiResponse<object>
            {
                Success = true,
                Message = "Usuário cadastrado."
            });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var token = await _authService.LoginAsync(dto);

        if (token == null)
        {
            return Unauthorized(new
            {
                mensagem = "E-mail ou senha inválidos."
            });
        }

        return Ok(
            new ApiResponse<object>
            {
                Success = true,
                Message = "Login realizado.",
                Data = new
                {
                    token
                }
            });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var resultado = await _authService.RefreshTokenAsync(dto);

        if (resultado == null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "Token inválido, expirado ou revogado."
            });
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Token atualizado com sucesso.",
            Data = resultado
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // O logout tenta revogar o token. Mesmo se retornar falso (token não achado),
        // retornamos sucesso para não dar pistas de segurança sobre a existência do token.
        await _authService.LogoutAsync(dto);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Logout realizado com sucesso."
        });
    }
}
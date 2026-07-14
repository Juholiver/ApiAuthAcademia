using ApiAuth.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiAuth.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GerarToken(Usuario usuario)
    {
        // 1. Garante que as variáveis de ambiente recarregadas sejam acessíveis
        var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key") 
                     ?? _configuration["Jwt:Key"];
                     
        var jwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer") 
                        ?? _configuration["Jwt:Issuer"];
                        
        var jwtAudience = Environment.GetEnvironmentVariable("Jwt__Audience") 
                          ?? _configuration["Jwt:Audience"];
                          
        var jwtExpires = Environment.GetEnvironmentVariable("Jwt__ExpiresInMinutes") 
                         ?? _configuration["Jwt:ExpiresInMinutes"] 
                         ?? "60";

        if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
        {
            throw new InvalidOperationException("A chave JWT (Jwt__Key) precisa ter no mínimo 32 caracteres e estar configurada corretamente.");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, usuario.Nome),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Adiciona identificador único ao Token
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Ajustamos para DateTime.UtcNow para evitar problemas de fuso horário na validação
        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtExpires)),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
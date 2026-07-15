using ApiAuth.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
        var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key")
                     ?? _configuration["Jwt:Key"];

        var jwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer")
                        ?? _configuration["Jwt:Issuer"];

        var jwtAudience = Environment.GetEnvironmentVariable("Jwt__Audience")
                          ?? _configuration["Jwt:Audience"];

        var jwtExpires = Environment.GetEnvironmentVariable("Jwt__ExpiresInMinutes")
                         ?? _configuration["Jwt:ExpiresInMinutes"]
                         ?? "60";

        if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
        {
            throw new InvalidOperationException(
                "A chave JWT deve possuir no mínimo 32 caracteres.");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtExpires)),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GerarRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }
}
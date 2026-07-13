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
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, usuario.Nome),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(_configuration["Jwt:ExpiresInMinutes"])),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
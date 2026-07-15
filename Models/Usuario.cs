namespace ApiAuth.Models;

public class Usuario
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string SenhaHash { get; set; } = string.Empty;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    // Relação 1:N com RefreshToken
    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();
}
namespace ApiAuth.Models;

public class RefreshToken
{
    public int Id { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiraEm { get; set; }

    public bool Revogado { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
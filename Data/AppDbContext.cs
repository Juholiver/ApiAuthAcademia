using ApiAuth.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiAuth.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<Treino> Treinos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Garante que se o usuário for deletado, os refresh tokens dele também sejam limpos (Cascade Delete)
        modelBuilder.Entity<RefreshToken>()
            .HasOne<Usuario>() 
            .WithMany() // Se sua classe Usuario não tiver uma List<RefreshToken>, deixe vazio
            .HasForeignKey(rt => rt.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Adiciona um índice no Token para buscas rápidas no banco de dados durante o Refresh/Logout
        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique();
    }
}
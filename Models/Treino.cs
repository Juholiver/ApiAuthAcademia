using System.ComponentModel.DataAnnotations;

namespace ApiAuth.Models;

public class Treino
{
    public int Id { get; set; }
    
    [Required]
    public int UsuarioId { get; set; }

    [Required]
    public int ExercicioId { get; set; } // <-- Adicionado: ID do exercício da API do Render
    
    [Required]
    [MaxLength(1)]
    public string Divisao { get; set; } = ""; 
    
    [Required]
    [MaxLength(100)]
    public string NomeExercicio { get; set; } = "";
    
    public int Series { get; set; }
    
    [MaxLength(50)]
    public string Repeticoes { get; set; } = "";
    
    [MaxLength(50)]
    public string Carga { get; set; } = "";
    
    [MaxLength(20)]
    public string Descanso { get; set; } = "";
}
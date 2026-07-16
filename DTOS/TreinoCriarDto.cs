using System.ComponentModel.DataAnnotations;

namespace ApiAuth.DTOs;

public class TreinoCriarDto
{
    [Required]
    public int ExercicioId { get; set; } // <-- Adicionado

    [Required]
    [RegularExpression("^[A-Za-z]$", ErrorMessage = "A divisão deve ser apenas uma letra (A, B, C, etc.).")]
    public string Divisao { get; set; } = "";

    [Required]
    public string NomeExercicio { get; set; } = "";

    public int Series { get; set; }
    public string Repeticoes { get; set; } = "";
    public string Carga { get; set; } = "";
    public string Descanso { get; set; } = "";
}
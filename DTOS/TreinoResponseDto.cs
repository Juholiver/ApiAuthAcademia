namespace ApiAuth.DTOs;

public class TreinoResponseDto
{
    public int Id { get; set; }
    public int ExercicioId { get; set; } // <-- Adicionado
    public string Divisao { get; set; } = "";
    public string NomeExercicio { get; set; } = "";
    public int Series { get; set; }
    public string Repeticoes { get; set; } = "";
    public string Carga { get; set; } = "";
    public string Descanso { get; set; } = "";
}
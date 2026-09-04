namespace PortalCopa2026.Models.Simulacao;

public class Simulacao
{
    public int Id { get; set; }
    public DateTime DataCriacao { get; set; }

    public ICollection<SimulacaoJogo> Jogos { get; set; } = new List<SimulacaoJogo>();
}

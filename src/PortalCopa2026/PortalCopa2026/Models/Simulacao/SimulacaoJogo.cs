using PortalCopa2026.Models.Copa;

namespace PortalCopa2026.Models.Simulacao;

public class SimulacaoJogo
{
    public int Id { get; set; }

    public int SimulacaoId { get; set; }
    public Simulacao? Simulacao { get; set; }

    public int JogoId { get; set; }
    public Jogo? Jogo { get; set; }

    public int PlacarMandante { get; set; }
    public int PlacarVisitante { get; set; }
}

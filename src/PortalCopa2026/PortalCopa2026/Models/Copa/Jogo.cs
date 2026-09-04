namespace PortalCopa2026.Models.Copa;

public class Jogo
{
    public int Id { get; set; }
    public DateOnly Data { get; set; }
    public TimeOnly Horario { get; set; }
    public string Fase { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estadio { get; set; } = string.Empty;

    public int? GrupoId { get; set; }
    public Grupo? Grupo { get; set; }

    // Nulo quando o confronto da fase eliminatória ainda depende do resultado de outro jogo.
    public int? SelecaoMandanteId { get; set; }
    public Selecao? SelecaoMandante { get; set; }

    public int? SelecaoVisitanteId { get; set; }
    public Selecao? SelecaoVisitante { get; set; }

    // Resultado oficial da fase de grupos, distinto do placar simulado (SimulacaoJogo). Nulo até ser registrado.
    public int? PlacarMandante { get; set; }
    public int? PlacarVisitante { get; set; }
}

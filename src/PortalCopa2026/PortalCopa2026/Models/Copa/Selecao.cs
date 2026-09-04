namespace PortalCopa2026.Models.Copa;

public class Selecao
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Tecnico { get; set; } = string.Empty;

    public int GrupoId { get; set; }
    public Grupo? Grupo { get; set; }

    public ICollection<Jogador> Jogadores { get; set; } = new List<Jogador>();

    public RankingFifa? RankingFifa { get; set; }
}

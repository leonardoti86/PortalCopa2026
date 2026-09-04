namespace PortalCopa2026.Models.Copa;

public class RankingFifa
{
    public int Id { get; set; }
    public int Posicao { get; set; }
    public double Pontos { get; set; }

    public int SelecaoId { get; set; }
    public Selecao? Selecao { get; set; }
}

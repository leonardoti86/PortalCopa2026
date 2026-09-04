namespace PortalCopa2026.Models.Copa;

public class Jogador
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Posicao { get; set; } = string.Empty;
    public int Idade { get; set; }
    public int Gols { get; set; }
    public int ParticipacoesCopa { get; set; }

    public int SelecaoId { get; set; }
    public Selecao? Selecao { get; set; }
}

namespace PortalCopa2026.Models.Copa;

public class Grupo
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;

    public ICollection<Selecao> Selecoes { get; set; } = new List<Selecao>();
}

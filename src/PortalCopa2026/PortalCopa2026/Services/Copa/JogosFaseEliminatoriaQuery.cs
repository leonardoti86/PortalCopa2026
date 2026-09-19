namespace PortalCopa2026.Services.Copa;

public static class JogosFaseEliminatoriaQuery
{
    public const string FaseSegundaFase = "Segunda Fase";
    public const string FaseOitavas = "Oitavas de Final";
    public const string FaseQuartas = "Quartas de Final";
    public const string FaseSemifinais = "Semifinais";
    public const string FaseTerceiroLugar = "Terceiro Lugar";
    public const string FaseFinal = "Final";

    // Rótulo curto usado para montar o texto do placeholder de um confronto ainda pendente
    // (ex.: "Vencedor Oitavas 2") - design.md (criar-jogos-quartas-semifinais) - Decisão 4.
    public static readonly IReadOnlyDictionary<string, string> RotuloCurtoPorFase = new Dictionary<string, string>
    {
        [FaseSegundaFase] = "Segunda Fase",
        [FaseOitavas] = "Oitavas",
        [FaseQuartas] = "Quartas",
        [FaseSemifinais] = "Semifinal"
    };
}

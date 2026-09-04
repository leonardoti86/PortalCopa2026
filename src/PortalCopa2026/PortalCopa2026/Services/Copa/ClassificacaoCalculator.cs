using PortalCopa2026.Services.Copa.Dtos;

namespace PortalCopa2026.Services.Copa;

// Cascata de desempate (design.md - Decisão 4 do change criar-simulador-jogos, mantida no change
// criar-grupos-classificacao/design.md - Decisão 2): Pts -> SG -> GP -> [confronto direto ->] Ranking FIFA (com
// fallback para seleção sem RankingFifa cadastrado) -> Codigo alfabético como desempate absoluto final.
public static class ClassificacaoCalculator
{
    public static IReadOnlyList<EstatisticasTime> ConstruirEstatisticasBase(IReadOnlyList<JogoClassificacaoDto> jogosGrupo)
    {
        var times = new Dictionary<string, EstatisticasTime>();

        foreach (var jogo in jogosGrupo)
        {
            if (!times.ContainsKey(jogo.CodigoMandante))
            {
                times[jogo.CodigoMandante] = new EstatisticasTime(jogo.CodigoMandante, jogo.SelecaoMandante);
            }

            if (!times.ContainsKey(jogo.CodigoVisitante))
            {
                times[jogo.CodigoVisitante] = new EstatisticasTime(jogo.CodigoVisitante, jogo.SelecaoVisitante);
            }

            if (jogo.PlacarMandante is null || jogo.PlacarVisitante is null)
            {
                continue;
            }

            times[jogo.CodigoMandante].RegistrarJogo(jogo.PlacarMandante.Value, jogo.PlacarVisitante.Value);
            times[jogo.CodigoVisitante].RegistrarJogo(jogo.PlacarVisitante.Value, jogo.PlacarMandante.Value);
        }

        return times.Values.ToList();
    }

    public static IReadOnlyList<EstatisticasTime> OrdenarComDesempate(
        IReadOnlyList<EstatisticasTime> times,
        IReadOnlyList<JogoClassificacaoDto> jogosGrupo,
        IReadOnlyDictionary<string, int?> rankingPorCodigo,
        bool aplicarConfrontoDireto)
    {
        var porBase = times
            .OrderByDescending(t => t.Pontos)
            .ThenByDescending(t => t.SaldoGols)
            .ThenByDescending(t => t.GolsPro)
            .ToList();

        var resultado = new List<EstatisticasTime>();
        var indice = 0;
        while (indice < porBase.Count)
        {
            var atual = porBase[indice];
            var bloco = porBase
                .Skip(indice)
                .TakeWhile(t => t.Pontos == atual.Pontos && t.SaldoGols == atual.SaldoGols && t.GolsPro == atual.GolsPro)
                .ToList();

            resultado.AddRange(bloco.Count == 1
                ? bloco
                : DesempatarBloco(bloco, jogosGrupo, rankingPorCodigo, aplicarConfrontoDireto));

            indice += bloco.Count;
        }

        return resultado;
    }

    private static List<EstatisticasTime> DesempatarBloco(
        List<EstatisticasTime> bloco,
        IReadOnlyList<JogoClassificacaoDto> jogosGrupo,
        IReadOnlyDictionary<string, int?> rankingPorCodigo,
        bool aplicarConfrontoDireto)
    {
        if (!aplicarConfrontoDireto)
        {
            return DesempatarPorRankingFifa(bloco, rankingPorCodigo);
        }

        var codigosBloco = bloco.Select(t => t.Codigo).ToHashSet();
        var miniEstatisticas = bloco.ToDictionary(t => t.Codigo, t => new EstatisticasTime(t.Codigo, t.Nome));

        foreach (var jogo in jogosGrupo)
        {
            if (jogo.PlacarMandante is null || jogo.PlacarVisitante is null)
            {
                continue;
            }

            if (!codigosBloco.Contains(jogo.CodigoMandante) || !codigosBloco.Contains(jogo.CodigoVisitante))
            {
                continue;
            }

            miniEstatisticas[jogo.CodigoMandante].RegistrarJogo(jogo.PlacarMandante.Value, jogo.PlacarVisitante.Value);
            miniEstatisticas[jogo.CodigoVisitante].RegistrarJogo(jogo.PlacarVisitante.Value, jogo.PlacarMandante.Value);
        }

        var porConfrontoDireto = bloco
            .OrderByDescending(t => miniEstatisticas[t.Codigo].Pontos)
            .ThenByDescending(t => miniEstatisticas[t.Codigo].SaldoGols)
            .ToList();

        var resultado = new List<EstatisticasTime>();
        var indice = 0;
        while (indice < porConfrontoDireto.Count)
        {
            var atual = porConfrontoDireto[indice];
            var subBloco = porConfrontoDireto
                .Skip(indice)
                .TakeWhile(t => miniEstatisticas[t.Codigo].Pontos == miniEstatisticas[atual.Codigo].Pontos
                    && miniEstatisticas[t.Codigo].SaldoGols == miniEstatisticas[atual.Codigo].SaldoGols)
                .ToList();

            resultado.AddRange(subBloco.Count == 1
                ? subBloco
                : DesempatarPorRankingFifa(subBloco, rankingPorCodigo));

            indice += subBloco.Count;
        }

        return resultado;
    }

    private static List<EstatisticasTime> DesempatarPorRankingFifa(
        List<EstatisticasTime> bloco,
        IReadOnlyDictionary<string, int?> rankingPorCodigo)
    {
        return bloco
            .OrderBy(t => PosicaoRankingComFallback(t.Codigo, rankingPorCodigo))
            .ThenBy(t => t.Codigo, StringComparer.Ordinal)
            .ToList();
    }

    // Fallback (design.md - Decisão 4 do change criar-simulador-jogos): seleção sem RankingFifa cadastrado é
    // tratada como a pior posição possível, garantindo que o desempate nunca fique indefinido nem dependa de
    // um valor nulo.
    private static int PosicaoRankingComFallback(string codigo, IReadOnlyDictionary<string, int?> rankingPorCodigo) =>
        rankingPorCodigo.TryGetValue(codigo, out var posicao) && posicao.HasValue ? posicao.Value : int.MaxValue;
}

public sealed class EstatisticasTime(string codigo, string nome)
{
    public string Codigo { get; } = codigo;
    public string Nome { get; } = nome;
    public int Jogos { get; private set; }
    public int Vitorias { get; private set; }
    public int Empates { get; private set; }
    public int Derrotas { get; private set; }
    public int GolsPro { get; private set; }
    public int GolsContra { get; private set; }
    public int SaldoGols => GolsPro - GolsContra;
    public int Pontos => Vitorias * 3 + Empates;

    public void RegistrarJogo(int golsPro, int golsContra)
    {
        Jogos++;
        GolsPro += golsPro;
        GolsContra += golsContra;

        if (golsPro > golsContra)
        {
            Vitorias++;
        }
        else if (golsPro < golsContra)
        {
            Derrotas++;
        }
        else
        {
            Empates++;
        }
    }

    public ClassificacaoTimeDto ParaDto(int posicao, bool classificado, bool wildcard) =>
        new(Codigo, Nome, Jogos, Vitorias, Empates, Derrotas, GolsPro, GolsContra, SaldoGols, Pontos, posicao, classificado, wildcard);
}

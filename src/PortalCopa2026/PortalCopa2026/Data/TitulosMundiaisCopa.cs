namespace PortalCopa2026.Data;

// Quantidade de títulos da Copa do Mundo FIFA conquistados por cada seleção até a edição de 2022
// (a última efetivamente disputada) - dado histórico público e verificável, sem coluna nova em
// Selecao e fora do escopo de "./fontes" (que trata apenas dos dados oficiais do torneio 2026).
// Nunca é incrementado pelo resultado simulado da Final de 2026 dentro do app - design.md
// (criar-jogo-teceiro-lugar-final) - Decisão 7. Cobre as 48 seleções de teams.json, já que o
// simulador não valida força das seleções em nenhuma fase anterior.
public static class TitulosMundiaisCopa
{
    public static readonly IReadOnlyDictionary<string, int> PorCodigoSelecao = new Dictionary<string, int>
    {
        ["ALG"] = 0,
        ["ARG"] = 3,
        ["AUS"] = 0,
        ["AUT"] = 0,
        ["BEL"] = 0,
        ["BIH"] = 0,
        ["BRA"] = 5,
        ["CAN"] = 0,
        ["CIV"] = 0,
        ["COD"] = 0,
        ["COL"] = 0,
        ["CPV"] = 0,
        ["CRO"] = 0,
        ["CUW"] = 0,
        ["CZE"] = 0,
        ["ECU"] = 0,
        ["EGY"] = 0,
        ["ENG"] = 1,
        ["ESP"] = 1,
        ["FRA"] = 2,
        ["GER"] = 4,
        ["GHA"] = 0,
        ["HAI"] = 0,
        ["IRN"] = 0,
        ["IRQ"] = 0,
        ["JOR"] = 0,
        ["JPN"] = 0,
        ["KOR"] = 0,
        ["KSA"] = 0,
        ["MAR"] = 0,
        ["MEX"] = 0,
        ["NED"] = 0,
        ["NOR"] = 0,
        ["NZL"] = 0,
        ["PAN"] = 0,
        ["PAR"] = 0,
        ["POR"] = 0,
        ["QAT"] = 0,
        ["RSA"] = 0,
        ["SCO"] = 0,
        ["SEN"] = 0,
        ["SUI"] = 0,
        ["SWE"] = 0,
        ["TUN"] = 0,
        ["TUR"] = 0,
        ["URU"] = 2,
        ["USA"] = 0,
        ["UZB"] = 0
    };

    public static int ObterQuantidade(string codigoSelecao) =>
        PorCodigoSelecao.GetValueOrDefault(codigoSelecao, 0);
}

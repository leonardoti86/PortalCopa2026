using PortalCopa2026.Models.Copa;

namespace PortalCopa2026.Services.Copa;

// Opera sobre o grafo de Jogo já carregado (JogoOrigemMandante/JogoOrigemVisitante precisam estar
// incluídos pelo chamador quando aplicável) - design.md (criar-jogos-fase2-oitavas) - Decisão 2.
public static class JogoEliminatorioResolver
{
    public static int? ResolverSelecaoId(Jogo jogo, bool mandante)
    {
        var selecaoId = mandante ? jogo.SelecaoMandanteId : jogo.SelecaoVisitanteId;
        if (selecaoId is not null)
        {
            return selecaoId;
        }

        var jogoOrigem = mandante ? jogo.JogoOrigemMandante : jogo.JogoOrigemVisitante;
        return jogoOrigem is not null ? ObterVencedorId(jogoOrigem) : null;
    }

    public static int? ObterVencedorId(Jogo jogo)
    {
        var mandanteId = ResolverSelecaoId(jogo, mandante: true);
        var visitanteId = ResolverSelecaoId(jogo, mandante: false);

        if (mandanteId is null || visitanteId is null || jogo.PlacarMandante is null || jogo.PlacarVisitante is null)
        {
            return null;
        }

        if (jogo.PlacarMandante > jogo.PlacarVisitante)
        {
            return mandanteId;
        }

        if (jogo.PlacarVisitante > jogo.PlacarMandante)
        {
            return visitanteId;
        }

        if (jogo.PlacarPenaltisMandante is null || jogo.PlacarPenaltisVisitante is null
            || jogo.PlacarPenaltisMandante == jogo.PlacarPenaltisVisitante)
        {
            return null;
        }

        return jogo.PlacarPenaltisMandante > jogo.PlacarPenaltisVisitante ? mandanteId : visitanteId;
    }

    // Espelha ObterVencedorId, retornando o lado que não venceu, usada apenas para alimentar o
    // jogo de Terceiro Lugar - design.md (criar-jogo-teceiro-lugar-final) - Decisão 2.
    public static int? ObterPerdedorId(Jogo jogo)
    {
        var vencedorId = ObterVencedorId(jogo);
        if (vencedorId is null)
        {
            return null;
        }

        var mandanteId = ResolverSelecaoId(jogo, mandante: true);
        var visitanteId = ResolverSelecaoId(jogo, mandante: false);

        return mandanteId == vencedorId ? visitanteId : mandanteId;
    }
}

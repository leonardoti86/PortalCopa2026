using Microsoft.EntityFrameworkCore;
using PortalCopa2026.Data;
using PortalCopa2026.Models.Copa;

namespace PortalCopa2026.Services.Copa;

// Extraído de FaseEliminatoriaService (sem alterar assinatura nem comportamento) para ser
// reaproveitado também por FinalCopaService - design.md (criar-jogo-teceiro-lugar-final) - Decisão 3.
internal static class GrafoEliminatorioReader
{
    // Carrega, de uma só vez, todos os jogos de fase eliminatória (Ordem != null cobre Segunda
    // Fase, Oitavas, Quartas, Semifinais, Terceiro Lugar e Final). Como todos ficam rastreados no
    // mesmo contexto, o relationship fixup do EF Core preenche sozinho
    // JogoOrigemMandante/JogoOrigemVisitante em qualquer profundidade, sem precisar de
    // Include/ThenInclude por nível - design.md (criar-jogos-quartas-semifinais) - Decisão 3.
    public static Task<List<Jogo>> CarregarGrafoEliminatorioAsync(AppDbContext context, CancellationToken cancellationToken) =>
        context.Jogos
            .Where(j => j.Ordem != null)
            .Include(j => j.SelecaoMandante)
            .Include(j => j.SelecaoVisitante)
            .ToListAsync(cancellationToken);

    // Monta o texto do placeholder (ex.: "Vencedor Oitavas 2") a partir da fase e da Ordem do
    // jogo de origem - design.md (criar-jogos-quartas-semifinais) - Decisão 4.
    public static string? RotularOrigem(Jogo? jogoOrigem)
    {
        if (jogoOrigem is null)
        {
            return null;
        }

        var rotuloFase = JogosFaseEliminatoriaQuery.RotuloCurtoPorFase.GetValueOrDefault(jogoOrigem.Fase, jogoOrigem.Fase);
        return $"Vencedor {rotuloFase} {jogoOrigem.Ordem}";
    }

    // Resolve a seleção de um lado percorrendo o encadeamento de jogos de origem em qualquer
    // profundidade (Segunda Fase -> Oitavas -> Quartas -> Semifinais), sem alterar a regra de
    // decisão do vencedor (JogoEliminatorioResolver, inalterado) - design.md
    // (criar-jogos-quartas-semifinais) - Decisão 2.
    public static Selecao? ResolverSelecao(Jogo jogo, bool mandante)
    {
        var selecao = mandante ? jogo.SelecaoMandante : jogo.SelecaoVisitante;
        if (selecao is not null)
        {
            return selecao;
        }

        var jogoOrigem = mandante ? jogo.JogoOrigemMandante : jogo.JogoOrigemVisitante;
        if (jogoOrigem is null)
        {
            return null;
        }

        var vencedorId = JogoEliminatorioResolver.ObterVencedorId(jogoOrigem);
        if (vencedorId is null)
        {
            return null;
        }

        var mandanteOrigem = ResolverSelecao(jogoOrigem, mandante: true);
        if (mandanteOrigem?.Id == vencedorId)
        {
            return mandanteOrigem;
        }

        var visitanteOrigem = ResolverSelecao(jogoOrigem, mandante: false);
        return visitanteOrigem?.Id == vencedorId ? visitanteOrigem : null;
    }
}

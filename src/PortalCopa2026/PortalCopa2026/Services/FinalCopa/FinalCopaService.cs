using PortalCopa2026.Data;
using PortalCopa2026.Models.Copa;
using PortalCopa2026.Services.Copa;
using PortalCopa2026.Services.FaseEliminatoria.Dtos;
using PortalCopa2026.Services.FinalCopa.Dtos;

namespace PortalCopa2026.Services.FinalCopa;

// Terceiro Lugar e Final reaproveitam o grafo eliminatório (GrafoEliminatorioReader), mas com uma
// diferença de leitura: a Final resolve por vencedor (mesma regra das demais fases), enquanto o
// Terceiro Lugar resolve por perdedor das Semifinais de origem (ObterPerdedorId) - design.md
// (criar-jogo-teceiro-lugar-final) - Decisão 3.
public class FinalCopaService(AppDbContext context) : IFinalCopaService
{
    public async Task<FinalCopaDto> ObterAsync(CancellationToken cancellationToken = default)
    {
        var jogos = await GrafoEliminatorioReader.CarregarGrafoEliminatorioAsync(context, cancellationToken);
        return MontarDto(jogos);
    }

    public async Task<FinalCopaDto> AtualizarPlacarOficialAsync(
        int jogoId,
        int? placarMandante,
        int? placarVisitante,
        int? placarPenaltisMandante,
        int? placarPenaltisVisitante,
        CancellationToken cancellationToken = default)
    {
        var jogos = await GrafoEliminatorioReader.CarregarGrafoEliminatorioAsync(context, cancellationToken);
        var jogo = jogos.FirstOrDefault(j => j.Id == jogoId)
            ?? throw new InvalidOperationException($"Jogo {jogoId} não encontrado.");

        if (jogo.Fase != JogosFaseEliminatoriaQuery.FaseTerceiroLugar && jogo.Fase != JogosFaseEliminatoriaQuery.FaseFinal)
        {
            throw new InvalidOperationException($"Jogo {jogoId} não pertence ao Terceiro Lugar nem à Final.");
        }

        if (placarPenaltisMandante is not null && placarPenaltisVisitante is not null
            && placarPenaltisMandante == placarPenaltisVisitante)
        {
            throw new InvalidOperationException("O placar de pênaltis não pode terminar empatado em uma fase eliminatória.");
        }

        var (mandante, visitante) = ResolverParticipantes(jogo);
        if (mandante is null || visitante is null)
        {
            throw new InvalidOperationException("Não é possível registrar o placar enquanto as duas seleções do confronto não forem conhecidas.");
        }

        jogo.PlacarMandante = placarMandante;
        jogo.PlacarVisitante = placarVisitante;
        jogo.PlacarPenaltisMandante = placarPenaltisMandante;
        jogo.PlacarPenaltisVisitante = placarPenaltisVisitante;

        await context.SaveChangesAsync(cancellationToken);

        return MontarDto(jogos);
    }

    private static FinalCopaDto MontarDto(IReadOnlyList<Jogo> jogos)
    {
        var jogoTerceiroLugar = jogos.First(j => j.Fase == JogosFaseEliminatoriaQuery.FaseTerceiroLugar);
        var jogoFinal = jogos.First(j => j.Fase == JogosFaseEliminatoriaQuery.FaseFinal);

        return new FinalCopaDto(
            ParaTerceiroLugarDto(jogoTerceiroLugar),
            ParaFinalDto(jogoFinal),
            ResolverCampeao(jogoFinal));
    }

    private static (Selecao? Mandante, Selecao? Visitante) ResolverParticipantes(Jogo jogo) =>
        jogo.Fase == JogosFaseEliminatoriaQuery.FaseTerceiroLugar
            ? (ResolverPerdedorSemifinal(jogo.JogoOrigemMandante), ResolverPerdedorSemifinal(jogo.JogoOrigemVisitante))
            : (GrafoEliminatorioReader.ResolverSelecao(jogo, mandante: true), GrafoEliminatorioReader.ResolverSelecao(jogo, mandante: false));

    // A origem do Terceiro Lugar é sempre a Semifinal diretamente (sem recursão adicional - a
    // Semifinal já resolve seu próprio mandante/visitante via ResolverSelecao) - design.md
    // (criar-jogo-teceiro-lugar-final) - Decisão 3.
    private static Selecao? ResolverPerdedorSemifinal(Jogo? jogoOrigemSemifinal)
    {
        if (jogoOrigemSemifinal is null)
        {
            return null;
        }

        var perdedorId = JogoEliminatorioResolver.ObterPerdedorId(jogoOrigemSemifinal);
        if (perdedorId is null)
        {
            return null;
        }

        var mandante = GrafoEliminatorioReader.ResolverSelecao(jogoOrigemSemifinal, mandante: true);
        if (mandante?.Id == perdedorId)
        {
            return mandante;
        }

        var visitante = GrafoEliminatorioReader.ResolverSelecao(jogoOrigemSemifinal, mandante: false);
        return visitante?.Id == perdedorId ? visitante : null;
    }

    private static FaseEliminatoriaJogoDto ParaTerceiroLugarDto(Jogo jogo)
    {
        var (mandante, visitante) = ResolverParticipantes(jogo);

        return new FaseEliminatoriaJogoDto(
            jogo.Id,
            jogo.Ordem ?? 0,
            jogo.Data,
            jogo.Horario,
            jogo.Estadio,
            jogo.Cidade,
            mandante?.Nome,
            mandante?.Codigo,
            jogo.SelecaoMandanteId is null ? RotularPerdedorOrigem(jogo.JogoOrigemMandante) : null,
            visitante?.Nome,
            visitante?.Codigo,
            jogo.SelecaoVisitanteId is null ? RotularPerdedorOrigem(jogo.JogoOrigemVisitante) : null,
            jogo.PlacarMandante,
            jogo.PlacarVisitante,
            jogo.PlacarPenaltisMandante,
            jogo.PlacarPenaltisVisitante);
    }

    private static FaseEliminatoriaJogoDto ParaFinalDto(Jogo jogo)
    {
        var mandante = GrafoEliminatorioReader.ResolverSelecao(jogo, mandante: true);
        var visitante = GrafoEliminatorioReader.ResolverSelecao(jogo, mandante: false);

        return new FaseEliminatoriaJogoDto(
            jogo.Id,
            jogo.Ordem ?? 0,
            jogo.Data,
            jogo.Horario,
            jogo.Estadio,
            jogo.Cidade,
            mandante?.Nome,
            mandante?.Codigo,
            jogo.SelecaoMandanteId is null ? GrafoEliminatorioReader.RotularOrigem(jogo.JogoOrigemMandante) : null,
            visitante?.Nome,
            visitante?.Codigo,
            jogo.SelecaoVisitanteId is null ? GrafoEliminatorioReader.RotularOrigem(jogo.JogoOrigemVisitante) : null,
            jogo.PlacarMandante,
            jogo.PlacarVisitante,
            jogo.PlacarPenaltisMandante,
            jogo.PlacarPenaltisVisitante);
    }

    private static string? RotularPerdedorOrigem(Jogo? jogoOrigemSemifinal) =>
        jogoOrigemSemifinal is null ? null : $"Perdedor Semifinal {jogoOrigemSemifinal.Ordem}";

    private static CampeaoDto? ResolverCampeao(Jogo jogoFinal)
    {
        var vencedorId = JogoEliminatorioResolver.ObterVencedorId(jogoFinal);
        if (vencedorId is null)
        {
            return null;
        }

        var mandante = GrafoEliminatorioReader.ResolverSelecao(jogoFinal, mandante: true);
        var campeao = mandante?.Id == vencedorId
            ? mandante
            : GrafoEliminatorioReader.ResolverSelecao(jogoFinal, mandante: false);

        return campeao is null
            ? null
            : new CampeaoDto(campeao.Nome, campeao.Codigo, TitulosMundiaisCopa.ObterQuantidade(campeao.Codigo));
    }
}

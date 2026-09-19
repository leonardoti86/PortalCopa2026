using Microsoft.EntityFrameworkCore;
using PortalCopa2026.Data;
using PortalCopa2026.Models.Copa;
using PortalCopa2026.Services.Copa;
using PortalCopa2026.Services.FaseEliminatoria.Dtos;

namespace PortalCopa2026.Services.FaseEliminatoria;

public class FaseEliminatoriaService(AppDbContext context) : IFaseEliminatoriaService
{
    public async Task<IReadOnlyList<FaseEliminatoriaJogoDto>> ObterSegundaFaseAsync(CancellationToken cancellationToken = default)
    {
        var jogos = await CarregarGrafoEliminatorioAsync(cancellationToken);

        return jogos
            .Where(j => j.Fase == JogosFaseEliminatoriaQuery.FaseSegundaFase)
            .OrderBy(j => j.Ordem)
            .Select(ParaDto)
            .ToList();
    }

    public async Task<IReadOnlyList<FaseEliminatoriaJogoDto>> ObterOitavasAsync(CancellationToken cancellationToken = default)
    {
        var jogos = await CarregarGrafoEliminatorioAsync(cancellationToken);

        return jogos
            .Where(j => j.Fase == JogosFaseEliminatoriaQuery.FaseOitavas)
            .OrderBy(j => j.Ordem)
            .Select(ParaDto)
            .ToList();
    }

    public async Task<IReadOnlyList<FaseEliminatoriaJogoDto>> ObterQuartasAsync(CancellationToken cancellationToken = default)
    {
        var jogos = await CarregarGrafoEliminatorioAsync(cancellationToken);

        return jogos
            .Where(j => j.Fase == JogosFaseEliminatoriaQuery.FaseQuartas)
            .OrderBy(j => j.Ordem)
            .Select(ParaDto)
            .ToList();
    }

    public async Task<IReadOnlyList<FaseEliminatoriaJogoDto>> ObterSemifinaisAsync(CancellationToken cancellationToken = default)
    {
        var jogos = await CarregarGrafoEliminatorioAsync(cancellationToken);

        return jogos
            .Where(j => j.Fase == JogosFaseEliminatoriaQuery.FaseSemifinais)
            .OrderBy(j => j.Ordem)
            .Select(ParaDto)
            .ToList();
    }

    public async Task<FaseEliminatoriaJogoDto> AtualizarPlacarOficialAsync(
        int jogoId,
        int? placarMandante,
        int? placarVisitante,
        int? placarPenaltisMandante,
        int? placarPenaltisVisitante,
        CancellationToken cancellationToken = default)
    {
        var jogos = await CarregarGrafoEliminatorioAsync(cancellationToken);
        var jogo = jogos.FirstOrDefault(j => j.Id == jogoId)
            ?? throw new InvalidOperationException($"Jogo {jogoId} não encontrado.");

        if (placarPenaltisMandante is not null && placarPenaltisVisitante is not null
            && placarPenaltisMandante == placarPenaltisVisitante)
        {
            throw new InvalidOperationException("O placar de pênaltis não pode terminar empatado em uma fase eliminatória.");
        }

        if (GrafoEliminatorioReader.ResolverSelecao(jogo, mandante: true) is null || GrafoEliminatorioReader.ResolverSelecao(jogo, mandante: false) is null)
        {
            throw new InvalidOperationException("Não é possível registrar o placar enquanto as duas seleções do confronto não forem conhecidas.");
        }

        jogo.PlacarMandante = placarMandante;
        jogo.PlacarVisitante = placarVisitante;
        jogo.PlacarPenaltisMandante = placarPenaltisMandante;
        jogo.PlacarPenaltisVisitante = placarPenaltisVisitante;

        await context.SaveChangesAsync(cancellationToken);

        return ParaDto(jogo);
    }

    // Delegado a GrafoEliminatorioReader (extraído para ser reaproveitado por FinalCopaService) -
    // design.md (criar-jogo-teceiro-lugar-final) - Decisão 3.
    private Task<List<Jogo>> CarregarGrafoEliminatorioAsync(CancellationToken cancellationToken) =>
        GrafoEliminatorioReader.CarregarGrafoEliminatorioAsync(context, cancellationToken);

    private static FaseEliminatoriaJogoDto ParaDto(Jogo jogo)
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
}

using Microsoft.EntityFrameworkCore;
using PortalCopa2026.Data;
using PortalCopa2026.Models.Simulacao;
using PortalCopa2026.Services.Copa;
using PortalCopa2026.Services.Copa.Dtos;
using PortalCopa2026.Services.Simulador.Dtos;

namespace PortalCopa2026.Services.Simulador;

public class SimuladorService(AppDbContext context) : ISimuladorService
{
    private const int TotalWildcards = 8;

    public async Task<SimuladorEstadoDto> ObterEstadoAtualAsync(CancellationToken cancellationToken = default)
    {
        var simulacaoId = await ObterOuCriarSimulacaoIdAsync(cancellationToken);
        return await MontarEstadoAsync(simulacaoId, cancellationToken);
    }

    public async Task<SimuladorEstadoDto> AtualizarPlacarAsync(int jogoId, int? placarMandante, int? placarVisitante, CancellationToken cancellationToken = default)
    {
        var simulacaoId = await ObterOuCriarSimulacaoIdAsync(cancellationToken);

        var simulacaoJogo = await context.SimulacaoJogos
            .FirstOrDefaultAsync(sj => sj.SimulacaoId == simulacaoId && sj.JogoId == jogoId, cancellationToken);

        if (placarMandante is null || placarVisitante is null)
        {
            if (simulacaoJogo is not null)
            {
                context.SimulacaoJogos.Remove(simulacaoJogo);
            }
        }
        else if (simulacaoJogo is null)
        {
            context.SimulacaoJogos.Add(new SimulacaoJogo
            {
                SimulacaoId = simulacaoId,
                JogoId = jogoId,
                PlacarMandante = placarMandante.Value,
                PlacarVisitante = placarVisitante.Value
            });
        }
        else
        {
            simulacaoJogo.PlacarMandante = placarMandante.Value;
            simulacaoJogo.PlacarVisitante = placarVisitante.Value;
        }

        await context.SaveChangesAsync(cancellationToken);

        return await MontarEstadoAsync(simulacaoId, cancellationToken);
    }

    private async Task<int> ObterOuCriarSimulacaoIdAsync(CancellationToken cancellationToken)
    {
        var simulacaoId = await context.Simulacoes
            .OrderBy(s => s.DataCriacao)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (simulacaoId.HasValue)
        {
            return simulacaoId.Value;
        }

        var simulacao = new Simulacao { DataCriacao = DateTime.Now };
        context.Simulacoes.Add(simulacao);
        await context.SaveChangesAsync(cancellationToken);
        return simulacao.Id;
    }

    private async Task<SimuladorEstadoDto> MontarEstadoAsync(int simulacaoId, CancellationToken cancellationToken)
    {
        var jogosDto = await CarregarJogosAsync(simulacaoId, cancellationToken);

        var rankingPorCodigo = await context.Selecoes
            .Select(s => new { s.Codigo, Posicao = s.RankingFifa != null ? (int?)s.RankingFifa!.Posicao : null })
            .ToDictionaryAsync(s => s.Codigo, s => s.Posicao, cancellationToken);

        var classificacoesPorGrupo = new List<(string Grupo, IReadOnlyList<EstatisticasTime> Ordenados)>();

        foreach (var grupoJogos in jogosDto.GroupBy(j => j.Grupo).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            var jogos = grupoJogos.Select(ParaClassificacao).ToList();
            var times = ClassificacaoCalculator.ConstruirEstatisticasBase(jogos);
            var ordenados = ClassificacaoCalculator.OrdenarComDesempate(times, jogos, rankingPorCodigo, aplicarConfrontoDireto: true);
            classificacoesPorGrupo.Add((grupoJogos.Key, ordenados));
        }

        var terceiros = classificacoesPorGrupo.Select(g => g.Ordenados[2]).ToList();
        var terceirosOrdenados = ClassificacaoCalculator.OrdenarComDesempate(terceiros, [], rankingPorCodigo, aplicarConfrontoDireto: false);
        var wildcardCodigos = terceirosOrdenados.Take(TotalWildcards).Select(t => t.Codigo).ToHashSet();

        var classificacoes = classificacoesPorGrupo
            .Select(g => new ClassificacaoGrupoDto(
                g.Grupo,
                g.Ordenados
                    .Select((t, index) => t.ParaDto(
                        posicao: index + 1,
                        classificado: index < 2,
                        wildcard: index == 2 && wildcardCodigos.Contains(t.Codigo)))
                    .ToList()))
            .ToList();

        var rankingTerceiros = terceirosOrdenados
            .Select((t, index) => t.ParaDto(posicao: index + 1, classificado: false, wildcard: index < TotalWildcards))
            .ToList();

        var totalJogos = jogosDto.Count;
        var jogosSimulados = jogosDto.Count(j => j.PlacarMandante.HasValue && j.PlacarVisitante.HasValue);

        return new SimuladorEstadoDto(jogosDto, classificacoes, rankingTerceiros, totalJogos, jogosSimulados);
    }

    private async Task<List<SimuladorJogoDto>> CarregarJogosAsync(int simulacaoId, CancellationToken cancellationToken)
    {
        var linhas = await (
            from j in JogosFaseGruposQuery.ObterJogosFaseGrupos(context)
            join sj in context.SimulacaoJogos.Where(x => x.SimulacaoId == simulacaoId)
                on j.Id equals sj.JogoId into placarGroup
            from placar in placarGroup.DefaultIfEmpty()
            orderby j.Data, j.Horario
            select new
            {
                j.Id,
                Grupo = j.Grupo!.Codigo,
                MandanteNome = j.SelecaoMandante!.Nome,
                MandanteCodigo = j.SelecaoMandante!.Codigo,
                VisitanteNome = j.SelecaoVisitante!.Nome,
                VisitanteCodigo = j.SelecaoVisitante!.Codigo,
                PlacarMandante = (int?)placar!.PlacarMandante,
                PlacarVisitante = (int?)placar!.PlacarVisitante
            }).ToListAsync(cancellationToken);

        return linhas
            .Select(l => new SimuladorJogoDto(l.Id, l.Grupo, l.MandanteNome, l.MandanteCodigo, l.VisitanteNome, l.VisitanteCodigo, l.PlacarMandante, l.PlacarVisitante))
            .ToList();
    }

    private static JogoClassificacaoDto ParaClassificacao(SimuladorJogoDto jogo) =>
        new(jogo.Grupo, jogo.CodigoMandante, jogo.SelecaoMandante, jogo.CodigoVisitante, jogo.SelecaoVisitante, jogo.PlacarMandante, jogo.PlacarVisitante);
}

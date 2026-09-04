using Microsoft.EntityFrameworkCore;
using PortalCopa2026.Data;
using PortalCopa2026.Services.Copa;
using PortalCopa2026.Services.Copa.Dtos;
using PortalCopa2026.Services.Grupos.Dtos;

namespace PortalCopa2026.Services.Grupos;

public class GruposService(AppDbContext context) : IGruposService
{
    private const int TotalWildcards = 8;

    public async Task<GruposEstadoDto> ObterEstadoAtualAsync(CancellationToken cancellationToken = default) =>
        await MontarEstadoAsync(cancellationToken);

    public async Task<GruposEstadoDto> AtualizarPlacarOficialAsync(int jogoId, int? placarMandante, int? placarVisitante, CancellationToken cancellationToken = default)
    {
        var jogo = await context.Jogos.FirstOrDefaultAsync(j => j.Id == jogoId, cancellationToken)
            ?? throw new InvalidOperationException($"Jogo {jogoId} não encontrado.");

        jogo.PlacarMandante = placarMandante;
        jogo.PlacarVisitante = placarVisitante;

        await context.SaveChangesAsync(cancellationToken);

        return await MontarEstadoAsync(cancellationToken);
    }

    private async Task<GruposEstadoDto> MontarEstadoAsync(CancellationToken cancellationToken)
    {
        var jogosDto = await CarregarJogosAsync(cancellationToken);

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

        return new GruposEstadoDto(jogosDto, classificacoes);
    }

    private async Task<List<GrupoJogoDto>> CarregarJogosAsync(CancellationToken cancellationToken)
    {
        var linhas = await JogosFaseGruposQuery.ObterJogosFaseGrupos(context)
            .OrderBy(j => j.Data)
            .ThenBy(j => j.Horario)
            .Select(j => new
            {
                j.Id,
                Grupo = j.Grupo!.Codigo,
                j.Data,
                j.Horario,
                MandanteNome = j.SelecaoMandante!.Nome,
                MandanteCodigo = j.SelecaoMandante!.Codigo,
                VisitanteNome = j.SelecaoVisitante!.Nome,
                VisitanteCodigo = j.SelecaoVisitante!.Codigo,
                j.PlacarMandante,
                j.PlacarVisitante
            }).ToListAsync(cancellationToken);

        return linhas
            .Select(l => new GrupoJogoDto(l.Id, l.Grupo, l.Data, l.Horario, l.MandanteNome, l.MandanteCodigo, l.VisitanteNome, l.VisitanteCodigo, l.PlacarMandante, l.PlacarVisitante))
            .ToList();
    }

    private static JogoClassificacaoDto ParaClassificacao(GrupoJogoDto jogo) =>
        new(jogo.Grupo, jogo.CodigoMandante, jogo.SelecaoMandante, jogo.CodigoVisitante, jogo.SelecaoVisitante, jogo.PlacarMandante, jogo.PlacarVisitante);
}

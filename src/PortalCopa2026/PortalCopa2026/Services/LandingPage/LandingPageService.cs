using Microsoft.EntityFrameworkCore;
using PortalCopa2026.Data;
using PortalCopa2026.Services.LandingPage.Dtos;

namespace PortalCopa2026.Services.LandingPage;

public class LandingPageService(AppDbContext context) : ILandingPageService
{
    private const string FaseGrupos = "Primeira Fase";

    public async Task<CopaEstatisticasDto> ObterEstatisticasAsync(CancellationToken cancellationToken = default)
    {
        var totalSelecoes = await context.Selecoes.CountAsync(cancellationToken);
        var totalJogos = await context.Jogos.CountAsync(cancellationToken);

        // Jogo.Estadio é usado em vez de Jogo.Cidade: o campo Cidade tem variações de
        // nomenclatura fora da "Primeira Fase" (ver design.md, Decision 3.1), enquanto
        // Estadio tem correspondência 1:1 e limpa com as 16 cidades-sede oficiais.
        var totalCidadesSede = await context.Jogos
            .Select(j => j.Estadio)
            .Distinct()
            .CountAsync(cancellationToken);

        return new CopaEstatisticasDto(totalSelecoes, totalCidadesSede, totalJogos);
    }

    public async Task<IReadOnlyList<ProximoJogoDto>> ObterProximosJogosAsync(int quantidade, CancellationToken cancellationToken = default)
    {
        var agora = DateTime.Now;

        var jogos = await context.Jogos
            .Where(j => j.Fase == FaseGrupos
                && j.SelecaoMandante != null
                && j.SelecaoVisitante != null
                && (j.Data > DateOnly.FromDateTime(agora)
                    || (j.Data == DateOnly.FromDateTime(agora) && j.Horario >= TimeOnly.FromDateTime(agora))))
            .OrderBy(j => j.Data)
            .ThenBy(j => j.Horario)
            .Take(quantidade)
            .Select(j => new ProximoJogoDto(
                j.Data,
                j.Horario,
                j.Grupo != null ? j.Grupo.Codigo : null,
                j.SelecaoMandante!.Nome,
                j.SelecaoMandante!.Codigo,
                j.SelecaoVisitante!.Nome,
                j.SelecaoVisitante!.Codigo,
                j.Estadio,
                j.Cidade))
            .ToListAsync(cancellationToken);

        return jogos;
    }

    public async Task<IReadOnlyList<RankingItemDto>> ObterRankingTopAsync(int quantidade, CancellationToken cancellationToken = default)
    {
        var ranking = await context.RankingsFifa
            .Include(r => r.Selecao)
            .Where(r => r.Selecao != null)
            .OrderBy(r => r.Posicao)
            .Take(quantidade)
            .Select(r => new RankingItemDto(r.Posicao, r.Selecao!.Nome, r.Selecao!.Codigo, r.Pontos))
            .ToListAsync(cancellationToken);

        return ranking;
    }
}

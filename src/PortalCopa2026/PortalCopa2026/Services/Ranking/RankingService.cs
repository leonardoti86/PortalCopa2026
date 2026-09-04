using Microsoft.EntityFrameworkCore;
using PortalCopa2026.Data;
using PortalCopa2026.Services.Ranking.Dtos;

namespace PortalCopa2026.Services.Ranking;

public class RankingService(AppDbContext context) : IRankingService
{
    public async Task<RankingEstadoDto> ObterRankingAsync(CancellationToken cancellationToken = default)
    {
        var posicoes = await context.RankingsFifa
            .Include(r => r.Selecao)
            .ThenInclude(s => s!.Grupo)
            .OrderBy(r => r.Posicao)
            .Select(r => new RankingPosicaoDto(r.Posicao, r.Selecao!.Nome, r.Selecao!.Codigo, r.Pontos, r.Selecao!.Grupo!.Codigo))
            .ToListAsync(cancellationToken);

        var semPosicao = await context.Selecoes
            .Where(s => s.RankingFifa == null)
            .Include(s => s.Grupo)
            .OrderBy(s => s.Nome)
            .Select(s => new SelecaoSemRankingDto(s.Nome, s.Codigo, s.Grupo!.Codigo))
            .ToListAsync(cancellationToken);

        return new RankingEstadoDto(posicoes, semPosicao);
    }
}

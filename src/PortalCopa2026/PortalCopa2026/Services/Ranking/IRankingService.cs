using PortalCopa2026.Services.Ranking.Dtos;

namespace PortalCopa2026.Services.Ranking;

public interface IRankingService
{
    Task<RankingEstadoDto> ObterRankingAsync(CancellationToken cancellationToken = default);
}

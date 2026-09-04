using PortalCopa2026.Services.LandingPage.Dtos;

namespace PortalCopa2026.Services.LandingPage;

public interface ILandingPageService
{
    Task<CopaEstatisticasDto> ObterEstatisticasAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProximoJogoDto>> ObterProximosJogosAsync(int quantidade, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RankingItemDto>> ObterRankingTopAsync(int quantidade, CancellationToken cancellationToken = default);
}

namespace PortalCopa2026.Services.Ranking.Dtos;

public record RankingEstadoDto(IReadOnlyList<RankingPosicaoDto> Posicoes, IReadOnlyList<SelecaoSemRankingDto> SemPosicao);

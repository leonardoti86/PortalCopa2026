using PortalCopa2026.Services.Copa.Dtos;

namespace PortalCopa2026.Services.Simulador.Dtos;

public record SimuladorEstadoDto(
    IReadOnlyList<SimuladorJogoDto> Jogos,
    IReadOnlyList<ClassificacaoGrupoDto> Classificacoes,
    IReadOnlyList<ClassificacaoTimeDto> RankingTerceiros,
    int TotalJogos,
    int JogosSimulados);

using PortalCopa2026.Services.Copa.Dtos;

namespace PortalCopa2026.Services.Grupos.Dtos;

public record GruposEstadoDto(
    IReadOnlyList<GrupoJogoDto> Jogos,
    IReadOnlyList<ClassificacaoGrupoDto> Classificacoes);

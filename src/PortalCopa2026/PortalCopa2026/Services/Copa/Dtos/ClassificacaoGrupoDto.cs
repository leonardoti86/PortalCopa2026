namespace PortalCopa2026.Services.Copa.Dtos;

public record ClassificacaoGrupoDto(
    string Grupo,
    IReadOnlyList<ClassificacaoTimeDto> Times);

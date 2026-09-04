namespace PortalCopa2026.Services.Grupos.Dtos;

public record GrupoJogoDto(
    int JogoId,
    string Grupo,
    DateOnly Data,
    TimeOnly Horario,
    string SelecaoMandante,
    string CodigoMandante,
    string SelecaoVisitante,
    string CodigoVisitante,
    int? PlacarMandante,
    int? PlacarVisitante);

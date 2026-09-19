namespace PortalCopa2026.Services.FaseEliminatoria.Dtos;

public record FaseEliminatoriaJogoDto(
    int JogoId,
    int Ordem,
    DateOnly Data,
    TimeOnly Horario,
    string Estadio,
    string Cidade,
    string? SelecaoMandante,
    string? CodigoMandante,
    string? OrigemMandanteRotulo,
    string? SelecaoVisitante,
    string? CodigoVisitante,
    string? OrigemVisitanteRotulo,
    int? PlacarMandante,
    int? PlacarVisitante,
    int? PlacarPenaltisMandante,
    int? PlacarPenaltisVisitante);

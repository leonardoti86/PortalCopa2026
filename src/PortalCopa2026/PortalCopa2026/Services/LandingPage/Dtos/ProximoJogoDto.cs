namespace PortalCopa2026.Services.LandingPage.Dtos;

public record ProximoJogoDto(
    DateOnly Data,
    TimeOnly Horario,
    string? Grupo,
    string SelecaoMandante,
    string CodigoMandante,
    string SelecaoVisitante,
    string CodigoVisitante,
    string Estadio,
    string Cidade);

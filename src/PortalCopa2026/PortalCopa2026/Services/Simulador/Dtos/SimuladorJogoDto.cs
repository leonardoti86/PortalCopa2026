namespace PortalCopa2026.Services.Simulador.Dtos;

public record SimuladorJogoDto(
    int JogoId,
    string Grupo,
    string SelecaoMandante,
    string CodigoMandante,
    string SelecaoVisitante,
    string CodigoVisitante,
    int? PlacarMandante,
    int? PlacarVisitante);

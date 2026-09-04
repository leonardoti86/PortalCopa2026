namespace PortalCopa2026.Services.Copa.Dtos;

public record JogoClassificacaoDto(
    string Grupo,
    string CodigoMandante,
    string SelecaoMandante,
    string CodigoVisitante,
    string SelecaoVisitante,
    int? PlacarMandante,
    int? PlacarVisitante);

namespace PortalCopa2026.Services.Jogos.Dtos;

public record JogoDto(
    int Id,
    DateOnly Data,
    TimeOnly Horario,
    string Grupo,
    string SelecaoMandante,
    string CodigoMandante,
    string SelecaoVisitante,
    string CodigoVisitante,
    string Estadio,
    string Cidade);

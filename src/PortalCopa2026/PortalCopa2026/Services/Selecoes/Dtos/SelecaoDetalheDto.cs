namespace PortalCopa2026.Services.Selecoes.Dtos;

public record SelecaoDetalheDto(
    string Nome,
    string Codigo,
    string GrupoCodigo,
    string FlagUrl,
    string Tecnico,
    int? RankingPosicao,
    double? RankingPontos,
    IReadOnlyList<JogadorDto> Jogadores);

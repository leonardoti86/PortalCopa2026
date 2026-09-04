namespace PortalCopa2026.Services.Copa.Dtos;

public record ClassificacaoTimeDto(
    string Codigo,
    string Nome,
    int Jogos,
    int Vitorias,
    int Empates,
    int Derrotas,
    int GolsPro,
    int GolsContra,
    int SaldoGols,
    int Pontos,
    int Posicao,
    bool Classificado,
    bool Wildcard);

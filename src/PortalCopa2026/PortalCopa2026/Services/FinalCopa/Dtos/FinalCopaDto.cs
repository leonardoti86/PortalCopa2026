using PortalCopa2026.Services.FaseEliminatoria.Dtos;

namespace PortalCopa2026.Services.FinalCopa.Dtos;

public record FinalCopaDto(
    FaseEliminatoriaJogoDto TerceiroLugar,
    FaseEliminatoriaJogoDto Final,
    CampeaoDto? Campeao);

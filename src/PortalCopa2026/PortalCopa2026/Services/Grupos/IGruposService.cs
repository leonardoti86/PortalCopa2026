using PortalCopa2026.Services.Grupos.Dtos;

namespace PortalCopa2026.Services.Grupos;

public interface IGruposService
{
    Task<GruposEstadoDto> ObterEstadoAtualAsync(CancellationToken cancellationToken = default);

    Task<GruposEstadoDto> AtualizarPlacarOficialAsync(int jogoId, int? placarMandante, int? placarVisitante, CancellationToken cancellationToken = default);
}

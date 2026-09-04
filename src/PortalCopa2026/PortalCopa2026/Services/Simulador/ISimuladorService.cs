using PortalCopa2026.Services.Simulador.Dtos;

namespace PortalCopa2026.Services.Simulador;

public interface ISimuladorService
{
    Task<SimuladorEstadoDto> ObterEstadoAtualAsync(CancellationToken cancellationToken = default);

    Task<SimuladorEstadoDto> AtualizarPlacarAsync(int jogoId, int? placarMandante, int? placarVisitante, CancellationToken cancellationToken = default);
}

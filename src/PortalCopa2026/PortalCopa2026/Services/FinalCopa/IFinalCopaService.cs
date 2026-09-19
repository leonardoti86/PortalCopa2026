using PortalCopa2026.Services.FinalCopa.Dtos;

namespace PortalCopa2026.Services.FinalCopa;

public interface IFinalCopaService
{
    Task<FinalCopaDto> ObterAsync(CancellationToken cancellationToken = default);

    Task<FinalCopaDto> AtualizarPlacarOficialAsync(
        int jogoId,
        int? placarMandante,
        int? placarVisitante,
        int? placarPenaltisMandante,
        int? placarPenaltisVisitante,
        CancellationToken cancellationToken = default);
}

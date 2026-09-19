using PortalCopa2026.Services.FaseEliminatoria.Dtos;

namespace PortalCopa2026.Services.FaseEliminatoria;

public interface IFaseEliminatoriaService
{
    Task<IReadOnlyList<FaseEliminatoriaJogoDto>> ObterSegundaFaseAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FaseEliminatoriaJogoDto>> ObterOitavasAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FaseEliminatoriaJogoDto>> ObterQuartasAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FaseEliminatoriaJogoDto>> ObterSemifinaisAsync(CancellationToken cancellationToken = default);

    Task<FaseEliminatoriaJogoDto> AtualizarPlacarOficialAsync(
        int jogoId,
        int? placarMandante,
        int? placarVisitante,
        int? placarPenaltisMandante,
        int? placarPenaltisVisitante,
        CancellationToken cancellationToken = default);
}

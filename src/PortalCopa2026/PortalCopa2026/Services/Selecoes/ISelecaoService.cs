using PortalCopa2026.Services.Selecoes.Dtos;

namespace PortalCopa2026.Services.Selecoes;

public interface ISelecaoService
{
    Task<IReadOnlyList<SelecaoResumoDto>> ObterListagemAsync(CancellationToken cancellationToken = default);

    Task<SelecaoDetalheDto?> ObterDetalheAsync(int selecaoId, CancellationToken cancellationToken = default);
}

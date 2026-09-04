using PortalCopa2026.Services.Jogos.Dtos;

namespace PortalCopa2026.Services.Jogos;

public interface IJogosService
{
    Task<IReadOnlyList<JogoDto>> ObterJogosFaseGruposAsync(CancellationToken cancellationToken = default);
}

using Microsoft.EntityFrameworkCore;
using PortalCopa2026.Data;
using PortalCopa2026.Services.Copa;
using PortalCopa2026.Services.Jogos.Dtos;

namespace PortalCopa2026.Services.Jogos;

public class JogosService(AppDbContext context) : IJogosService
{
    public async Task<IReadOnlyList<JogoDto>> ObterJogosFaseGruposAsync(CancellationToken cancellationToken = default)
    {
        var jogos = await JogosFaseGruposQuery.ObterJogosFaseGrupos(context)
            .OrderBy(j => j.Data)
            .ThenBy(j => j.Horario)
            .Select(j => new JogoDto(
                j.Id,
                j.Data,
                j.Horario,
                j.Grupo!.Codigo,
                j.SelecaoMandante!.Nome,
                j.SelecaoMandante!.Codigo,
                j.SelecaoVisitante!.Nome,
                j.SelecaoVisitante!.Codigo,
                j.Estadio,
                j.Cidade))
            .ToListAsync(cancellationToken);

        return jogos;
    }
}

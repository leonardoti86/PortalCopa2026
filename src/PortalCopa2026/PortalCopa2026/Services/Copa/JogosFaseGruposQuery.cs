using PortalCopa2026.Data;
using PortalCopa2026.Models.Copa;

namespace PortalCopa2026.Services.Copa;

public static class JogosFaseGruposQuery
{
    public const string FaseGrupos = "Primeira Fase";

    public static IQueryable<Jogo> ObterJogosFaseGrupos(AppDbContext context) =>
        context.Jogos.Where(j => j.Fase == FaseGrupos
            && j.Grupo != null
            && j.SelecaoMandante != null
            && j.SelecaoVisitante != null);
}

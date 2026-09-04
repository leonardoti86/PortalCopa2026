using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PortalCopa2026.Data;
using PortalCopa2026.Services.Selecoes.Dtos;

namespace PortalCopa2026.Services.Selecoes;

public class SelecaoService(AppDbContext context) : ISelecaoService
{
    private static readonly StringComparer NomeComparer = StringComparer.Create(CultureInfo.GetCultureInfo("pt-BR"), ignoreCase: false);

    public async Task<IReadOnlyList<SelecaoResumoDto>> ObterListagemAsync(CancellationToken cancellationToken = default)
    {
        var selecoes = await context.Selecoes
            .Select(s => new { s.Id, s.Nome, s.Codigo, GrupoCodigo = s.Grupo!.Codigo })
            .ToListAsync(cancellationToken);

        return selecoes
            .OrderBy(s => s.Nome, NomeComparer)
            .Select(s => new SelecaoResumoDto(s.Id, s.Nome, s.Codigo, s.GrupoCodigo, ConstruirFlagUrl(s.Codigo)))
            .ToList();
    }

    public async Task<SelecaoDetalheDto?> ObterDetalheAsync(int selecaoId, CancellationToken cancellationToken = default)
    {
        var selecao = await context.Selecoes
            .Include(s => s.Grupo)
            .Include(s => s.RankingFifa)
            .Include(s => s.Jogadores)
            .FirstOrDefaultAsync(s => s.Id == selecaoId, cancellationToken);

        if (selecao is null)
        {
            return null;
        }

        var jogadores = selecao.Jogadores
            .OrderBy(j => j.Nome, NomeComparer)
            .Select(j => new JogadorDto(j.Nome, j.Posicao, j.Idade, j.Gols))
            .ToList();

        return new SelecaoDetalheDto(
            selecao.Nome,
            selecao.Codigo,
            selecao.Grupo!.Codigo,
            ConstruirFlagUrl(selecao.Codigo),
            selecao.Tecnico,
            selecao.RankingFifa?.Posicao,
            selecao.RankingFifa?.Pontos,
            jogadores);
    }

    private static string ConstruirFlagUrl(string codigo) =>
        $"https://api.fifa.com/api/v3/picture/flags-sq-4/{codigo}";
}

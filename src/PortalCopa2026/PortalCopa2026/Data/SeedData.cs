using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PortalCopa2026.Models.Copa;

namespace PortalCopa2026.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Selecoes.AnyAsync(cancellationToken))
        {
            return;
        }

        var teams = LoadJson<List<TeamSeedDto>>("teams.json");
        var groups = LoadJson<List<GroupSeedDto>>("groups.json");
        var players = LoadJson<Dictionary<string, List<PlayerSeedDto>>>("players.json");
        var matches = LoadJson<List<MatchSeedDto>>("matches.json");

        var grupos = groups.ToDictionary(g => g.Code, g => new Grupo { Codigo = g.Code });

        var selecoes = new Dictionary<string, Selecao>();
        foreach (var team in teams)
        {
            var selecao = new Selecao
            {
                Nome = team.Name,
                Codigo = team.Code,
                Tecnico = team.Coach,
                Grupo = grupos[team.Group]
            };

            if (team.Ranking is not null)
            {
                selecao.RankingFifa = new RankingFifa
                {
                    Posicao = team.Ranking.Position,
                    Pontos = team.Ranking.Points
                };
            }

            selecoes[team.Name] = selecao;
        }

        foreach (var (teamName, teamPlayers) in players)
        {
            if (!selecoes.TryGetValue(teamName, out var selecao))
            {
                continue;
            }

            foreach (var player in teamPlayers)
            {
                selecao.Jogadores.Add(new Jogador
                {
                    Nome = player.Name,
                    Posicao = player.Position,
                    Idade = player.Age,
                    Gols = player.Goals,
                    ParticipacoesCopa = 0
                });
            }
        }

        var jogos = new List<Jogo>();
        foreach (var match in matches)
        {
            var mandanteNome = MatchSeedDto.ResolveTeamName(match.Home);
            var visitanteNome = MatchSeedDto.ResolveTeamName(match.Away);

            jogos.Add(new Jogo
            {
                Data = DateOnly.Parse(match.Date),
                Horario = TimeOnly.Parse(match.Time),
                Fase = match.Phase,
                Cidade = match.City,
                Estadio = match.Stadium,
                Grupo = match.Group is not null ? grupos.GetValueOrDefault(match.Group) : null,
                SelecaoMandante = mandanteNome is not null ? selecoes.GetValueOrDefault(mandanteNome) : null,
                SelecaoVisitante = visitanteNome is not null ? selecoes.GetValueOrDefault(visitanteNome) : null
            });
        }

        context.Grupos.AddRange(grupos.Values);
        context.Selecoes.AddRange(selecoes.Values);
        context.Jogos.AddRange(jogos);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static T LoadJson<T>(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{assembly.GetName().Name}.Data.SeedJson.{fileName}";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Recurso embutido não encontrado: {resourceName}");

        return JsonSerializer.Deserialize<T>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException($"Falha ao desserializar {fileName}");
    }
}

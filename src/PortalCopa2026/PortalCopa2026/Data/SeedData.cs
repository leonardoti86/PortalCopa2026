using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PortalCopa2026.Models.Copa;
using PortalCopa2026.Services.Copa;

namespace PortalCopa2026.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        await SeedBaseAsync(context, cancellationToken);

        // Roda sempre, mesmo quando SeedBaseAsync já não faz nada (banco existente): idempotente
        // por fase, para completar Quartas/Semifinais sem recriar dados já persistidos - design.md
        // (criar-jogos-quartas-semifinais) - Decisão 5.
        await SeedQuartasSemifinaisAsync(context, cancellationToken);

        // Mesmo mecanismo idempotente, completando Terceiro Lugar e Final - design.md
        // (criar-jogo-teceiro-lugar-final) - Decisão 5.
        await SeedTerceiroLugarFinalAsync(context, cancellationToken);
    }

    private static async Task SeedBaseAsync(AppDbContext context, CancellationToken cancellationToken)
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

        var jogosSegundaFasePorOrdem = SeedJogosSegundaFase(context, selecoes);
        SeedJogosOitavas(context, jogosSegundaFasePorOrdem);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static Dictionary<int, Jogo> SeedJogosSegundaFase(AppDbContext context, Dictionary<string, Selecao> selecoes)
    {
        var matches = LoadJson<List<SegundaFaseMatchSeedDto>>("matches_segunda_fase.json");
        var jogosPorOrdem = new Dictionary<int, Jogo>();

        foreach (var match in matches)
        {
            var jogo = new Jogo
            {
                Ordem = match.Order,
                Data = DateOnly.Parse(match.Date),
                Horario = TimeOnly.Parse(match.Time),
                Fase = JogosFaseEliminatoriaQuery.FaseSegundaFase,
                Cidade = match.City,
                Estadio = match.Stadium,
                SelecaoMandante = selecoes.GetValueOrDefault(match.Home),
                SelecaoVisitante = selecoes.GetValueOrDefault(match.Away)
            };

            jogosPorOrdem[match.Order] = jogo;
            context.Jogos.Add(jogo);
        }

        return jogosPorOrdem;
    }

    private static void SeedJogosOitavas(AppDbContext context, Dictionary<int, Jogo> jogosSegundaFasePorOrdem)
    {
        var matches = LoadJson<List<MataMataMatchSeedDto>>("matches_oitavas.json");

        foreach (var match in matches)
        {
            var jogo = new Jogo
            {
                Ordem = match.Order,
                Data = DateOnly.Parse(match.Date),
                Horario = TimeOnly.Parse(match.Time),
                Fase = JogosFaseEliminatoriaQuery.FaseOitavas,
                Cidade = match.City,
                Estadio = match.Stadium,
                JogoOrigemMandante = jogosSegundaFasePorOrdem[match.HomeSourceOrder],
                JogoOrigemVisitante = jogosSegundaFasePorOrdem[match.AwaySourceOrder]
            };

            context.Jogos.Add(jogo);
        }
    }

    // Idempotente por fase: roda em toda inicialização (mesmo com o banco já populado),
    // completando apenas as fases ainda ausentes, sem recriar nem duplicar jogo nenhum -
    // design.md (criar-jogos-quartas-semifinais) - Decisão 5.
    private static async Task SeedQuartasSemifinaisAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        // Passo 1: remove os jogos legados inertes de "Quartas"/"Semifinal" (semeados antes desta
        // change, sem número oficial e sem jogo de origem) - nunca tiveram tela nem resultado.
        var jogosLegados = await context.Jogos
            .Where(j => (j.Fase == "Quartas" || j.Fase == "Semifinal")
                && j.Ordem == null
                && j.JogoOrigemMandanteId == null
                && j.JogoOrigemVisitanteId == null)
            .ToListAsync(cancellationToken);

        if (jogosLegados.Count > 0)
        {
            context.Jogos.RemoveRange(jogosLegados);
            await context.SaveChangesAsync(cancellationToken);
        }

        // Passo 2: Quartas de Final, ligadas aos jogos de origem das Oitavas por Fase + Ordem.
        if (!await context.Jogos.AnyAsync(j => j.Fase == JogosFaseEliminatoriaQuery.FaseQuartas, cancellationToken))
        {
            var oitavasPorOrdem = await context.Jogos
                .Where(j => j.Fase == JogosFaseEliminatoriaQuery.FaseOitavas)
                .ToDictionaryAsync(j => j.Ordem!.Value, cancellationToken);

            var matches = LoadJson<List<MataMataMatchSeedDto>>("matches_quartas.json");
            foreach (var match in matches)
            {
                context.Jogos.Add(new Jogo
                {
                    Ordem = match.Order,
                    Data = DateOnly.Parse(match.Date),
                    Horario = TimeOnly.Parse(match.Time),
                    Fase = JogosFaseEliminatoriaQuery.FaseQuartas,
                    Cidade = match.City,
                    Estadio = match.Stadium,
                    JogoOrigemMandante = oitavasPorOrdem[match.HomeSourceOrder],
                    JogoOrigemVisitante = oitavasPorOrdem[match.AwaySourceOrder]
                });
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        // Passo 3: Semifinais, ligadas aos jogos de origem das Quartas por Fase + Ordem.
        if (!await context.Jogos.AnyAsync(j => j.Fase == JogosFaseEliminatoriaQuery.FaseSemifinais, cancellationToken))
        {
            var quartasPorOrdem = await context.Jogos
                .Where(j => j.Fase == JogosFaseEliminatoriaQuery.FaseQuartas)
                .ToDictionaryAsync(j => j.Ordem!.Value, cancellationToken);

            var matches = LoadJson<List<MataMataMatchSeedDto>>("matches_semifinais.json");
            foreach (var match in matches)
            {
                context.Jogos.Add(new Jogo
                {
                    Ordem = match.Order,
                    Data = DateOnly.Parse(match.Date),
                    Horario = TimeOnly.Parse(match.Time),
                    Fase = JogosFaseEliminatoriaQuery.FaseSemifinais,
                    Cidade = match.City,
                    Estadio = match.Stadium,
                    JogoOrigemMandante = quartasPorOrdem[match.HomeSourceOrder],
                    JogoOrigemVisitante = quartasPorOrdem[match.AwaySourceOrder]
                });
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }

    // Terceiro Lugar e Final, ligados às Semifinais por Ordem (1 e 2). O grafo persistido é
    // idêntico para os dois jogos (mesmas Semifinais de origem); a diferença entre "perdedor" e
    // "vencedor" fica inteiramente na leitura (FinalCopaService), não no armazenamento -
    // design.md (criar-jogo-teceiro-lugar-final) - Decisão 5. Cada fase é checada
    // independentemente para permanecer idempotente mesmo se só uma delas já existir.
    private static async Task SeedTerceiroLugarFinalAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        // Passo 1: remove os jogos legados inertes de "Terceiro Lugar"/"Final" (semeados por
        // matches.json antes desta change, com placeholders "type": "winner" que o SeedBaseAsync
        // nunca resolveu - sem Ordem nem jogo de origem, nunca tiveram tela nem resultado), mesmo
        // padrão do Passo 1 de SeedQuartasSemifinaisAsync.
        var jogosLegados = await context.Jogos
            .Where(j => (j.Fase == JogosFaseEliminatoriaQuery.FaseTerceiroLugar || j.Fase == JogosFaseEliminatoriaQuery.FaseFinal)
                && j.Ordem == null
                && j.JogoOrigemMandanteId == null
                && j.JogoOrigemVisitanteId == null)
            .ToListAsync(cancellationToken);

        if (jogosLegados.Count > 0)
        {
            context.Jogos.RemoveRange(jogosLegados);
            await context.SaveChangesAsync(cancellationToken);
        }

        var precisaTerceiroLugar = !await context.Jogos.AnyAsync(j => j.Fase == JogosFaseEliminatoriaQuery.FaseTerceiroLugar, cancellationToken);
        var precisaFinal = !await context.Jogos.AnyAsync(j => j.Fase == JogosFaseEliminatoriaQuery.FaseFinal, cancellationToken);

        if (!precisaTerceiroLugar && !precisaFinal)
        {
            return;
        }

        var semifinaisPorOrdem = await context.Jogos
            .Where(j => j.Fase == JogosFaseEliminatoriaQuery.FaseSemifinais)
            .ToDictionaryAsync(j => j.Ordem!.Value, cancellationToken);

        if (precisaTerceiroLugar)
        {
            var matches = LoadJson<List<MataMataMatchSeedDto>>("matches_terceiro_lugar.json");
            foreach (var match in matches)
            {
                context.Jogos.Add(new Jogo
                {
                    Ordem = match.Order,
                    Data = DateOnly.Parse(match.Date),
                    Horario = TimeOnly.Parse(match.Time),
                    Fase = JogosFaseEliminatoriaQuery.FaseTerceiroLugar,
                    Cidade = match.City,
                    Estadio = match.Stadium,
                    JogoOrigemMandante = semifinaisPorOrdem[match.HomeSourceOrder],
                    JogoOrigemVisitante = semifinaisPorOrdem[match.AwaySourceOrder]
                });
            }
        }

        if (precisaFinal)
        {
            var matches = LoadJson<List<MataMataMatchSeedDto>>("matches_final.json");
            foreach (var match in matches)
            {
                context.Jogos.Add(new Jogo
                {
                    Ordem = match.Order,
                    Data = DateOnly.Parse(match.Date),
                    Horario = TimeOnly.Parse(match.Time),
                    Fase = JogosFaseEliminatoriaQuery.FaseFinal,
                    Cidade = match.City,
                    Estadio = match.Stadium,
                    JogoOrigemMandante = semifinaisPorOrdem[match.HomeSourceOrder],
                    JogoOrigemVisitante = semifinaisPorOrdem[match.AwaySourceOrder]
                });
            }
        }

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

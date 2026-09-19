## 1. Modelo de Dados

- [x] 1.1 Adicionar a `Models/Copa/Jogo.cs`: `JogoOrigemMandanteId`/`JogoOrigemMandante`, `JogoOrigemVisitanteId`/`JogoOrigemVisitante` (self-reference, `int?`), `PlacarPenaltisMandante`/`PlacarPenaltisVisitante` (`int?`) e `Ordem` (`int?`, número oficial do jogo dentro da fase eliminatória, sem FK — design.md Decisão 1).
- [x] 1.2 Configurar em `Data/AppDbContext.cs` as duas FKs auto-referenciadas de `Jogo` para `Jogo` (`WithMany()`, `OnDelete(DeleteBehavior.Restrict)`), mesmo padrão de `SelecaoMandante`/`SelecaoVisitante`.
- [x] 1.3 Gerar a migration EF Core (`dotnet ef migrations add AddFaseEliminatoria`) e revisar o `Up`/`Down` gerado.

## 2. Regra de Vencedor

- [x] 2.1 Criar `Services/Copa/JogoEliminatorioResolver.cs` com `ResolverSelecaoId(Jogo jogo, bool mandante)` (recursivo pelos jogos de origem) e `ObterVencedorId(Jogo jogo)` (placar oficial decide; empate exige pênaltis decisivos; caso contrário `null`), conforme design.md - Decisão 2.
- [x] 2.2 Criar `Services/Copa/JogosFaseEliminatoriaQuery.cs` com as constantes `FaseSegundaFase = "Segunda Fase"` e `FaseOitavas = "Oitavas de Final"` (design.md - Decisão 4).

## 3. Dados Oficiais e Seed

- [x] 3.1 Extrair as 16 entradas `r32-*` de `Data/SeedJson/matches.json` para `Data/SeedJson/matches_segunda_fase.json`, campos `order`/`date`/`time`/`home`/`away`/`city`/`stadium` (design.md - Decisão 3, incluindo a correção do nome "Bósnia e Herzegovina").
- [x] 3.2 Extrair as 8 entradas `r16-*` de `Data/SeedJson/matches.json` para `Data/SeedJson/matches_oitavas.json`, campos `order`/`date`/`time`/`city`/`stadium`/`homeSourceOrder`/`awaySourceOrder` (design.md - Decisão 3, incluindo a normalização de cidade para Seattle/Vancouver/Nova York-Nova Jersey/Cidade do México).
- [x] 3.3 Remover de `Data/SeedJson/matches.json` as 16 entradas `r32-*` e as 8 `r16-*` extraídas (mantendo `grp-*` e as fases futuras já presentes intocadas).
- [x] 3.4 Adicionar os novos JSONs como embedded resource (mesmo `.csproj`/pasta `SeedJson` dos arquivos existentes).
- [x] 3.5 Criar os DTOs de seed correspondentes em `Data/SeedDtos.cs` (`SegundaFaseMatchSeedDto`, `OitavasMatchSeedDto`).
- [x] 3.6 Implementar `SeedData.SeedJogosSegundaFase(AppDbContext, Dictionary<string, Selecao>)` retornando o dicionário `ordem -> Jogo` criado.
- [x] 3.7 Implementar `SeedData.SeedJogosOitavas(AppDbContext, Dictionary<int, Jogo> jogosSegundaFasePorOrdem)`, ligando `JogoOrigemMandante`/`JogoOrigemVisitante` sem definir `SelecaoMandanteId`/`SelecaoVisitanteId`.
- [x] 3.8 Chamar os dois métodos novos em `SeedData.SeedAsync`, na ordem correta (Segunda Fase antes de Oitavas), antes do único `SaveChangesAsync` existente.

## 4. Serviço de Apresentação

- [x] 4.1 Criar DTO `FaseEliminatoriaJogoDto` (ordem, data, horário, estádio, cidade, nome/código de seleção mandante e visitante — nulos quando pendente —, `OrigemMandanteOrdem`/`OrigemVisitanteOrdem` — nulos quando o lado já é uma seleção real, usados para o texto "Vencedor Segunda Fase N" — placar oficial, placar de pênaltis) em `Services/FaseEliminatoria/Dtos`.
- [x] 4.2 Criar `IFaseEliminatoriaService`/`FaseEliminatoriaService` (`Services/FaseEliminatoria`) com `ObterSegundaFaseAsync()`, `ObterOitavasAsync()` (usando `JogoEliminatorioResolver` para resolver seleção/vencedor de cada jogo das Oitavas) e `AtualizarPlacarOficialAsync(jogoId, placarMandante, placarVisitante, placarPenaltisMandante, placarPenaltisVisitante)`.
- [x] 4.3 Em `AtualizarPlacarOficialAsync`, validar e rejeitar (`InvalidOperationException`) um placar de pênaltis empatado e a tentativa de registrar placar em um jogo das Oitavas com mandante ou visitante ainda pendente (design.md - Decisão 5).
- [x] 4.4 Registrar `IFaseEliminatoriaService` no DI (`Program.cs`), ao lado dos demais serviços `AddScoped`.

## 5. Interface

- [x] 5.1 Adicionar "Fase2" (`/fase2`) e "Oitavas" (`/oitavas`) a `Components/Layout/NavMenu.razor`.
- [x] 5.2 Criar `Components/Pages/FaseEliminatoria/FaseEliminatoriaJogoCard.razor`, componente compartilhado entre Fase2 e Oitavas (design.md - Decisão 6): reaproveita `lp-match-card` (`JogoCard.razor`) para exibição e `sim-jogo__placares` (`GrupoJogo.razor`) para o placar; quando `OrigemMandanteOrdem`/`OrigemVisitanteOrdem` estiver preenchido, exibe "Vencedor Segunda Fase N" no lugar do nome/bandeira daquele lado e mantém os campos de placar desabilitados; exibe os campos de placar de pênaltis apenas quando o placar oficial informado estiver empatado, bloqueando no próprio input um placar de pênaltis também empatado antes de notificar o componente pai com os quatro valores.
- [x] 5.3 Criar `Components/Pages/Fase2.razor`, listando os 16 jogos da Segunda Fase com `FaseEliminatoriaJogoCard`, chamando `AtualizarPlacarOficialAsync` a cada alteração de placar.
- [x] 5.4 Criar `Components/Pages/Oitavas.razor`, listando os 8 jogos das Oitavas com o mesmo componente `FaseEliminatoriaJogoCard`.

## 6. Verificação

- [x] 6.1 Rodar a aplicação com banco vazio e confirmar 16 jogos na Segunda Fase e 8 nas Oitavas (dados batendo com `./fontes/copa2026_jogos_segunda_fase.txt` e `./fontes/copa2026_jogos_oitavas.txt`).
- [x] 6.2 Registrar o placar de um jogo da Segunda Fase que é origem de um confronto das Oitavas e confirmar, na página Oitavas, a seleção vencedora aparecendo imediatamente naquele lado.
- [x] 6.3 Registrar um placar empatado em um jogo eliminatório, confirmar que o vencedor fica indefinido até o placar de pênaltis ser informado, e que informar os pênaltis define o vencedor e propaga corretamente.
- [x] 6.4 Alterar um placar da Segunda Fase já registrado (trocando o vencedor) e confirmar que a página Oitavas atualiza o confronto dependente para a nova seleção vencedora.
- [x] 6.5 Confirmar que um confronto das Oitavas com um lado ainda pendente não permite registrar placar, e que tentar registrar um placar de pênaltis empatado é rejeitado (sem persistir).

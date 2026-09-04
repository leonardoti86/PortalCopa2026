## 1. DTOs do Simulador

- [x] 1.1 Criar `Services/Simulador/Dtos/SimuladorJogoDto.cs` (record: JogoId, Grupo, SelecaoMandante, CodigoMandante, SelecaoVisitante, CodigoVisitante, PlacarMandante?, PlacarVisitante?)
- [x] 1.2 Criar `Services/Simulador/Dtos/ClassificacaoTimeDto.cs` (record: Codigo, Nome, Jogos, Vitorias, Empates, Derrotas, GolsPro, GolsContra, SaldoGols, Pontos, Posicao, Classificado, Wildcard)
- [x] 1.3 Criar `Services/Simulador/Dtos/ClassificacaoGrupoDto.cs` (record: Grupo, IReadOnlyList<ClassificacaoTimeDto> Times)
- [x] 1.4 Criar `Services/Simulador/Dtos/SimuladorEstadoDto.cs` (record: IReadOnlyList<SimuladorJogoDto> Jogos, IReadOnlyList<ClassificacaoGrupoDto> Classificacoes, IReadOnlyList<ClassificacaoTimeDto> RankingTerceiros, int TotalJogos, int JogosSimulados)

## 2. Serviço do Simulador

- [x] 2.1 Criar `Services/Simulador/ISimuladorService.cs` com `Task<SimuladorEstadoDto> ObterEstadoAtualAsync(CancellationToken ct = default)` e `Task<SimuladorEstadoDto> AtualizarPlacarAsync(int jogoId, int? placarMandante, int? placarVisitante, CancellationToken ct = default)`
- [x] 2.2 Implementar em `SimuladorService.cs` o get-or-create da simulação atual (busca a `Simulacao` mais antiga; cria uma nova se não existir)
- [x] 2.3 Implementar carga dos 72 jogos da fase de grupos com seus placares simulados (join com `SimulacaoJogo` da simulação atual)
- [x] 2.4 Implementar upsert/remoção de `SimulacaoJogo` em `AtualizarPlacarAsync` (cria/atualiza quando há placar em ambos os campos; remove o registro quando ambos os placares vierem nulos) com `SaveChangesAsync` imediato
- [x] 2.5 Implementar o cálculo de estatísticas base por seleção (J/V/E/D/GP/GC/SG/Pts) a partir dos jogos simulados de cada grupo
- [x] 2.6 Implementar a cascata de desempate por grupo (Pts → SG → GP → confronto direto (mini-Pts/mini-SG entre empatados) → Ranking FIFA), atribuindo Posição 1–4 e marcando `Classificado = true` para 1º/2º — aplicando o fallback determinístico do design.md (Decisão 4) para seleção sem `RankingFifa` cadastrado (tratar como pior posição possível; desempate final por `Selecao.Codigo`)
- [x] 2.7 Implementar o ranking geral dos 12 terceiros colocados (Pts → SG → GP → Ranking FIFA, sem confronto direto) e marcar `Wildcard = true` para os 8 primeiros — aplicando o mesmo fallback da tarefa 2.6 para seleção sem `RankingFifa` cadastrado
- [x] 2.8 Registrar `ISimuladorService`/`SimuladorService` como `AddScoped` em `Program.cs`

## 3. Componentes de apresentação

- [x] 3.1 Criar `Components/Pages/Simulador/SimuladorJogo.razor` (parâmetros: `SimuladorJogoDto Jogo`, `EventCallback<(int JogoId, int? PlacarMandante, int? PlacarVisitante)> PlacarAlterado`) com dois inputs numéricos de placar
- [x] 3.2 Criar `Components/Pages/Simulador/ClassificacaoGrupo.razor` (parâmetro: `ClassificacaoGrupoDto Classificacao`) — tabela Bootstrap responsiva (`table-responsive`) com colunas alinhadas (posição, seleção, J/V/E/D/GP/GC/SG/Pts) e destaque visual para `Classificado` (1º/2º) e `Wildcard` (3º quando aplicável)
- [x] 3.3 Criar `Components/Pages/Simulador/SimuladorGrupo.razor` (parâmetros: `IReadOnlyList<SimuladorJogoDto> Jogos`, `ClassificacaoGrupoDto Classificacao`, `EventCallback<(int JogoId, int? PlacarMandante, int? PlacarVisitante)> PlacarAlterado`) — compõe `SimuladorJogo` (lista) e `ClassificacaoGrupo` em layout responsivo (lado a lado em telas largas, empilhado em telas estreitas)
- [x] 3.4 Criar `Components/Pages/Simulador/SimuladorResumo.razor` (parâmetros: `IReadOnlyList<ClassificacaoTimeDto> RankingTerceiros`, `int TotalJogos`, `int JogosSimulados`) — progresso da simulação e tabela do ranking geral dos 12 terceiros com destaque dos 8 primeiros (wildcard)

## 4. Página do Simulador

- [x] 4.1 Criar `Components/Pages/Simulador/Simulador.razor` com `@page "/simulador"`, injetando `ISimuladorService`
- [x] 4.2 Carregar o estado atual (`ObterEstadoAtualAsync`) em `OnInitializedAsync`, restaurando placares e classificações já persistidos
- [x] 4.3 Implementar seleção de grupo ativo (abas A–L) e renderizar o `SimuladorGrupo` do grupo selecionado
- [x] 4.4 Tratar o evento de alteração de placar: chamar `AtualizarPlacarAsync` e atualizar o estado local (jogos, classificações do grupo afetado e ranking de terceiros) com o retorno
- [x] 4.5 Renderizar `SimuladorResumo` com o progresso geral e o ranking de terceiros/wildcards

## 5. Validação

- [x] 5.1 Rodar `dotnet build` na solução e confirmar ausência de erros/warnings novos
- [x] 5.2 Validar manualmente: informar placares em um grupo e conferir classificação recalculada (J/V/E/D/GP/GC/SG/Pts corretos)
- [x] 5.3 Validar cenário de desempate por saldo de gols, por confronto direto (2 e 3 seleções empatadas) e por Ranking FIFA, incluindo o fallback para seleção sem Ranking FIFA cadastrado (ex.: Bósnia e Herzegovina no Grupo B, ou Arábia Saudita/Cabo Verde no Grupo H)
- [x] 5.4 Validar destaque de classificados (1º/2º) e do 3º colocado como wildcard, incluindo a transição de wildcard ao alterar placares de outros grupos
- [x] 5.5 Validar persistência automática: informar placares, recarregar a página e confirmar que os placares e a classificação são restaurados sem ação adicional
- [x] 5.6 Confirmar que jogos do mata-mata não aparecem no simulador e que nenhum dado fictício foi introduzido

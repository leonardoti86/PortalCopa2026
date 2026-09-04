## Context

Hoje só existe classificação "hipotética" no Simulador (`SimuladorService`), calculada a partir de `SimulacaoJogo` (placar simulado, por sessão de simulação). A entidade `Jogo` não tem campo de placar — não há como registrar um resultado oficial. `SimuladorService` já implementa toda a cascata de desempate oficial (RN-01) e a lógica de estatísticas por seleção (ver `SimuladorService.cs` — `EstatisticasTime`, `ConstruirEstatisticasBase`, `OrdenarComDesempate`, `DesempatarBloco`, `DesempatarPorRankingFifa`), incluindo o tratamento de "sem Ranking FIFA cadastrado" e o desempate final por código alfabético. Ver proposal.md - Why/What Changes para motivação.

## Goals / Non-Goals

**Goals:**
- Persistir o resultado oficial no próprio `Jogo` (`PlacarMandante`/`PlacarVisitante`), sem misturar com `SimulacaoJogo`.
- Reaproveitar a cascata de desempate e o cálculo de estatísticas já validados no Simulador, sem duplicar a lógica para os resultados oficiais.
- Página de Grupos com a mesma estrutura de interação do Simulador (abas por grupo, tabela de classificação, edição inline de placar), fiel ao protótipo `grupos.html`.

**Non-Goals:**
- Alterar o comportamento ou os dados do Simulador (`SimulacaoJogo` continua isolado).
- Implementar Fair Play (cartões) ou sorteio real — ambos permanecem fora do alcance por falta de dado oficial/mecanismo (mesma decisão já tomada no Simulador).
- Deep-link do botão "Simular" para pré-selecionar o grupo de origem no Simulador (ver proposal.md - Impact).
- Redesenhar a navegação global do site (navbar do protótipo, com Home/Jogos/Grupos/Seleções/Ranking/Simulador e menu mobile). Hoje nenhuma página existente (Jogos, Simulador, LandingPage) está linkada em `NavMenu.razor` — a navegação real acontece via botões contextuais dentro das páginas. Esta change adiciona apenas o link de Grupos ao `NavMenu.razor` atual como ajuste mínimo, sem resolver essa lacuna pré-existente; uma navegação global fiel ao protótipo fica para uma change futura dedicada.

## Decisions

### Decisão 1 — Placar oficial vive em `Jogo`, não em uma tabela nova
Adicionar `PlacarMandante` e `PlacarVisitante` (`int?`) diretamente em `Models/Copa/Jogo.cs`, com uma migração EF Core (`AddPlacarOficialJogo` ou nome equivalente). Alternativa considerada: criar uma tabela `ResultadoOficial` separada, espelhando `SimulacaoJogo`. Rejeitada porque o resultado oficial é uma propriedade do próprio jogo (não há múltiplas "versões" concorrentes como no Simulador, que precisa de uma linha por `SimulacaoId`), então uma tabela extra só adicionaria um join desnecessário em toda leitura.

### Decisão 2 — Extrair a cascata de desempate para um componente compartilhado
Mover `EstatisticasTime`, `ConstruirEstatisticasBase`, `OrdenarComDesempate`, `DesempatarBloco` e `DesempatarPorRankingFifa` de `SimuladorService` para um novo componente reutilizável (ex.: `Services/Copa/ClassificacaoCalculator.cs`, estático ou injetável), operando sobre uma abstração comum de "jogo com placar resolvido" (grupo, seleções, placar mandante/visitante nullable) em vez do DTO específico do Simulador. `SimuladorService` passa a montar essa lista a partir de `Jogo` + `SimulacaoJogo` (como já faz hoje) e `GruposService` a monta a partir de `Jogo.PlacarMandante/PlacarVisitante` diretamente. Ambos os serviços consomem o mesmo cálculo de classificação por grupo e o mesmo ranking de terceiros colocados (top 8 de 12) para a indicação de "possível melhor terceiro" na página de Grupos.
Alternativa considerada: duplicar a lógica dentro de `GruposService`. Rejeitada por violar a diretriz do projeto de evitar duplicação de código e por criar risco de os dois cálculos divergirem silenciosamente ao longo do tempo (ex.: uma correção de desempate aplicada só em um dos dois).

### Decisão 3 — `GruposService` como novo serviço, seguindo o padrão de `SimuladorService`/`JogosService`
`IGruposService`/`GruposService` expõe: obter o estado de todos os grupos (classificações + jogos, para popular as 12 abas de uma vez, como o Simulador já faz) e atualizar o placar oficial de um jogo (`AtualizarPlacarOficialAsync`), retornando o estado recalculado. Acesso a dados só via este serviço, nunca `AppDbContext` direto em componentes Razor (regra do projeto).
O DTO de jogo do grupo usado pela página SHALL incluir `Data` e `Horario`, além de grupo, seleções e placar oficial. Isso NÃO é um reaproveitamento direto de `SimuladorJogoDto` (`Services/Simulador/Dtos/SimuladorJogoDto.cs`): esse DTO não tem `Data`/`Horario` porque o Simulador nunca precisou exibi-los — o Simulador pode continuar usando seu DTO atual sem alteração.

### Decisão 4 — Componentização da página seguindo o padrão do Simulador
`Components/Pages/Grupos/Grupos.razor` reaproveita a estrutura de `Simulador.razor` (abas Bootstrap `nav-tabs`, estado do grupo ativo em memória, recarrega o estado após cada alteração de placar). Subcomponentes: tabela de classificação (baseada em `ClassificacaoGrupo.razor`) e um item de jogo com edição de placar (baseado em `SimuladorJogo.razor`, adaptado para emitir `(JogoId, PlacarMandante, PlacarVisitante)` contra `Jogo` em vez de `SimulacaoJogo`). Onde a estrutura de UI for idêntica (ex.: inputs numéricos de placar, badges de classificado/wildcard), reaproveitar o componente existente ou extrair um componente comum em `Components/Shared`, em vez de duplicar o Razor.
Seguindo o padrão já estabelecido por `landing-page.css`, `jogos.css` e `simulador.css` (cada capacidade com seu próprio arquivo, registrado como `<link>` em `Components/App.razor`), a página de Grupos ganha seu próprio `wwwroot/css/grupos.css`, também registrado em `Components/App.razor`.

### Decisão 5 — Extrair também a consulta e a constante "Primeira Fase" duplicadas em `JogosService` e `SimuladorService`
Além da cascata de desempate (Decisão 2), `JogosService.FaseGrupos` e `SimuladorService.FaseGrupos` já duplicam a mesma constante `"Primeira Fase"` e uma consulta quase idêntica (jogos da fase de grupos com `Grupo`, `SelecaoMandante` e `SelecaoVisitante` carregados). Mover essa constante e essa consulta para o mesmo local compartilhado da Decisão 2 (ex.: `Services/Copa/`), consumido por `JogosService`, `SimuladorService` e `GruposService`.
Alternativa considerada: deixar `GruposService` com sua própria cópia da constante/consulta, isolada das outras duas. Rejeitada pelo mesmo motivo da Decisão 2 — criaria uma terceira cópia da mesma lógica, contrariando a diretriz do projeto de evitar duplicação de código e o pedido explícito de reaproveitamento feito nesta change.

## Risks / Trade-offs

- [Migração de schema em banco já existente] → `AppDbContext`/EF Core já garante criação/atualização automática do schema (data-persistence spec); colunas novas são nullable, então jogos existentes continuam válidos sem backfill.
- [Extração de lógica hoje só usada pelo Simulador pode introduzir regressão no cálculo já validado] → cobrir a extração com os mesmos cenários de desempate já descritos em `specs/simulador/spec.md` aplicados também à nova `specs/grupos/spec.md`, validando que o comportamento do Simulador não muda após a extração.
- [Divergência de escopo entre "resultado oficial" e "resultado simulado" na mesma tela de edição] → nomear claramente os DTOs/serviços (`GruposService`/oficial vs `SimuladorService`/simulado) e não compartilhar a tabela de persistência entre os dois.

## Migration Plan

1. Adicionar `PlacarMandante`/`PlacarVisitante` (`int?`) em `Jogo` e gerar a migração EF Core (`dotnet ef migrations add AddPlacarOficialJogo`).
2. Extrair a cascata de desempate/estatísticas de `SimuladorService`, e a constante/consulta de jogos da fase de grupos de `JogosService`/`SimuladorService`, para o componente compartilhado; ajustar `JogosService` e `SimuladorService` para consumi-lo sem alterar seu comportamento externo.
3. Implementar `GruposService`/DTOs consumindo o componente compartilhado a partir do placar oficial em `Jogo`.
4. Implementar a página de Grupos e subcomponentes, ligar ao `NavMenu`.
5. Validar manualmente: registrar um resultado oficial, confirmar que a classificação do grupo atualiza e que o Simulador permanece inalterado para o mesmo jogo.

Rollback: reverter a migração (`dotnet ef database update <migração anterior>`) e o commit da feature; não há dado de produção em risco (aplicação ainda não publicada).

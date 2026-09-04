## 1. Domínio e Persistência

- [x] 1.1 Adicionar `PlacarMandante` e `PlacarVisitante` (`int?`) em `Models/Copa/Jogo.cs`
- [x] 1.2 Gerar a migração EF Core (`AddPlacarOficialJogo`) e confirmar que ela aplica sem perda de dados sobre o banco existente
- [x] 1.3 Confirmar que `AppDbContext` expõe as novas colunas sem exigir configuração adicional (mapeamento padrão por convenção)

## 2. Cálculo de Classificação Compartilhado

- [x] 2.1 Extrair `EstatisticasTime`, `ConstruirEstatisticasBase`, `OrdenarComDesempate`, `DesempatarBloco` e `DesempatarPorRankingFifa` de `SimuladorService` para um componente compartilhado (ex.: `Services/Copa/ClassificacaoCalculator.cs`), operando sobre uma abstração comum de jogo com placar nullable (grupo, seleções, placar mandante/visitante)
- [x] 2.2 Ajustar `SimuladorService` para consumir o componente compartilhado sem alterar seu comportamento externo (mesmos DTOs, mesma API pública)
- [x] 2.3 Validar manualmente que a página `/simulador` continua se comportando exatamente como antes da extração (classificação, wildcards de terceiros, edição de placar simulado)
- [x] 2.4 Extrair a constante `"Primeira Fase"` e a consulta de jogos da fase de grupos (com `Grupo`, `SelecaoMandante`, `SelecaoVisitante` carregados) hoje duplicadas em `JogosService` e `SimuladorService` para o mesmo local compartilhado da tarefa 2.1; ajustar `JogosService` e `SimuladorService` para consumi-la sem alterar seu comportamento externo

## 3. GruposService

- [x] 3.1 Criar `Services/Grupos/Dtos/` reaproveitando (ou apontando para) `ClassificacaoGrupoDto`/`ClassificacaoTimeDto` já existentes, e um DTO de jogo do grupo com placar oficial nullable incluindo também `Data` e `Horario` — campos que `SimuladorJogoDto` não tem, então este DTO não é um reaproveitamento direto dele (ver design.md - Decisão 3)
- [x] 3.2 Criar `IGruposService`/`GruposService`: `ObterEstadoAtualAsync()` retornando os 12 grupos com jogos e classificação oficial (via componente compartilhado da seção 2), e `AtualizarPlacarOficialAsync(jogoId, placarMandante, placarVisitante)` persistindo em `Jogo` e retornando o estado recalculado
- [x] 3.3 Registrar `IGruposService`/`GruposService` na injeção de dependência em `Program.cs`

## 4. Página de Grupos

- [x] 4.1 Criar `Components/Pages/Grupos/Grupos.razor` (rota `/grupos`) com abas Bootstrap para os 12 grupos, seguindo a estrutura de `Simulador.razor`
- [x] 4.2 Reaproveitar ou adaptar `ClassificacaoGrupo.razor` para exibir Posição, Seleção, J, V, E, D, GP, GC, SG, Pts, com badges de classificado (1º/2º) e possível melhor terceiro (3º)
- [x] 4.3 Exibir a legenda de classificação (classificado x possível vaga de melhor terceiro) na página, fiel ao protótipo `grupos.html`
- [x] 4.4 Criar/adaptar componente de jogo do grupo com edição inline do placar oficial (baseado em `SimuladorJogo.razor`, emitindo `(JogoId, PlacarMandante, PlacarVisitante)` contra `GruposService`)
- [x] 4.5 Adicionar botão "Simular" no grupo selecionado, navegando para `/simulador`
- [x] 4.6 Ao salvar um placar, recarregar o estado via `GruposService` e atualizar a interface imediatamente (sem reload de página)
- [x] 4.7 Criar `wwwroot/css/grupos.css` (seguindo o padrão de `jogos.css`/`simulador.css`) e registrar o `<link>` correspondente em `Components/App.razor`

## 5. Navegação

- [x] 5.1 Adicionar o link "Grupos" (`/grupos`) em `Components/Layout/NavMenu.razor` — ajuste mínimo pontual; o redesenho completo da navegação global do site (fiel ao protótipo, com Jogos/Simulador também linkados) não está no escopo desta change (ver design.md - Non-Goals)

## 6. Validação Manual

- [x] 6.1 Rodar a aplicação, registrar um resultado oficial em um jogo da fase de grupos e confirmar que a classificação do grupo é recalculada e refletida na tela
- [x] 6.2 Confirmar que o mesmo jogo, quando simulado com placar diferente em `/simulador`, não altera a classificação oficial exibida em `/grupos`
- [x] 6.3 Reiniciar a aplicação e confirmar que o resultado oficial registrado permanece persistido e refletido na classificação
- [x] 6.4 Conferir fidelidade visual da página de Grupos em relação ao protótipo `grupos.html` (abas, legenda, tabela, lista de jogos, botão Simular)

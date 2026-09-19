## 1. Resolução de Vencedor/Perdedor Compartilhada

- [x] 1.1 Extrair `CarregarGrafoEliminatorioAsync`, `ResolverSelecao` e `RotularOrigem` de `FaseEliminatoriaService` para uma nova classe `internal static` `Services/Copa/GrafoEliminatorioReader.cs`, sem alterar assinatura nem comportamento; ajustar `FaseEliminatoriaService` para delegar a ela. Rodar as páginas Fase2/Oitavas/Quartas/Semifinais localmente após a extração para confirmar que o comportamento não mudou.
- [x] 1.2 Adicionar `JogoEliminatorioResolver.ObterPerdedorId(Jogo jogo)`, espelhando `ObterVencedorId` (mesma regra de decisão por placar/pênaltis), sem alterar o método existente.
- [x] 1.3 Adicionar `FaseSemifinais` à tabela `RotuloCurtoPorFase` em `JogosFaseEliminatoriaQuery` (rótulo curto `"Semifinal"`), sem alterar as entradas existentes.

## 2. Seed dos Jogos Oficiais

- [x] 2.1 Conferir em `./fontes/copa2026_jogo_terceiro_lugar.txt` e `./fontes/copa2026_jogo_final.txt` a data, horário e cidade oficiais do Terceiro Lugar e da Final; cruzar cada cidade com `./fontes/copa2026_cidades_sede_estadios.txt` para obter o nome do estádio (Miami → Hard Rock Stadium; Nova York/Nova Jersey → MetLife Stadium) — os dois primeiros arquivos não contêm o nome do estádio.
- [x] 2.2 Criar `Data/SeedJson/matches_terceiro_lugar.json` e `Data/SeedJson/matches_final.json`, cada um com um único registro no shape de `MataMataMatchSeedDto` (`order: 1`, `date`, `time`, `city`, `stadium`, `homeSourceOrder: 1`, `awaySourceOrder: 2`), incluídos como recurso embutido (mesmo mecanismo de `matches_semifinais.json`).
- [x] 2.3 Adicionar `SeedTerceiroLugarFinalAsync` em `SeedData.cs`, chamada a partir de `SeedAsync` após `SeedQuartasSemifinaisAsync`. Primeiro remove os jogos legados inertes de `Fase IN ("Terceiro Lugar", "Final")` vindos de `matches.json` (sem `Ordem` nem jogo de origem — descoberto ao inspecionar `portalcopa26.db`, ver design.md Decisão 5), depois insere os jogos reais de forma idempotente por `Fase` (cada uma checada independentemente), lendo os dois JSONs de 2.2 e ligando cada jogo à Semifinal de `homeSourceOrder`/`awaySourceOrder` (1 e 2) já persistida (`JogoOrigemMandante`/`JogoOrigemVisitante`) — sem migração de banco.
- [x] 2.4 Rodar a aplicação localmente e confirmar, via inspeção do banco (`portalcopa26.db`), que os dois jogos foram criados uma única vez e que reiniciar a aplicação não duplica nem recria os jogos.

## 3. Serviço da Página Final

- [x] 3.1 Criar `Services/FinalCopa/Dtos` com o DTO que agrega o Terceiro Lugar, a Final e a seleção campeã (quando houver), reaproveitando `FaseEliminatoriaJogoDto` para cada um dos dois jogos.
- [x] 3.2 Criar `IFinalCopaService`/`FinalCopaService`, usando `GrafoEliminatorioReader` (1.1) para carregar o grafo e resolver a Final por vencedor (`ResolverSelecao`/`RotularOrigem`, prefixo "Vencedor"); para o Terceiro Lugar, resolver mandante/visitante da Semifinal de origem via `ResolverSelecao`, calcular o perdedor dela com `ObterPerdedorId` (1.2), com rótulo pendente montado como `$"Perdedor Semifinal {Ordem}"`.
- [x] 3.3 Expor `AtualizarPlacarOficialAsync` no novo serviço (mesma assinatura/validações de `FaseEliminatoriaService.AtualizarPlacarOficialAsync`: placar de pênaltis empatado rejeitado, seleções pendentes bloqueiam o registro).
- [x] 3.4 Expor a seleção campeã, quando a Final tiver vencedor definido, como `CampeaoDto` (Nome/Codigo/TitulosMundiais, via `TitulosMundiaisCopa`) em vez da entidade `Selecao` crua — o painel de destaque recebe o dado já pronto; `null` quando a Final não tiver vencedor definido.
- [x] 3.5 Registrar `IFinalCopaService` em `Program.cs` (`AddScoped`).

## 4. Títulos Mundiais e Fotos dos Estádios

- [x] 4.1 Criar uma tabela estática em código (ex.: `Data/TitulosMundiaisCopa.cs`) com a quantidade de títulos mundiais conquistados até a Copa de 2022 por seleção (`Codigo` → quantidade), cobrindo as 48 seleções do torneio (qualquer uma pode chegar à Final, já que o simulador não valida força das seleções); documentar que é dado histórico público, mantido fora de `./fontes`, sem coluna nova no banco, e que nunca é incrementado pelo resultado simulado da Final (design.md — Decisão 7).
- [x] 4.2 Definir, em código, as URLs fixas das fotos dos estádios do Terceiro Lugar e da Final (Wikimedia Commons, verificadas no design.md — Decisão 6), associadas por `Fase` ou por nome de estádio.

## 5. Página e Componentes

- [x] 5.1 Adicionar o parâmetro opcional `TituloJogo` (string?, padrão `null`) a `FaseEliminatoriaJogoCard.razor`, substituindo o texto `"Jogo @Ordem"` do cabeçalho quando informado; confirmar que nenhuma página existente (Fase2/Oitavas/Quartas/Semifinais) passa esse parâmetro, preservando o comportamento atual delas.
- [x] 5.2 Criar `Components/Pages/Final.razor` (`@page "/final"`), injetando `IFinalCopaService`, exibindo o card do Terceiro Lugar (`TituloJogo="Disputa de Terceiro Lugar"`) e o card da Final (`TituloJogo="Final"`) com `FaseEliminatoriaJogoCard` (mesmo padrão de `Semifinais.razor`: `OnInitializedAsync` carrega os dados, `PlacarAlterado` chama `AtualizarPlacarOficialAsync` e recarrega).
- [x] 5.3 Exibir, junto a cada card, a foto do respectivo estádio (Decisão 6: `<img>` com `loading="lazy"` e `onerror` ocultando a imagem em caso de falha).
- [x] 5.4 Criar o componente de destaque do campeão (ex.: `Components/Pages/FaseEliminatoria/CampeaoDestaque.razor`), recebendo a seleção campeã como parâmetro, exibindo fundo dourado, ícone de taça, bandeira ampliada, nome em fonte maior e quantidade de títulos mundiais; renderizado condicionalmente pela página Final apenas quando houver campeão.
- [x] 5.5 Adicionar animação de confete opcional ao painel do campeão (CSS/JS leve, apenas quando o painel é exibido).
- [x] 5.6 Adicionar estilos necessários (fundo dourado, tamanhos de fonte/bandeira, enquadramento das fotos de estádio) em `wwwroot/css/landing-page.css`, reaproveitando classes existentes (`lp-section`, `lp-match-card`) sempre que possível.

## 6. Navegação

- [x] 6.1 Adicionar o item de menu "Final" (`href="final"`) em `Components/Layout/NavMenu.razor`, seguindo o mesmo padrão dos itens existentes (Oitavas, Quartas, Semifinais).

## 7. Validação

- [x] 7.1 Build da solução sem erros (`dotnet build`).
- [x] 7.2 Testes existentes continuam passando (`dotnet test`, se houver suíte de testes no projeto) — não há projeto de testes na solução (`PortalCopa2026.slnx` só referencia `PortalCopa2026.csproj`), nada a rodar.
- [x] 7.3 Validar manualmente, navegando pelo menu Final: placeholders iniciais ("Perdedor/Vencedor Semifinal 1/2"), atualização ao registrar cada Semifinal, exibição de nomes e bandeiras quando ambos os lados forem conhecidos, bloqueio de placar enquanto pendente. Validado no navegador via `claude-in-chrome`, decidindo o bracket completo (Segunda Fase → Oitavas → Quartas → Semifinais) diretamente no banco para exercitar o serviço real.
- [x] 7.4 Validar manualmente os casos de borda: apenas um vencedor conhecido, apenas um perdedor conhecido, nenhum conhecido, resultado decidido nos pênaltis, alteração de placar recalculando o chaveamento, remoção de resultado revertendo para placeholder. Todos confirmados (ex.: "Portugal x Perdedor Semifinal 2" / "Alemanha x Vencedor Semifinal 2" com Semifinal 2 pendente; Terceiro Lugar decidido nos pênaltis 4×3).
- [x] 7.5 Validar manualmente a exibição e o recálculo do campeão: painel ausente antes da Final ser decidida, exibido após decidida, atualizado ao alterar o placar da Final, removido ao remover o resultado da Final. Confirmado: Alemanha (4 títulos) → alterado para empate (painel some) → decidido nos pênaltis para Brasil (5 títulos, painel atualiza) → placar removido (painel some, nomes das seleções permanecem).
- [x] 7.6 Confirmar que nenhum jogo é duplicado ou recriado ao reiniciar a aplicação (repetir 2.4 após as demais tarefas). Confirmado antes dos testes manuais (mesmos ids 105/106 após reinício) e o banco foi limpo de volta ao estado sem resultados ao final da validação.
- [x] 7.7 Confirmar visualmente que as fotos dos estádios do Terceiro Lugar e da Final carregam corretamente. Confirmado (Hard Rock Stadium e MetLife Stadium renderizados).

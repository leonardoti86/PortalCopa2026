## Why

O chaveamento eliminatório hoje termina nas Semifinais: não existe página para a Disputa do Terceiro Lugar nem para a Final, e por consequência o Campeão da Copa do Mundo FIFA 2026 nunca é exibido. É preciso fechar o torneio com uma página única que mostre os dois últimos jogos e destaque o campeão assim que a Final for decidida.

## What Changes

- Criar a página única **Final** (`/final`), reutilizando `FaseEliminatoriaJogoCard`/`FaseEliminatoriaService`, contendo:
  - O jogo da Disputa do Terceiro Lugar, alimentado pelos **perdedores** das Semifinais 1 e 2.
  - O jogo da Final, alimentado pelos **vencedores** das Semifinais 1 e 2.
  - Um painel de destaque do Campeão da Copa do Mundo (fundo dourado, taça, bandeira ampliada, nome em fonte maior, quantidade de títulos mundiais, confete opcional), exibido somente após a Final ter vencedor definido.
- Ambos os jogos SHALL ser exibidos desde o início com placeholders (`Perd./Venc. Semifinal 1/2`) e SHALL ter mandante/visitante substituídos automaticamente conforme os resultados das Semifinais forem registrados, alterados ou removidos — sem recriar nem duplicar jogos.
- Adicionar `JogoEliminatorioResolver.ObterPerdedorId`, espelhando `ObterVencedorId` já existente, para resolver o perdedor de um jogo eliminatório decidido (necessário apenas para alimentar o Terceiro Lugar; a lógica de decisão de vencedor das Semifinais não é alterada).
- Semear (via `SeedData`, mesmo padrão idempotente por fase já usado para Quartas/Semifinais) os dois jogos oficiais "Terceiro Lugar" e "Final" a partir de `./fontes/copa2026_jogo_terceiro_lugar.txt` e `./fontes/copa2026_jogo_final.txt`, ligados por `JogoOrigemMandante`/`JogoOrigemVisitante` às Semifinais 1 e 2 — sem criar entidades nem migrações novas (o modelo `Jogo` já suporta o grafo de origem).
- Adicionar a opção de menu **Final** ao menu principal (`NavMenu.razor`), apontando para `/final`.
- Exibir a foto do estádio de cada um dos dois jogos (Miami, para o Terceiro Lugar; Nova York/Nova Jersey, para a Final), reaproveitando o campo `Estadio` já exibido pelo card existente.
- Introduzir uma tabela estática (em código, não em banco de dados) de quantidade de títulos mundiais por seleção, usada exclusivamente pelo painel do campeão — dado histórico público, não fictício e não relacionado aos confrontos do torneio, portanto fora do escopo de "usar exclusivamente os dados de `./fontes`" (que trata dos dados oficiais do torneio).

## Capabilities

### New Capabilities
- `final-copa`: Página Final (Terceiro Lugar + Final + destaque do Campeão), propagação automática de vencedores e perdedores das Semifinais, e navegação pelo menu principal.

### Modified Capabilities
(nenhuma — a extensão do resolver de vencedor/perdedor é um detalhe de implementação aditivo, sem alterar nenhum requisito já publicado em `fase-eliminatoria`)

## Impact

- **Novo**: `Components/Pages/Final.razor` (ou equivalente), consumindo `IFaseEliminatoriaService` (ou uma extensão dele) para obter/atualizar os dois jogos.
- **Novo**: componente de destaque do campeão (ex.: `Components/Pages/FaseEliminatoria/CampeaoDestaque.razor`).
- **Alterado**: `Services/Copa/JogoEliminatorioResolver.cs` (novo método `ObterPerdedorId`), `Services/FaseEliminatoria/*` (nova operação de leitura para os dois jogos), `Data/SeedData.cs` (nova etapa idempotente de seed), `Components/Layout/NavMenu.razor` (novo item "Final").
- **Sem impacto em banco de dados**: nenhuma migração, nenhuma nova entidade ou coluna; reaproveita o grafo `Jogo`/`JogoOrigemMandante`/`JogoOrigemVisitante` já existente.
- **Sem impacto** na lógica das Semifinais nem nas demais fases eliminatórias já implementadas.

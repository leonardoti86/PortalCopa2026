## 1. Constantes e dados oficiais das novas fases

- [x] 1.1 Adicionar em `Services/Copa/JogosFaseEliminatoriaQuery.cs` as constantes `FaseQuartas = "Quartas de Final"` e `FaseSemifinais = "Semifinais"`, e um mapa `fase -> rótulo curto` (`"Segunda Fase"` → `"Segunda Fase"`, `"Oitavas de Final"` → `"Oitavas"`, `"Quartas de Final"` → `"Quartas"`) usado para montar o texto do placeholder (design.md — Decisões 1 e 4)
- [x] 1.2 Criar `Data/SeedJson/matches_quartas.json` com os 4 jogos da tabela de Quartas do design.md (Decisão 1), no mesmo contrato de `matches_oitavas.json` (`order`, `date`, `time`, `city`, `stadium`, `homeSourceOrder`, `awaySourceOrder`), com os dados de `./fontes/copa2026_jogos_quartas.txt`
- [x] 1.3 Criar `Data/SeedJson/matches_semifinais.json` com os 2 jogos da tabela de Semifinais do design.md, com os dados de `./fontes/copa2026_jogos_semifinal.txt` (`homeSourceOrder`/`awaySourceOrder` apontando para a `order` das Quartas)
- [x] 1.4 Renomear `OitavasMatchSeedDto` para `MataMataMatchSeedDto` em `Data/SeedDtos.cs` (renomeação pura, mesmo contrato JSON) e atualizar o uso em `SeedData.SeedJogosOitavas`
- [x] 1.5 Remover de `Data/SeedJson/matches.json` as 4 entradas `qf-*` e as 2 `sf-*`, mantendo `final-1` e `third-1` intactas

## 2. Seed idempotente das Quartas e Semifinais

- [x] 2.1 Extrair o corpo atual de `SeedData.SeedAsync` (com o guard `if (Selecoes.Any()) return;`) para `SeedBaseAsync`, deixando `SeedAsync` como orquestrador que chama `SeedBaseAsync` e depois `SeedQuartasSemifinaisAsync` (design.md — Decisão 5)
- [x] 2.2 Implementar `SeedQuartasSemifinaisAsync`, passo 1: remover os jogos legados inertes — `Fase` em (`"Quartas"`, `"Semifinal"`), `Ordem == null` e sem jogo de origem
- [x] 2.3 Implementar o passo 2: se não houver nenhum jogo com `Fase == FaseQuartas`, criar os 4 jogos a partir de `matches_quartas.json`, ligando `JogoOrigemMandante`/`JogoOrigemVisitante` aos jogos das Oitavas buscados por `Fase` + `Ordem`, sem atribuir seleção alguma; salvar
- [x] 2.4 Implementar o passo 3: se não houver nenhum jogo com `Fase == FaseSemifinais`, criar os 2 jogos a partir de `matches_semifinais.json`, ligando aos jogos das Quartas por `Fase` + `Ordem`; salvar
- [x] 2.5 Verificar a idempotência executando a aplicação duas vezes seguidas sobre o mesmo `portalcopa26.db`: na segunda execução nenhum jogo é criado, removido ou alterado

## 3. Resolução do chaveamento em profundidade arbitrária

- [x] 3.1 Substituir `FaseEliminatoriaService.CarregarComOrigemAsync` por uma carga única do grafo eliminatório (`Where(j => j.Ordem != null)` + `Include` de `SelecaoMandante`/`SelecaoVisitante`), confiando no relationship fixup do EF Core para preencher `JogoOrigemMandante`/`JogoOrigemVisitante` (design.md — Decisão 3)
- [x] 3.2 Tornar `FaseEliminatoriaService.ResolverSelecao` recursiva conforme design.md — Decisão 2, mantendo `JogoEliminatorioResolver` inalterado
- [x] 3.3 Passar `ObterSegundaFaseAsync`, `ObterOitavasAsync` e `AtualizarPlacarOficialAsync` a usar a carga única, filtrando por `Fase` (ou `Id`) em memória, sem alterar o comportamento observável de Fase2 e Oitavas
- [x] 3.4 Adicionar `ObterQuartasAsync` e `ObterSemifinaisAsync` a `IFaseEliminatoriaService` e implementá-los em `FaseEliminatoriaService`, ordenando por `Ordem`

## 4. Rótulo genérico do placeholder

- [x] 4.1 Trocar em `FaseEliminatoriaJogoDto` os campos `OrigemMandanteOrdem`/`OrigemVisitanteOrdem` por `OrigemMandanteRotulo`/`OrigemVisitanteRotulo` (`string?`)
- [x] 4.2 Montar o rótulo no serviço a partir da fase e da `Ordem` do jogo de origem (`"Vencedor Oitavas 2"`, `"Vencedor Quartas 1"`), usando o mapa da tarefa 1.1
- [x] 4.3 Ajustar `Components/Pages/FaseEliminatoria/FaseEliminatoriaJogoCard.razor` para exibir o rótulo pronto no lugar do literal `"Vencedor Segunda Fase {Ordem}"`, conferindo que o texto exibido nas Oitavas continua exatamente `"Vencedor Segunda Fase N"`

## 5. Páginas Quartas e Semifinais

- [x] 5.1 Criar `Components/Pages/Quartas.razor` (`/quartas`) espelhando `Oitavas.razor`: `@rendermode InteractiveServer`, `IFaseEliminatoriaService`, grade de `FaseEliminatoriaJogoCard` e `OnPlacarAlteradoAsync` que persiste e recarrega a lista da fase (design.md — Decisão 6)
- [x] 5.2 Criar `Components/Pages/Semifinais.razor` (`/semifinais`) no mesmo padrão
- [x] 5.3 Adicionar no cabeçalho das duas páginas os links de navegação entre fases (`Oitavas` ↔ `Quartas` ↔ `Semifinais`), no padrão do botão "Ver Segunda Fase" de `Oitavas.razor`, sem CSS novo
- [x] 5.4 Adicionar as opções "Quartas" (`quartas`) e "Semifinais" (`semifinais`) a `Components/Layout/NavMenu.razor`, depois de "Oitavas"

## 6. Validação

- [x] 6.1 `dotnet build` do projeto sem erros nem avisos novos
- [x] 6.2 Com o banco zerado e com o banco já existente: conferir 4 jogos em `"Quartas de Final"` (Ordem 1–4), 2 em `"Semifinais"` (Ordem 1–2), nenhum em `"Quartas"`/`"Semifinal"` e total de 104 jogos, sem duplicidade
- [x] 6.3 Navegar por `/quartas` e `/semifinais` sem nenhum resultado registrado e conferir os placeholders (`Vencedor Oitavas 1 x Vencedor Oitavas 2`, `Vencedor Quartas 1 x Vencedor Quartas 2`) e os campos de placar desabilitados
- [x] 6.4 Registrar o placar de um jogo das Oitavas e conferir que o lado correspondente das Quartas passa a exibir nome e bandeira da seleção vencedora (caso de borda: apenas um vencedor conhecido continua exibindo o placeholder do outro lado)
- [x] 6.5 Registrar os dois vencedores de um confronto das Quartas, decidir um deles nos pênaltis, e conferir a propagação até as Semifinais
- [x] 6.6 Alterar o placar de um jogo das Oitavas invertendo o vencedor e conferir o recálculo automático em Quartas e Semifinais
- [x] 6.7 Remover o placar de um jogo das Oitavas e conferir que Quartas e Semifinais voltam ao placeholder e ao placar bloqueado
- [x] 6.8 Definir o vencedor de um jogo da Segunda Fase (sem editar Oitavas nem Quartas manualmente) e conferir que a propagação em cascata de 4 níveis chega até a página Semifinais, validando a resolução recursiva da Decisão 2/3 do design em toda a profundidade da árvore
- [x] 6.9 Conferir que Fase2 e Oitavas continuam com o mesmo comportamento e o mesmo texto de placeholder de antes da change
- [x] 6.10 `openspec validate criar-jogos-quartas-semifinais --strict`

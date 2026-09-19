## Why

O PortalCopa26 já cobre a Fase de Grupos, a Segunda Fase e as Oitavas de Final, com propagação automática dos vencedores da Segunda Fase para as Oitavas. As duas fases seguintes do chaveamento oficial — Quartas de Final (4 jogos) e Semifinais (2 jogos) — não existem como páginas nem como jogos reais no banco: as 4 + 2 entradas hoje presentes em `matches.json` são semeadas sem seleções, sem `Ordem` e sem referência ao jogo de origem, ficando inertes e invisíveis. Sem elas o usuário não consegue seguir o mata-mata além das Oitavas, e o vencedor de um jogo das Oitavas não alimenta confronto nenhum.

## What Changes

- Criar as páginas `Quartas` (`/quartas`) e `Semifinais` (`/semifinais`), reaproveitando `FaseEliminatoriaJogoCard` e o mesmo padrão visual/estrutural de `Fase2.razor` e `Oitavas.razor` (classes `lp-match-card` / `sim-jogo`, seguindo o card de jogo do protótipo).
- Semear os 4 jogos das Quartas e os 2 das Semifinais a partir de `./fontes/copa2026_jogos_quartas.txt` e `./fontes/copa2026_jogos_semifinal.txt`, sempre por referência ao jogo de origem (`JogoOrigemMandante`/`JogoOrigemVisitante`), nunca com seleção fixa — Quartas N aponta para Oitavas 2N-1 e 2N; Semifinal N aponta para Quartas 2N-1 e 2N.
- Tornar o seed dessas duas fases **idempotente e independente do guard global** (`if (Selecoes.Any()) return;`), para que um banco já existente receba os novos jogos sem recriar nada e sem duplicar: os jogos são criados apenas quando a fase ainda não tem jogos com `Ordem` preenchida.
- Remover de `matches.json` as 4 entradas `qf-*` e as 2 `sf-*` (hoje inertes), que passam a ser carregadas pelos novos métodos de seed — mesmo tratamento já dado às entradas `r32-*`/`r16-*` na change anterior —, e limpar de bancos já existentes as linhas inertes correspondentes. Sem isso haveria jogo duplicado nas duas fases.
- Generalizar a resolução de seleção do `FaseEliminatoriaService` para percorrer o encadeamento de jogos de origem em **qualquer profundidade** (Semifinal → Quartas → Oitavas → Segunda Fase). Hoje ela só resolve um nível, o que bastava para as Oitavas mas devolveria sempre "pendente" nas Quartas e Semifinais. A regra de decisão do vencedor (`JogoEliminatorioResolver`) não muda.
- Generalizar o rótulo do placeholder exibido no card: hoje é o literal `"Vencedor Segunda Fase {Ordem}"`; passa a ser derivado da fase do jogo de origem (`Venc. Oitavas 2`, `Venc. Quartas 1`), mantendo inalterado o texto já exibido nas Oitavas.
- Adicionar "Quartas" e "Semifinais" ao menu principal (a opção "Oitavas" já existe).

### Premissa registrada (prorrogação)

O enunciado pede placar de tempo regulamentar, prorrogação e pênaltis, mas a change proíbe alterar o banco e criar entidades — e `Jogo` só tem `PlacarMandante`/`PlacarVisitante` e `PlacarPenaltisMandante`/`PlacarPenaltisVisitante`. Esta change mantém a decisão já tomada (e documentada como Non-Goal) na change das Oitavas: **o placar oficial é o resultado final, já incluída a prorrogação**, e só os pênaltis têm campos próprios, por serem a única situação em que o placar não decide o jogo. Registrar a prorrogação separadamente exigiria novas colunas e uma migration, o que está explicitamente fora do escopo. Se o placar separado de prorrogação for mesmo necessário, ele deve virar uma change própria.

## Capabilities

### New Capabilities
<!-- Nenhuma capacidade nova: Quartas e Semifinais são a continuação natural da capacidade fase-eliminatoria já existente. -->

### Modified Capabilities
- `fase-eliminatoria`: passa a cobrir também as Quartas de Final e as Semifinais — listagem com placeholder desde o início, registro de placar (incluindo pênaltis) quando as duas seleções forem conhecidas, propagação em cascata dos vencedores entre Oitavas → Quartas → Semifinais, reversão para placeholder quando um resultado deixa de definir vencedor, e navegação pelo menu principal.
- `seed-data`: a carga inicial passa a incluir os 4 jogos das Quartas e os 2 das Semifinais, a partir dos arquivos oficiais em `./fontes`, de forma idempotente (sem recriar nem duplicar jogos em bancos já existentes).

## Impact

- Dados: `Data/SeedData.cs` (novo passo idempotente de seed das fases finais; `SeedJogosOitavas` passa a devolver o dicionário `ordem -> Jogo`), `Data/SeedDtos.cs` (reuso do DTO de jogo por jogo-de-origem), novos `Data/SeedJson/matches_quartas.json` e `Data/SeedJson/matches_semifinais.json`, remoção das entradas `qf-*`/`sf-*` de `Data/SeedJson/matches.json`.
- Serviços: `Services/Copa/JogosFaseEliminatoriaQuery.cs` (constantes `"Quartas de Final"` e `"Semifinais"`), `Services/FaseEliminatoria/IFaseEliminatoriaService.cs` + `FaseEliminatoriaService.cs` (`ObterQuartasAsync`, `ObterSemifinaisAsync`, resolução recursiva de seleção e carga do grafo eliminatório), `Services/FaseEliminatoria/Dtos/FaseEliminatoriaJogoDto.cs` (rótulo do jogo de origem no lugar da ordem crua).
- Interface: novas `Components/Pages/Quartas.razor` e `Components/Pages/Semifinais.razor`, `Components/Pages/FaseEliminatoria/FaseEliminatoriaJogoCard.razor` (rótulo de placeholder genérico), `Components/Layout/NavMenu.razor`.
- Sem alteração de schema: nenhuma entidade nova, nenhuma coluna nova, nenhuma migration. `Models/Copa/Jogo.cs` e `Data/AppDbContext.cs` ficam intactos.
- Fora do escopo: Final, disputa de terceiro lugar, simulador do mata-mata e placar separado de prorrogação.

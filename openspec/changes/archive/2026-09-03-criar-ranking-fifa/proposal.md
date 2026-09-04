## Why

A Landing Page já exibe o Top 10 do Ranking FIFA e traz um link "Ver ranking completo →" apontando para `/ranking`, mas essa página ainda não existe. O PortalCopa26 precisa de uma página dedicada de Ranking FIFA para que visitantes consultem a posição, pontuação e grupo das 48 seleções da Copa 2026 e busquem uma seleção específica.

## What Changes

- Criar a página `Ranking` (`/ranking`), acessível pelo menu principal e pelo link já existente na Landing Page.
- Exibir a lista completa das seleções da Copa 2026 que possuem posição no Ranking FIFA oficial, ordenada por posição, com posição, seleção (bandeira + nome), pontuação e grupo.
- Destacar visualmente as 3 primeiras posições (pódio), com fundo escuro e texto em cor clara para contraste, reaproveitando a paleta já usada na Hero Section da Landing Page.
- Permitir buscar seleções por nome, reaproveitando o componente `FiltroSelecao` já existente (busca por nome + filtro por grupo).
- Tratar de forma explícita as seleções da Copa 2026 que não possuem posição na fonte oficial de Ranking FIFA (7 das 48 seleções), listando-as à parte em vez de omiti-las silenciosamente ou inventar uma posição.
- Criar `IRankingService`/`RankingService` para centralizar a consulta ao ranking, seguindo o padrão de serviços por capacidade já usado no projeto (`GruposService`, `SelecaoService`, etc.).
- **Não** criar novas entidades nem alterar o schema: a entidade `RankingFifa` e o seed a partir de `teams.json` já existem e cobrem exatamente as seleções da Copa 2026 (ver Impact).

## Capabilities

### New Capabilities
- `ranking`: página pública que lista o Ranking FIFA das seleções da Copa 2026, com destaque para o Top 3 e busca por seleção.

### Modified Capabilities
(nenhuma — `domain-model` e `seed-data` já cobrem a entidade `RankingFifa` e sua carga inicial; esta change apenas consome esses dados em uma nova página)

## Impact

- **Novo**: `Services/Ranking/IRankingService.cs`, `Services/Ranking/RankingService.cs`, `Services/Ranking/Dtos/*`, `Components/Pages/Ranking/*.razor`, `wwwroot/css/ranking.css`.
- **Alterado**: `Components/Layout/NavMenu.razor` (novo item "Ranking"), `Program.cs` (registro do `IRankingService`), `Components/App.razor` (referência a `ranking.css`, mesmo padrão de `grupos.css`/`selecoes.css`/`simulador.css`).
- **Dados**: nenhuma alteração de schema ou de seed. A página consome o `DbSet<RankingFifa>` já populado por `SeedData.cs` a partir de `teams.json` (fonte oficial: `fontes/copa2026_ranking_fifa.txt`).
- **Observação (fora do escopo desta change)**: `Data/SeedJson/ranking.json` (lista bruta com as ~98 seleções do ranking mundial) existe no repositório mas não é lida por nenhum código de seed — não será removido nem utilizado aqui para não introduzir uma alteração não relacionada ao objetivo da change.

## Why

O PortalCopa26 ainda não possui uma página dedicada às 48 seleções da Copa do Mundo FIFA 2026. Os dados de seleções, jogadores, técnicos e ranking FIFA já existem no banco (seed via `SeedData`/`Selecao`/`Jogador`), mas não há nenhuma capacidade que os exponha ao usuário. O protótipo de referência (`../prototipo`, página "Equipes") já validou a experiência esperada — busca, filtro por grupo e detalhe do elenco — servindo de base visual e funcional para esta implementação em Blazor.

## What Changes

- Criar a capacidade **Seleções**, acessível pelo menu principal, exibindo as 48 seleções em um grid de cartões (bandeira, nome, grupo, código FIFA).
- Adicionar busca por nome da seleção e filtro por grupo (A a L) na listagem.
- Ao clicar em uma seleção, abrir uma janela modal (Bootstrap 5 `Modal`) com: bandeira, nome, grupo, técnico e ranking FIFA da seleção, além da lista completa de jogadores convocados (nome, posição, idade, gols pela seleção).
- Criar `SelecaoService`/`ISelecaoService` em `Services/Selecoes` para centralizar o acesso a `Selecao`/`Jogador` via `AppDbContext` (nenhuma página acessa o `DbContext` diretamente).
- Criar os componentes em `Components/Pages/Selecoes`: `Selecoes.razor` (página), `CartaoSelecao.razor`, `FiltroSelecao.razor`, `ModalSelecao.razor` e `TabelaElencoSelecao.razor`.
- Adicionar item "Seleções" ao `NavMenu`.

Não há alteração de modelo de dados: `Selecao`, `Jogador`, `RankingFifa` e o relacionamento Selecao → Jogadores já existem em `Models/Copa` e estão mapeados em `AppDbContext`/`SeedData` a partir de `Data/SeedJson/teams.json` e `players.json` (gerados a partir das fontes oficiais `copa2026_selecoes_jogadores.txt` e `copa2026_pais_tecnicos.txt`).

## Capabilities

### New Capabilities
- `selecoes`: Página de Seleções com listagem (busca + filtro por grupo) das 48 seleções e modal de detalhe/elenco por seleção.

### Modified Capabilities
(nenhuma — as demais capacidades não têm requisitos alterados)

## Impact

- **Novo código**: `Components/Pages/Selecoes/*.razor` (`Selecoes.razor` + `CartaoSelecao.razor`, `FiltroSelecao.razor`, `ModalSelecao.razor`, `TabelaElencoSelecao.razor`), `Services/Selecoes/ISelecaoService.cs`, `Services/Selecoes/SelecaoService.cs`, `Services/Selecoes/Dtos/*`, `wwwroot/css/selecoes.css`.
- **Modificado**: `Components/Layout/NavMenu.razor` (novo item de menu), `Program.cs` (registro de `ISelecaoService`), `Components/App.razor` (link do novo CSS).
- **Dados**: nenhuma migração nova; reaproveita `Selecao`, `Jogador`, `RankingFifa`, `Grupo` e o seed já existente.
- **Dependências**: nenhuma nova biblioteca; usa Bootstrap 5 (já presente) para o modal e o grid.

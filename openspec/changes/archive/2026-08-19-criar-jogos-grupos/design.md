## Context

O domínio já expõe `Jogo` (Data, Horario, Fase, Cidade, Estadio, GrupoId/Grupo, SelecaoMandante/Visitante) e `Grupo` (Codigo) via EF Core (`AppDbContext`). Os 72 jogos da fase de grupos foram carregados pelo Seed Data com `Fase == "Primeira Fase"`, `GrupoId` preenchido e ambas as seleções definidas — os demais 30 jogos do torneio (fases eliminatórias) têm `GrupoId` nulo e podem ter seleções nulas (placeholder), como já tratado em `LandingPageService`. A Landing Page já resolve um recorte semelhante (próximos jogos da fase de grupos) através de `ILandingPageService`/`LandingPageService`, servindo de referência de padrão a seguir: página consome apenas uma interface de serviço injetada, serviço consulta `AppDbContext` e projeta para DTOs (`records`), nunca expõe entidades EF para a camada de apresentação. Ver proposal.md - Why.

## Goals / Non-Goals

**Goals:**
- Definir a estrutura de dados (DTOs) e o contrato de serviço (`IJogosService`) que a página de Jogos consome.
- Definir a decomposição dos componentes Razor solicitados (`Jogos`, `JogosFiltro`, `JogosDataHeader`, `JogoCard`) e a responsabilidade de cada um.
- Definir onde ocorre o agrupamento por data e a ordenação (servidor vs. componente).

**Non-Goals:**
- Implementar a página de Grupos (`/grupos`) — o botão "Ver Grupos" apenas navega para a rota; a página em si é uma capability futura.
- Alterar o schema do banco, as Migrations ou o Seed Data.
- Exibir jogos das fases eliminatórias (mata-mata) — escopo restrito à fase de grupos, conforme CLAUDE.md (Escopo > Jogos).

## Decisions

### Decisão 1: Filtragem por grupo é client-side, sobre uma única carga de dados
O serviço (`IJogosService.ObterJogosFaseGruposAsync`) carrega uma única vez todos os jogos da fase de grupos (72 registros — volume pequeno e fixo). O componente `Jogos.razor` mantém a lista completa em memória e aplica o filtro de grupo localmente antes de agrupar/renderizar.
- **Alternativa considerada**: repassar o `GrupoId` selecionado ao serviço e reconsultar o banco a cada mudança de filtro. Rejeitada porque adiciona uma viagem ao banco por clique sem benefício real dado o volume fixo e pequeno de dados (72 jogos), e complica o estado de carregamento da UI sem necessidade.

### Decisão 2: Agrupamento por data acontece no componente de apresentação, ordenação acontece no serviço
`JogosService` retorna a lista já ordenada por Data/Horário crescente (via LINQ/EF, igual ao padrão usado em `LandingPageService.ObterProximosJogosAsync`). `Jogos.razor` agrupa a lista (já ordenada) por `Data` usando `GroupBy` preservando a ordem, e renderiza um `JogosDataHeader` por grupo de data seguido dos `JogoCard` daquele dia.
- **Alternativa considerada**: retornar do serviço uma estrutura já agrupada (`IReadOnlyList<JogosPorDataDto>`). Rejeitada porque acopla o filtro por grupo (que é client-side, Decisão 1) ao agrupamento — filtrar depois de agrupar exigiria remontar os grupos de data no componente de qualquer forma; mais simples manter o serviço retornando uma lista plana e reaproveitável.

### Decisão 3: Componentes e responsabilidades
- **`Jogos.razor`** (`@page "/jogos"`): página. Injeta `IJogosService`, carrega os dados no `OnInitializedAsync`, mantém o estado do filtro selecionado (`string? _grupoSelecionado`), deriva a lista filtrada e agrupada por data, e renderiza `JogosFiltro`, os blocos `JogosDataHeader`/`JogoCard`, e o botão "Ver Grupos".
- **`JogosFiltro.razor`**: componente controlado (dropdown/botões dos códigos de grupo A–L presentes nos dados + opção "Todos os grupos"). Recebe a lista de grupos disponíveis e o grupo selecionado via `[Parameter]`, expõe `EventCallback<string?>` para notificar mudança de seleção. Não acessa dados diretamente.
- **`JogosDataHeader.razor`**: apresentacional puro. Recebe uma `DateOnly` via `[Parameter]` e renderiza o cabeçalho formatado (ex.: dia da semana + data).
- **`JogoCard.razor`**: apresentacional puro. Recebe um `JogoDto` via `[Parameter]` e renderiza horário, seleções (com bandeiras, seguindo o padrão de `ProximosJogos.razor`), grupo e estádio/cidade.

### Decisão 4: Novo DTO `JogoDto` dedicado (não reaproveita `ProximoJogoDto`)
Criar `Services/Jogos/Dtos/JogoDto.cs` como um `record` próprio da capability `jogos`, com os mesmos campos de `ProximoJogoDto` mais o `Id` do jogo (necessário como `@key`/identificador estável nas listas do Blazor, o que `ProximoJogoDto` não expõe).
- **Alternativa considerada**: reutilizar `ProximoJogoDto` de `Services/LandingPage/Dtos`. Rejeitada para não criar uma dependência cruzada entre as capabilities `landing-page` e `jogos` (CLAUDE.md - Organização Funcional: cada capacidade deve possuir componentes e serviços próprios) e porque falta o `Id` do jogo.

## Risks / Trade-offs

- [Carregar todos os 72 jogos de uma vez pode parecer desperdício em uma listagem que também é filtrada] → Mitigação: volume é fixo e pequeno (72 registros da fase de grupos), a consulta é uma única leitura simples via EF Core; filtragem client-side evita round-trips repetidos ao trocar o filtro.
- [Duplicação de DTO entre `landing-page` e `jogos` (`ProximoJogoDto` vs `JogoDto`)] → Mitigação: aceita conscientemente (Decisão 4) para manter o isolamento entre capabilities definido em CLAUDE.md; os dois DTOs podem divergir livremente no futuro sem afetar a outra capability.
- [Botão "Ver Grupos" aponta para uma rota (`/grupos`) que ainda não existe nesta change] → Mitigação: navegação via `NavLink`/`href="/grupos"` não quebra a página de Jogos; ao acessar antes da página de Grupos existir, o Blazor exibirá a página `NotFound` já presente no projeto.

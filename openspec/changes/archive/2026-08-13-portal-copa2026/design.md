## Context

Este change cria a base de código do PortalCopa26 do zero (não há projeto .NET existente ainda). As decisões arquiteturais globais (Blazor Web App, .NET 10, EF Core, SQLite, projeto único com organização preparada para camadas futuras) já estão fixadas em `CLAUDE.md` e não são reabertas aqui. O protótipo HTML validado em `copa2026/prototipo/data/*.js` é a fonte de referência para o formato dos dados (seleções, grupos, jogos, jogadores, ranking FIFA), mas está em JavaScript (`window.COPA26.x = [...]`), não em um formato diretamente consumível pelo .NET. Ver `proposal.md` - Why para a motivação completa.

## Goals / Non-Goals

**Goals:**
- Definir a estrutura de pastas e projeto que sustente as páginas e serviços futuros sem retrabalho estrutural.
- Definir o modelo de dados (entidades + relacionamentos) e o mecanismo de persistência (EF Core + SQLite) usados por todas as funcionalidades futuras.
- Definir como o SeedData é gerado a partir dos dados do protótipo e carregado de forma idempotente.
- Definir a estratégia de evolução de schema (migrations) já que o modelo crescerá quando LandingPage, Ranking, Simulador etc. forem implementados.

**Non-Goals:**
- Definir queries, endpoints ou componentes Razor específicos de cada página futura (landing page, jogos, grupos, seleções, ranking, simulador) - isso pertence às specs dessas funcionalidades.
- Definir a lógica de cálculo de classificação do simulador (regras de pontuação, critérios de desempate) - fora do escopo deste change.
- Definir a arquitetura em camadas final (ex.: projetos separados Domain/Application/Infrastructure) - apenas preparar o terreno para que essa migração seja possível depois.

## Decisions

### 1. Estrutura de pastas dentro do projeto único
`Pages/`, `Components/`, `Models/`, `Services/`, `Data/` na raiz do projeto Blazor, conforme `CLAUDE.md`. Dentro de `Models/`, as entidades ficam agrupadas por área (`Models/Copa/` para Grupo, Selecao, Jogador, Jogo, RankingFifa; `Models/Simulacao/` para Simulacao, SimulacaoJogo), o que facilita uma futura extração para projetos `Domain`/`Application` sem precisar reorganizar namespaces internos, apenas mover pastas.

**Alternativa considerada**: um único namespace `Models` sem subpastas. Rejeitada por dificultar a extração seletiva de entidades quando a migração em camadas ocorrer.

### 2. Estratégia de schema: EF Core Migrations aplicadas no startup
O `AppDbContext` usa **EF Core Migrations** (não `EnsureCreated()`). No `Program.cs`, após montar o `WebApplication`, o sistema executa `dbContext.Database.Migrate()` dentro de um scope, antes de `app.Run()`.

**Por quê**: o modelo de dados vai crescer nas próximas specs (landing page, ranking, simulador). Migrations permitem evoluir o schema preservando dados já persistidos (incluindo simulações do usuário). `EnsureCreated()` não gera migrations e impede alterações incrementais de schema sem apagar o banco.

**Alternativa considerada**: `EnsureCreated()` por ser mais simples para um protótipo. Rejeitada porque conflita com o requisito de persistência duradoura de simulações à medida que o schema evolui.

### 3. Arquivo SQLite e connection string
Banco de dados único em arquivo, ex. `Data Source=portalcopa26.db`, localizado na raiz de execução do projeto (mesmo diretório do `.csproj`/publicação). Sem necessidade de configuração adicional (sem autenticação, sem multi-tenant) dado que a aplicação é local e sem área administrativa.

### 4. Sem entidade dedicada para Estádio nesta fase
`CLAUDE.md` menciona "Informações do estádio" no contexto da página de Jogos, mas o escopo deste change lista explicitamente apenas Grupo, Seleção, Jogador, Jogo, RankingFifa, Simulacao e SimulacaoJogo como entidades iniciais. Cidade, nome do estádio e capacidade ficam como propriedades simples (`Cidade`, `Estadio`) na entidade `Jogo`, em vez de uma entidade `Estadio` separada.

**Por quê**: evita introduzir uma entidade fora do escopo pedido. Se uma página futura precisar listar estádios de forma independente (com capacidade, notas etc.), isso justifica uma entidade `Estadio` própria naquele momento - migração de campo simples para FK é um refactor localizado.

### 5. Classificação de simulação não é persistida como entidade própria
`CLAUDE.md` fala em persistir "classificações geradas a partir das simulações", mas o simulador (que define as regras de pontuação e critérios de desempate) está fora do escopo deste change. Persistir aqui uma tabela `Classificacao` seria adivinhar um schema sem as regras de negócio que o definem.

**Decisão**: este change persiste apenas os resultados simulados (`SimulacaoJogo`, com placar por seleção). A classificação é dado derivado, calculável a partir de `SimulacaoJogo` a qualquer momento. Quando a spec do Simulador for criada, ela decide se a classificação calculada deve também ser cacheada/persistida - isso não exige mudanças nas entidades já criadas aqui, apenas uma adição futura.

### 6. Geração do SeedData a partir do protótipo
Os arquivos `copa2026/prototipo/data/*.js` (teams, groups, players, matches, ranking) são a fonte de verdade dos dados iniciais, mas estão em formato JavaScript. Durante a implementação (tasks.md), esses dados são transcritos para coleções C# estáticas (ou JSON embutido no projeto, lido no startup) usadas por uma classe `SeedData` em `Data/`, chamada uma única vez durante `Database.Migrate()`.

**Alternativa considerada**: ler os arquivos `.js` diretamente em runtime via parsing. Rejeitada por adicionar complexidade e uma dependência de parsing desnecessária para um conjunto de dados estático que não muda em runtime.

### 7. Seed idempotente
A rotina de seed verifica se já existem registros (`if (!context.Selecoes.Any())`) antes de inserir. Toda a carga inicial roda em uma única operação/transação para não deixar dados parcialmente inseridos em caso de falha.

## Risks / Trade-offs

- [Risco] Transcrição manual dos dados do protótipo (JS → C#) pode introduzir divergências ou erros de digitação → Mitigação: revisar a contagem de registros (times, jogos, grupos) contra os arquivos-fonte do protótipo como parte da task de seed.
- [Risco] Ausência de uma entidade `Estadio` pode exigir uma migration de schema quando a página de estádios for implementada → Mitigação: aceito conscientemente (ver Decisão 4); o campo permanece simples até haver requisito real.
- [Risco] `Database.Migrate()` no startup pode falhar silenciosamente se o processo não tiver permissão de escrita no diretório do banco → Mitigação: deixar a falha propagar (fail-fast) no startup, sem catch silencioso, para que o erro apareça imediatamente nos logs.
- [Trade-off] Modelar Ranking FIFA como um registro por Seleção (sem histórico) simplifica o schema desta fase, mas não suporta evolução histórica do ranking → aceito porque não há requisito de histórico no escopo atual.

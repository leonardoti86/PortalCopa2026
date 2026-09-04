## Context

O projeto já tem fundação pronta: `AppDbContext` (EF Core + SQLite) com `Grupos`, `Selecoes`, `Jogadores`, `Jogos`, `RankingsFifa`, `Simulacoes`/`SimulacaoJogos`, e `SeedData` populando o banco a partir de JSON gerado das fontes oficiais. A UI hoje é o `Home.razor` padrão do template Blazor Web App (renderização interativa por servidor, já configurada em `Program.cs`). Bootstrap 5 já está em `wwwroot/lib/bootstrap`; Chart.js ainda não está presente no projeto. O protótipo em `copa2026/prototipo` (`index.html`, `css/styles.css`, `js/pages/home.js`) define a referência visual: hero em gradiente com países-sede e CTA, stats-row, seção "Próximos jogos" (cards), seção "Ranking FIFA" (tabela) e CTA final para o simulador. Bandeiras usam o padrão `https://api.fifa.com/api/v3/picture/flags-sq-4/{CodigoSelecao}` (ver `Selecao.Codigo`, já com 3 letras, `HasMaxLength(3)`).

Ver proposal.md - Why/What Changes para a motivação completa.

## Goals / Non-Goals

**Goals:**
- Compor a Landing Page a partir de 5 componentes Razor independentes e reutilizáveis, sem lógica de acesso a dados dentro deles.
- Centralizar o acesso a dados da Landing Page em um serviço de aplicação único, registrado via DI, consumido pelos componentes via `@inject`.
- Fornecer um componente de gráfico Chart.js genérico (tipo de gráfico, labels e dados como parâmetros) reutilizável por futuras páginas estatísticas, não acoplado ao domínio de ranking FIFA.
- Manter o visual alinhado ao protótipo (paleta de cores, hero, cards, tabela) usando Bootstrap 5 como base de grid/utilitários, com CSS complementar próprio do componente onde o Bootstrap não cobrir o visual do protótipo.

**Non-Goals:**
- Não implementar a lógica do simulador (`SimuladorPainel.razor` é somente um CTA estático/navegação).
- Não criar as páginas Jogos, Grupos, Seleções ou Ranking completo, nem navegação avançada (multi-nível, active-state complexo) — apenas o necessário para os links da Landing Page funcionarem como CTA (podem apontar para rotas ainda não implementadas nesta change).
- Não alterar o schema do banco, `AppDbContext` ou `SeedData`.
- Não implementar o countdown/contagem regressiva até a abertura da Copa presente no protótipo (`js/pages/home.js`, `renderCountdown`). É uma funcionalidade só do protótipo, conscientemente fora desta change (não há requirement correspondente na spec); pode ser proposta como change futura se desejado.

## Decisions

### 1. Um serviço único para a Landing Page, não um serviço por componente
`ILandingPageService` (implementado por `LandingPageService`) expõe os métodos necessários aos 5 componentes: `ObterEstatisticasAsync()`, `ObterProximosJogosAsync(int quantidade)`, `ObterRankingTopAsync(int quantidade)`. Alternativa considerada: um serviço por componente (`IEstatisticasService`, `IProximosJogosService`, etc.). Rejeitada por criar fragmentação desnecessária para consultas de leitura simples sobre as mesmas poucas tabelas; um serviço único de "leitura da home" é mais simples de registrar e testar nesta fase, e pode ser dividido depois se crescer. O serviço vive em `Services/LandingPage/` para manter a organização por área e não obstruir uma futura extração em camada de aplicação.

### 2. DTOs de leitura próprios da Landing Page, não expor entidades EF diretamente
O serviço retorna DTOs simples (`CopaEstatisticasDto`, `ProximoJogoDto`, `RankingItemDto`) em vez de `Selecao`/`Jogo`/`RankingFifa`. Alternativa considerada: os componentes consumirem as entidades diretamente. Rejeitada porque acopla a UI ao modelo de persistência e vaza propriedades de navegação (`ICollection<Jogador>`, referências cíclicas) desnecessárias para a página; DTOs também deixam explícito o contrato que os componentes usam via Razor markup.

### 3. Consulta de "próximos jogos" usa `Fase == "Primeira Fase"` e `Data/Horario >= agora`
Alinhado ao protótipo (`js/pages/home.js`, filtro por `phase === 'Primeira Fase'`) e ao valor de `Jogo.Fase` populado pelo seed (`match.Phase`). Quando não há jogos futuros (dados históricos ou torneio já iniciado sem próximos jogos), o serviço retorna lista vazia e o componente exibe uma mensagem, conforme o requirement "Ausência de próximos jogos".

**Nota de validação:** os jogos de "Primeira Fase" no seed (`matches.json`) têm datas entre 11/06/2026 e 27/06/2026. Rodando a aplicação em qualquer data posterior a 27/06/2026 (inclusive na data atual do ambiente de desenvolvimento), `ObterProximosJogosAsync` sempre retornará lista vazia e a seção sempre exibirá o estado "sem próximos jogos" — esse é o comportamento correto e esperado dado o dado seedado, não um bug. Ver tasks 6.1/6.2 para como validar os dois cenários (vazio e feliz) sabendo disso.

### 3.1 Estatística "cidades-sede" usa `Jogo.Estadio` distinto, não `Jogo.Cidade` distinto
`Jogo.Cidade` é um campo texto livre e tem inconsistências nos dados seedados fora da "Primeira Fase" (variações como "Nova Iorque"/"Nova Jersey"/"Nova York/Nova Jersey", "Seattle"/"Seattle Field", "Vancouver"/"Vancouver Place", e um caso onde o nome do estádio "Azteca" aparece como cidade). Contar `Cidade` distinto sobre todos os `Jogos` resulta em 21 valores, divergindo das 16 cidades-sede oficiais do torneio (CLAUDE.md, `copa2026_cidades_sede_estadios.txt`). `Jogo.Estadio`, por outro lado, tem exatamente 16 valores distintos e limpos em todas as fases, com correspondência 1:1 com as cidades-sede oficiais. `ObterEstatisticasAsync` deve calcular `TotalCidadesSede` como `Jogos.Select(j => j.Estadio).Distinct().Count()`, não `j.Cidade`. Alternativa considerada: filtrar `Cidade` distinto apenas em jogos de "Primeira Fase" (também dá 16). Rejeitada em favor de `Estadio` por não depender de um filtro de fase adicional e por ser robusta mesmo que dados de fases futuras (mata-mata) sejam corrigidos ou alterados depois.

### 4. Componente de gráfico genérico via JSInterop com Chart.js local (self-hosted)
Criar `Components/Shared/ChartJs.razor` (ou `Components/Shared/Charts/ChartJs.razor`) que recebe `ElementId`, `ChartType` ("bar" nesta change), `Labels` e `Datasets` como parâmetros, e um módulo JS `wwwroot/js/chartInterop.js` com `initChart(elementId, config)` / `updateChart(elementId, config)` usando `IJSObjectReference` (isolamento de módulo JS, padrão recomendado para Blazor Web App). Chart.js é adicionado como arquivo estático em `wwwroot/lib/chartjs/chart.umd.min.js` (self-hosted), na versão **4.4.9** (última 4.x estável no momento desta change), em vez de CDN, para manter consistência com o Bootstrap já vendorizado localmente no projeto e evitar dependência de rede em runtime. Alternativa considerada: usar CDN do Chart.js. Rejeitada para manter o mesmo padrão de dependências estáticas locais já adotado para Bootstrap.
`RankingFifaChart.razor` usa `ChartJs.razor` internamente, passando os dados do Top 10 recebidos do serviço.

### 5. Render mode: Interactive Server, declarado explicitamente em `Home.razor`
O projeto tem `AddInteractiveServerComponents()` / `AddInteractiveServerRenderMode()` habilitados globalmente em `Program.cs`, mas isso apenas registra o serviço e o endpoint — cada página ainda precisa declarar `@rendermode InteractiveServer` individualmente para de fato abrir um circuito interativo (convenção já usada em `Counter.razor`; `App.razor` não aplica `@rendermode` global em `<Routes />`). **Correção em relação à primeira versão desta decisão:** validado durante a implementação que, sem `@rendermode InteractiveServer` em `Home.razor`, a página renderiza em modo estático (SSR), o `IJSRuntime` não abre um circuito real e o gráfico Chart.js não é inicializado (canvas permanece com dimensões default, sem erro visível no console). `Home.razor` declara `@rendermode InteractiveServer` para que o JSInterop do `ChartJs.razor` funcione.

### 6. `Home.razor` vira o composer da página
`Home.razor` (rota `/`) passa a apenas injetar `ILandingPageService`, carregar os dados necessários em `OnInitializedAsync`/parâmetros e renderizar os 5 componentes em sequência (Hero, Estatísticas, Próximos Jogos, Ranking, Simulador CTA), evitando concentrar HTML/lógica na página conforme exigido pelo CLAUDE.md.

## Risks / Trade-offs

- [Divergência visual do protótipo, já que ele usa CSS próprio (`css/styles.css`) e a Landing Page usará Bootstrap 5 + CSS complementar] → Mitigação: usar o protótipo como referência de estrutura/conteúdo e paleta de cores (via CSS custom properties reaproveitadas), não como CSS a ser copiado literalmente; aceitável ligeira diferença visual desde que a hierarquia de seções e informações seja preservada.
- [Chart.js self-hosted precisa ser adicionado manualmente ao `wwwroot` (sem gerenciador de pacotes front-end configurado no projeto)] → Mitigação: baixar o build UMD minificado do Chart.js e versionar em `wwwroot/lib/chartjs/`, documentando a versão usada em um comentário no arquivo ou no tasks.md.
- [Timezone/"próximos jogos" pode divergir entre `DateOnly`/`TimeOnly` do servidor e o horário local do visitante] → Mitigação: comparar `Data`/`Horario` com `DateTime.Now` do servidor (consistente com o restante do backend, sem exigir JSInterop de timezone nesta change).
- [Todos os jogos de "Primeira Fase" do seed já estão no passado a partir de 28/06/2026, então a seção "Próximos Jogos" mostrará o estado vazio por padrão em qualquer execução após essa data, inclusive na data atual do ambiente de desenvolvimento] → Mitigação: comportamento esperado e coberto pelo requirement "Ausência de próximos jogos"; documentado na Decision 3 e nas tasks 6.1/6.2 para não ser confundido com defeito durante a validação manual.
- [`Jogo.Cidade` tem inconsistências de nomenclatura fora da "Primeira Fase" que poderiam inflar a contagem de "cidades-sede" se usado ingenuamente] → Mitigação: usar `Jogo.Estadio` distinto (dado limpo, 16 valores) em vez de `Jogo.Cidade` distinto, conforme Decision 3.1.

## Open Questions

- Nenhuma pergunta em aberto que exija decisão antes da implementação; o número de "próximos jogos" exibidos (ex.: 6, alinhado ao protótipo) e o quanto de estilo do protótipo será replicado via CSS complementar ficam a critério da implementação em tasks.md, sem impacto nos requirements da spec.

## Why

O PortalCopa26 hoje expõe apenas a página Blazor padrão ("Hello, world!"). O protótipo HTML validado em `copa2026/prototipo` já define a experiência esperada para a página inicial (hero, próximos jogos, ranking FIFA, chamada ao simulador), e a fundação do projeto (EF Core, SQLite, entidades e seed data) já está pronta para alimentar essa página. É hora de implementar a Landing Page real, em Blazor, consumindo os dados oficiais já persistidos no banco.

## What Changes

- Substituir a página `Home.razor` padrão por uma Landing Page composta a partir de componentes reutilizáveis dedicados, seguindo a referência visual do protótipo (`copa2026/prototipo/index.html`, `css/styles.css`, `js/pages/home.js`).
- Criar os componentes em `Components/Pages/LandingPage/`: `HeroSection.razor`, `EstatisticasCopa.razor`, `ProximosJogos.razor`, `RankingFifaChart.razor`, `SimuladorPainel.razor`.
- Criar serviços de acesso a dados dedicados à Landing Page (contagem de seleções/cidades/jogos, próximos jogos da fase de grupos, Top 10 do ranking FIFA), registrados via DI nativa do ASP.NET Core, sem consultas EF Core diretas nos componentes Razor.
- Adicionar um componente `ChartJs`/wrapper de gráfico reutilizável que integra Chart.js via JSInterop, usado inicialmente para o gráfico de barras do Ranking FIFA (Top 10) e preparado para reutilização em futuras visualizações estatísticas do portal.
- Adicionar os assets do Chart.js ao `wwwroot` e o JS de interop necessário.
- `SimuladorPainel.razor` é apenas uma chamada de destaque (CTA) para o simulador — não implementa a lógica de simulação.

## Capabilities

### New Capabilities
- `landing-page`: Composição da página inicial do portal (hero, estatísticas da copa, próximos jogos, ranking FIFA Top 10 com gráfico Chart.js, e CTA para o simulador), incluindo os serviços de leitura de dados e o componente de gráfico reutilizável via JSInterop.

### Modified Capabilities
(nenhuma — capacidades existentes de domínio/dados/seed não têm requisitos alterados)

## Impact

- Código novo em `Components/Pages/LandingPage/*.razor` e `Services/` (novo diretório de serviços de leitura, ex.: `ILandingPageService`/implementação).
- Novo componente reutilizável de gráfico (ex.: `Components/Shared/ChartJs.razor` + `wwwroot/js/chartInterop.js`).
- Assets estáticos novos em `wwwroot/lib/chartjs` (biblioteca Chart.js).
- Substituição do conteúdo de `Components/Pages/Home.razor`.
- Leitura via `AppDbContext` (Selecoes, Jogos, RankingFifa) somente através dos novos serviços — sem alterações de schema/EF Core.
- Sem alterações em autenticação, simulador, ou demais páginas (Jogos, Grupos, Seleções, Ranking completo) — permanecem fora do escopo.

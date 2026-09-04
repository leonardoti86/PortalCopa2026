## 1. Assets do Chart.js

- [x] 1.1 Baixar o build UMD minificado do Chart.js **versão 4.4.9** e versioná-lo em `wwwroot/lib/chartjs/chart.umd.min.js`
- [x] 1.2 Registrar a referência ao script do Chart.js e ao módulo de interop no `App.razor` (ou layout apropriado)

## 2. Serviço de dados da Landing Page

- [x] 2.1 Criar `Services/LandingPage/Dtos/CopaEstatisticasDto.cs` (TotalSelecoes, TotalCidadesSede, TotalJogos)
- [x] 2.2 Criar `Services/LandingPage/Dtos/ProximoJogoDto.cs` (Data, Horario, Grupo, SelecaoMandante, CodigoMandante, SelecaoVisitante, CodigoVisitante, Estadio, Cidade)
- [x] 2.3 Criar `Services/LandingPage/Dtos/RankingItemDto.cs` (Posicao, Selecao, CodigoSelecao, Pontos)
- [x] 2.4 Criar `Services/LandingPage/ILandingPageService.cs` com `ObterEstatisticasAsync`, `ObterProximosJogosAsync(int quantidade)`, `ObterRankingTopAsync(int quantidade)`
- [x] 2.5 Implementar `Services/LandingPage/LandingPageService.cs` usando `AppDbContext` (contagens de `Selecoes`/`Jogos`/**`Estadio` distinto de `Jogos`** para `TotalCidadesSede` — não usar `Cidade`, pois tem valores inconsistentes fora da "Primeira Fase" (ver design.md, Decision 3.1); filtro de `Jogos` por `Fase == "Primeira Fase"` e `Data`/`Horario` futuros ordenados; `RankingsFifa` ordenado por `Posicao` limitado ao Top N)
- [x] 2.6 Registrar `ILandingPageService`/`LandingPageService` no `Program.cs` via `AddScoped`

## 3. Componente de gráfico reutilizável (Chart.js + JSInterop)

- [x] 3.1 Criar `wwwroot/js/chartInterop.js` com funções `initChart(elementId, config)` e `updateChart(elementId, config)` usando a API do Chart.js
- [x] 3.2 Criar `Components/Shared/ChartJs.razor` com parâmetros `ElementId`, `ChartType`, `Labels`, `Datasets`, carregando o módulo JS via `IJSRuntime.InvokeAsync<IJSObjectReference>` em `OnAfterRenderAsync` (primeira renderização) e atualizando o gráfico quando os parâmetros mudarem
- [x] 3.3 Garantir o `Dispose`/`IAsyncDisposable` do componente para liberar a referência ao módulo JS

## 4. Componentes da Landing Page

- [x] 4.1 Criar `Components/Pages/LandingPage/HeroSection.razor` (título, tagline, países-sede, CTA para `/simulador` e `/jogos`) recebendo dados via parâmetro quando aplicável
- [x] 4.2 Criar `Components/Pages/LandingPage/EstatisticasCopa.razor` recebendo `CopaEstatisticasDto` e exibindo os stat-tiles (seleções, cidades-sede, jogos)
- [x] 4.3 Criar `Components/Pages/LandingPage/ProximosJogos.razor` recebendo `IReadOnlyList<ProximoJogoDto>` e exibindo os cards de jogo (data, hora, grupo, seleções com bandeira via `https://api.fifa.com/api/v3/picture/flags-sq-4/{Codigo}`, estádio, cidade), com mensagem de estado vazio quando a lista estiver vazia
- [x] 4.4 Criar `Components/Pages/LandingPage/RankingFifaChart.razor` recebendo `IReadOnlyList<RankingItemDto>`, renderizando a tabela Top 10 e usando `ChartJs.razor` para o gráfico de barras (labels = seleções, dataset = pontos)
- [x] 4.5 Criar `Components/Pages/LandingPage/SimuladorPainel.razor` com o CTA estático de chamada para `/simulador` (sem lógica de simulação)

## 5. Composição da página inicial

- [x] 5.1 Atualizar `Components/Pages/Home.razor` para injetar `ILandingPageService`, carregar estatísticas/próximos jogos/ranking em `OnInitializedAsync` e compor os 5 componentes da Landing Page em sequência (inclui `@rendermode InteractiveServer` explícito na página — necessário para o circuito SignalR/JSInterop do gráfico funcionar; ver design.md Decision 5)
- [x] 5.2 Adicionar/ajustar CSS complementar (ex.: `Home.razor.css` ou classes em `wwwroot/app.css`) usando a paleta e a estrutura visual do protótipo (`copa2026/prototipo/css/styles.css`) sobre a base do Bootstrap 5

## 6. Validação

- [x] 6.1 Rodar a aplicação (`dotnet run`) e conferir visualmente a Landing Page: hero, estatísticas (conferir que "cidades-sede" mostra 16), ranking com gráfico renderizado e CTA do simulador — validado via browser: 48 seleções / 16 cidades-sede / 104 jogos, tabela + gráfico de barras do Top 10 renderizados
- [x] 6.2 Validar os dois estados de "Próximos Jogos": **(a) estado vazio** — com a data atual do ambiente, todos os jogos de "Primeira Fase" do seed (11–27/06/2026) já estão no passado, então a mensagem de "sem próximos jogos" deve aparecer por padrão, sem erros (comportamento esperado, ver design.md Decision 3); **(b) estado feliz** — validar temporariamente (ex.: ajustando a comparação de data em `LandingPageService` para uma constante fixa dentro do intervalo do seed, ou depurando `ObterProximosJogosAsync` isoladamente) que a lista retorna os jogos ordenados por data/horário crescente com seleções, grupo, estádio e cidade corretos, e depois reverter o ajuste temporário antes de finalizar — ambos os estados validados via browser (estado vazio com `DateTime.Now` real; estado feliz com data de referência temporária 11/06/2026, depois revertida)
- [x] 6.3 Confirmar que nenhum componente Razor da Landing Page faz consulta EF Core diretamente (toda leitura passa por `ILandingPageService`)
- [x] 6.4 Rodar `dotnet build` para garantir que o projeto compila sem erros/novos warnings relevantes

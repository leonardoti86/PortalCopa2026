## 1. Serviço de Seleções

- [x] 1.1 Criar `Services/Selecoes/Dtos/SelecaoResumoDto.cs` (Id, Nome, Codigo, GrupoCodigo, FlagUrl) para a listagem
- [x] 1.2 Criar `Services/Selecoes/Dtos/JogadorDto.cs` (Nome, Posicao, Idade, Gols)
- [x] 1.3 Criar `Services/Selecoes/Dtos/SelecaoDetalheDto.cs` (Nome, Codigo, GrupoCodigo, FlagUrl, Tecnico, RankingPosicao?, RankingPontos?, Jogadores: IReadOnlyList<JogadorDto>)
- [x] 1.4 Criar `Services/Selecoes/ISelecaoService.cs` com `ObterListagemAsync()` (48 `SelecaoResumoDto` ordenados por nome) e `ObterDetalheAsync(int selecaoId)` (retorna `SelecaoDetalheDto` ou null)
- [x] 1.5 Implementar `Services/Selecoes/SelecaoService.cs` usando `AppDbContext` (`Include(Grupo)`, `Include(RankingFifa)` na listagem; jogadores carregados só no detalhe), construindo `FlagUrl` a partir do `Codigo` (`https://api.fifa.com/api/v3/picture/flags-sq-4/{Codigo}`)
- [x] 1.6 Registrar `builder.Services.AddScoped<ISelecaoService, SelecaoService>();` em `Program.cs`

## 2. Componentes de Seleções

- [x] 2.1 Criar `Components/Pages/Selecoes/CartaoSelecao.razor` (bandeira, nome, código FIFA, grupo; `EventCallback` ao clicar)
- [x] 2.2 Criar `Components/Pages/Selecoes/FiltroSelecao.razor` (campo de busca por nome + `select` de grupo A-L, com `EventCallback<string>`/`EventCallback<string?>` para busca e grupo)
- [x] 2.3 Criar `Components/Pages/Selecoes/TabelaElencoSelecao.razor` (recebe `IReadOnlyList<JogadorDto>`, renderiza nome, posição, idade, gols, ordenados alfabeticamente por nome)
- [x] 2.4 Criar `Components/Pages/Selecoes/ModalSelecao.razor` (modal Bootstrap 5 controlado por parâmetro `SelecaoDetalheDto?`; exibe bandeira, nome, grupo, técnico, ranking FIFA — com fallback "sem dado na fonte" quando ranking for nulo — e `TabelaElencoSelecao`)
- [x] 2.5 Criar `Components/Pages/Selecoes/Selecoes.razor` com `@page "/selecoes"`, `@rendermode InteractiveServer`, `@inject ISelecaoService`, orquestrando `FiltroSelecao`, grid de `CartaoSelecao` e `ModalSelecao`
- [x] 2.6 Implementar lógica de busca (case-insensitive, contains) e filtro por grupo em `Selecoes.razor`, combináveis, sobre a lista carregada em `OnInitializedAsync`
- [x] 2.7 Implementar abertura do modal (chama `ISelecaoService.ObterDetalheAsync` ao clicar em um `CartaoSelecao`) e fechamento (limpa o estado selecionado)

## 3. Estilo e Navegação

- [x] 3.1 Criar `wwwroot/css/selecoes.css` reaproveitando os tokens `--lp-color-*` de `landing-page.css`, com visual equivalente ao grid/cartões/modal do protótipo (`../prototipo/css/styles.css`, classes `.team-search`, `.team-grid-card`, `.team-detail__*`, `.player-list`, `.player-row`)
- [x] 3.2 Adicionar `<link rel="stylesheet" href="@Assets["css/selecoes.css"]" />` em `Components/App.razor` (e o script `lib/bootstrap/dist/js/bootstrap.bundle.min.js`, necessário para o `data-bs-toggle` do modal funcionar — nenhum outro CSS/JS do Bootstrap era carregado até então)
- [x] 3.3 Adicionar item "Seleções" (`href="selecoes"`) em `Components/Layout/NavMenu.razor`

## 4. Validação

- [x] 4.1 Rodar a aplicação (`dotnet run`) e conferir na página `/selecoes`: 48 cartões exibidos, busca funcionando, filtro por grupo (A-L) funcionando isoladamente e combinado com a busca
- [x] 4.2 Abrir o modal de pelo menos 3 seleções diferentes e conferir bandeira, nome, grupo, técnico, ranking FIFA e a lista de jogadores (nome, posição, idade, gols) batendo com `copa2026_selecoes_jogadores.txt` e `copa2026_pais_tecnicos.txt` (testado: Brasil, Escócia, Curaçao)
- [x] 4.3 Conferir o comportamento com uma seleção cujo elenco na fonte oficial tenha menos de 26 jogadores, confirmando que o sistema exibe exatamente a quantidade da fonte (sem completar nem truncar) (Escócia: 22 jogadores, batendo com a fonte)
- [x] 4.4 Conferir que nenhuma página/componente acessa `AppDbContext` diretamente (apenas via `ISelecaoService`)

## 1. Serviço de Jogos

- [x] 1.1 Criar `Services/Jogos/Dtos/JogoDto.cs` (record com Id, Data, Horario, Grupo, SelecaoMandante, CodigoMandante, SelecaoVisitante, CodigoVisitante, Estadio, Cidade)
- [x] 1.2 Criar `Services/Jogos/IJogosService.cs` com `Task<IReadOnlyList<JogoDto>> ObterJogosFaseGruposAsync(CancellationToken cancellationToken = default)`
- [x] 1.3 Implementar `Services/Jogos/JogosService.cs` consultando `AppDbContext.Jogos` filtrando pela fase de grupos ("Primeira Fase"), com seleções mandante/visitante e grupo não nulos, ordenado por Data e Horário crescente
- [x] 1.4 Registrar `IJogosService`/`JogosService` como `AddScoped` em `Program.cs`

## 2. Componentes de apresentação

- [x] 2.1 Criar `Components/Pages/Jogos/JogosDataHeader.razor` (parâmetro `DateOnly Data`, renderiza cabeçalho de data formatado)
- [x] 2.2 Criar `Components/Pages/Jogos/JogoCard.razor` (parâmetro `JogoDto Jogo`, renderiza horário, bandeiras/nomes das seleções, grupo, estádio e cidade — seguindo o padrão visual de `ProximosJogos.razor`)
- [x] 2.3 Criar `Components/Pages/Jogos/JogosFiltro.razor` (parâmetros `IReadOnlyList<string> Grupos` e `string? GrupoSelecionado`, `EventCallback<string?> GrupoSelecionadoChanged`, com opção "Todos os grupos")

## 3. Página de Jogos

- [x] 3.1 Criar `Components/Pages/Jogos/Jogos.razor` com `@page "/jogos"`, injetando `IJogosService`
- [x] 3.2 Carregar a lista completa de jogos da fase de grupos em `OnInitializedAsync`
- [x] 3.3 Implementar estado de filtro por grupo (`string? _grupoSelecionado`) e lista derivada filtrada
- [x] 3.4 Agrupar a lista filtrada por `Data` (preservando a ordenação por Data/Horário do serviço) e renderizar um `JogosDataHeader` + `JogoCard`(s) por grupo de data
- [x] 3.5 Exibir mensagem de estado vazio quando o filtro não retornar jogos
- [x] 3.6 Adicionar botão "Ver Grupos" (`href="/grupos"`) na página

## 4. Validação

- [x] 4.1 Rodar `dotnet build` na solução e confirmar ausência de erros/warnings novos
- [x] 4.2 Validar manualmente: listagem completa, agrupamento por data, ordenação, filtro por grupo (incluindo grupo sem resultado), exibição de grupo/estádio em cada jogo, e navegação do botão "Ver Grupos"
- [x] 4.3 Confirmar que nenhum dado fictício foi introduzido — todos os jogos exibidos correspondem aos 72 jogos da fase de grupos vindos do Seed Data

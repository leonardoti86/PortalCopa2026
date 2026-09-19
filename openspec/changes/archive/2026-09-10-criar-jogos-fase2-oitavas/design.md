## Context

Hoje `Jogo` (`Models/Copa/Jogo.cs`) sempre tem `SelecaoMandanteId`/`SelecaoVisitanteId` apontando para uma `Selecao` (nulos apenas enquanto o jogo não foi resolvido — comentário já deixado no código prevendo isso). `JogosFaseGruposQuery` filtra por `Fase == "Primeira Fase"`. O seed (`SeedData.SeedAsync`) lê um único `matches.json` e cria um `Jogo` para cada entrada, resolvendo o nome do time via `MatchSeedDto.ResolveTeamName` (que já reconhece um formato `{"type":"team","name":...}` e devolve `null` para qualquer outro `type`, incluindo os objetos `{"type":"winner","sourceId":...}` já presentes no arquivo hoje).

`matches.json` já contém, hoje, entradas para Segunda Fase (`r32-1`..`r32-16`) e Oitavas (`r16-1`..`r16-8`), aparentemente pré-carregadas a partir dos mesmos arquivos oficiais em `./fontes`, com datas/cidades/estádios batendo com `copa2026_jogos_segunda_fase.txt`, `copa2026_jogos_oitavas.txt` e `copa2026_cidades_sede_estadios.txt`. Essas entradas são inertes hoje: como não existe conceito de "jogo de origem" no modelo, os `Jogo` de Oitavas (e das fases seguintes, também presentes no arquivo) são semeados com `SelecaoMandanteId`/`SelecaoVisitanteId` nulos e nenhuma referência à Segunda Fase é persistida — o `sourceId` do JSON é descartado no processo de seed.

Ver proposal.md para a motivação. Este documento cobre como o modelo, o seed e os serviços mudam para tornar os dados de Segunda Fase e Oitavas reais e a propagação de vencedores automática.

## Goals / Non-Goals

**Goals:**
- Modelo de `Jogo` capaz de representar "seleção só conhecida após o vencedor de outro jogo", reutilizável por Quartas/Semis/3º lugar/Final sem novas colunas.
- Seed dedicado e idempotente para os 16 jogos da Segunda Fase e os 8 das Oitavas, batendo exatamente com os arquivos oficiais.
- Resolução de vencedor centralizada em um único lugar (sem duplicar a regra "maior placar, ou pênaltis em caso de empate" em páginas/serviços diferentes).
- Páginas Fase2 e Oitavas com o mesmo padrão visual/estrutural já usado em Jogos/Grupos.

**Non-Goals:**
- Implementar Quartas, Semifinais, Terceiro Lugar, Final ou o simulador do mata-mata (fora do escopo da proposal).
- Modelar prorrogação como um conceito separado — o placar após prorrogação é apenas o placar oficial final (`PlacarMandante`/`PlacarVisitante`); só a disputa de pênaltis precisa de campos próprios, por ser a única situação em que o placar oficial por si só não decide o jogo.
- Alterar a classificação de grupos, o simulador ou a página de Jogos/Grupos existentes.

## Decisions

### 1. Referência a jogos de origem via FK auto-referenciada em `Jogo`, não uma tabela de "confrontos"
Adicionar a `Jogo`: `JogoOrigemMandanteId`/`JogoOrigemMandante` e `JogoOrigemVisitanteId`/`JogoOrigemVisitante` (ambos `int?`, FK para `Jogo.Id`, `OnDelete(DeleteBehavior.Restrict)` — mesmo padrão já usado para `SelecaoMandante`/`SelecaoVisitante`/`Grupo`). Quando um lado do jogo é decidido pelo vencedor de outro jogo, o `SelecaoXId` correspondente permanece `null` e o `JogoOrigemXId` é preenchido; quando o lado é uma seleção real (todo jogo da Segunda Fase, e qualquer fase futura já com seleção definida), `SelecaoXId` é preenchido e `JogoOrigemXId` fica `null`.

Adicionar também `Ordem` (`int?`, sem FK) a `Jogo`: o número oficial do jogo dentro da sua fase eliminatória ("Segundafase N"/"Oitavas N" do arquivo oficial). É persistido (não só usado em memória durante o seed) porque a página Oitavas precisa exibir "Vencedor Segunda Fase N" a partir do `JogoOrigemMandante`/`JogoOrigemVisitante` já carregado do banco, em qualquer momento (não só durante o seed) — sem essa coluna não haveria como recuperar esse número depois que a aplicação reinicia. Fica `null` para jogos da Fase de Grupos, que não têm essa numeração oficial exposta nas telas atuais.

Alternativas consideradas:
- Tabela separada `ConfrontoEliminatorio` (jogo origem A + origem B + jogo destino): mais indireção sem necessidade — o requisito já é 1:1 com o próprio `Jogo`, e uma tabela nova complicaria as queries de exibição sem ganhar nada, já que um jogo eliminatório nunca tem mais de dois jogos de origem (um por lado).
- Guardar `sourceId` como string livre (como o JSON atual faz) sem FK: perde integridade referencial e obriga a resolver a seleção por uma segunda consulta/índice fora do EF Core; uma FK real deixa o EF Core (e o SQLite) garantirem consistência e permite `Include` direto.

### 2. Vencedor calculado sob demanda, nunca copiado/persistido
Um novo `JogoEliminatorioResolver` (`Services/Copa/JogoEliminatorioResolver.cs`, ao lado de `ClassificacaoCalculator`) expõe:
- `int? ResolverSelecaoId(Jogo jogo, bool mandante)`: se `SelecaoXId` estiver preenchido, retorna-o; senão, se `JogoOrigemXId` estiver preenchido, carrega o jogo de origem e retorna `ObterVencedorId` dele (recursivo); senão `null`.
- `int? ObterVencedorId(Jogo jogo)`: resolve os dois lados via `ResolverSelecaoId`; se algum lado ou algum placar oficial for `null`, retorna `null`; se os gols forem diferentes, retorna o lado com mais gols; se empatados, exige `PlacarPenaltisMandante`/`PlacarPenaltisVisitante` preenchidos e diferentes, senão retorna `null`.

Isso satisfaz "um jogo sempre deve ter um vencedor" (nunca fica ambíguo — ou tem vencedor, ou o resultado está incompleto) sem nunca gravar a seleção vencedora em lugar nenhum: toda leitura da página Oitavas chama esse resolver, então uma mudança no placar da Segunda Fase se reflete imediatamente, sem job de recálculo nem cache.

`docs/RegrasCopa2026.md` ("Resultado do Jogo") é explícito: *"Nas fases eliminatórias não pode existir empate"*. Por isso `FaseEliminatoriaService.AtualizarPlacarOficialAsync` (Decisão 5) valida, antes de persistir, que um placar de pênaltis informado não seja igual (`PlacarPenaltisMandante == PlacarPenaltisVisitante`) — rejeitando a atualização com erro em vez de gravar um estado que nunca converge para um vencedor. `PlacarPenaltisMandante`/`PlacarPenaltisVisitante` nulos (pênaltis ainda não jogados) continuam permitidos e resultam em vencedor indefinido, conforme o resolver acima.

Alternativa considerada: gravar o `SelecaoVencedoraId` calculado direto no jogo das Oitavas quando o resultado da Segunda Fase muda (via evento/trigger). Rejeitada — é exatamente o "hardcode/cópia de seleção" que a proposal pede para evitar, e introduz um caminho de escrita a mais para manter sincronizado.

### 3. Seed dedicado a partir de dois JSONs novos, derivados de `matches.json` + fontes oficiais
Extrair as 16 entradas `r32-*` para `Data/SeedJson/matches_segunda_fase.json` e as 8 `r16-*` para `Data/SeedJson/matches_oitavas.json`, mantendo a mesma convenção de nomes de campo em inglês já usada em `teams.json`/`groups.json`/`players.json`/`matches.json` (`Data/SeedDtos.cs`: `date`, `time`, `home`, `away`, `city`, `stadium`), em vez de introduzir nomes em português só para estes dois arquivos:
- `matches_segunda_fase.json`: `order` (int, 1–16, = "Segundafase N" do arquivo oficial), `date`, `time`, `home`, `away` (strings simples, sem o wrapper `{"type":"team",...}`, já que aqui a seleção é sempre conhecida), `city`, `stadium`.
- `matches_oitavas.json`: `order` (int, 1–8, = "Oitavas N"), `date`, `time`, `city`, `stadium`, `homeSourceOrder`, `awaySourceOrder` (int, 1–16, apontam para o `order` da Segunda Fase — não há `home`/`away`, pois a seleção nunca é fixa).

As 16 + 8 entradas correspondentes são removidas de `matches.json` (ficam só `grp-*`, i.e., a Fase de Grupos) para não duplicar o seed dessas fases.

Ao extrair, dois ajustes de dado (não invenção — apenas usar o rótulo oficial já existente nos próprios arquivos de `./fontes`, no lugar de um rótulo inconsistente que já estava em `matches.json`):
- Nome de seleção: `matches.json` usa "Bósnia" na entrada `r32-7`; `teams.json` (a base de seleções já semeada) cadastra essa seleção como "Bósnia e Herzegovina". `SeedJogosSegundaFase` usa o nome oficial de `teams.json` para poder resolver a `Selecao`.
- Cidade das Oitavas: `matches.json` usa os rótulos textuais do arquivo de fontes das Oitavas tal como escritos ("Seattle Field", "Vancouver Place", "Nova Jersey", "Azteca"), que não são os nomes de cidade-sede oficiais. `SeedJogosOitavas` usa o nome de cidade de `copa2026_cidades_sede_estadios.txt` correspondente ao estádio (Seattle, Vancouver, Nova York/Nova Jersey, Cidade do México — o mesmo já usado nas entradas de Segunda Fase/Grupos para esses mesmos estádios), mantendo o nome do estádio como está.

Mapeamento completo (data ISO, horário BRT, ordem = "Segundafase N"/"Oitavas N" do arquivo oficial):

Segunda Fase (`Fase = "Segunda Fase"`):
| Ordem | Data | Hora | Mandante | Visitante | Estádio | Cidade |
|---|---|---|---|---|---|---|
| 1 | 2026-06-29 | 17:30 | Alemanha | Paraguai | Gillette Stadium | Boston |
| 2 | 2026-06-30 | 18:00 | França | Suécia | MetLife Stadium | Nova York/Nova Jersey |
| 3 | 2026-06-28 | 16:00 | África do Sul | Canadá | SoFi Stadium | Los Angeles |
| 4 | 2026-06-29 | 22:00 | Holanda | Marrocos | Estadio BBVA | Monterrey |
| 5 | 2026-07-02 | 20:00 | Portugal | Croácia | BMO Field | Toronto |
| 6 | 2026-07-02 | 16:00 | Espanha | Áustria | SoFi Stadium | Los Angeles |
| 7 | 2026-07-01 | 21:00 | Estados Unidos | Bósnia e Herzegovina | Levi's Stadium | Santa Clara |
| 8 | 2026-07-01 | 17:00 | Bélgica | Senegal | Lumen Field | Seattle |
| 9 | 2026-06-29 | 14:00 | Brasil | Japão | NRG Stadium | Houston |
| 10 | 2026-06-30 | 14:00 | Côte d'Ivoire | Noruega | AT&T Stadium | Dallas |
| 11 | 2026-06-30 | 22:00 | México | Equador | Estádio Azteca | Cidade do México |
| 12 | 2026-07-01 | 13:00 | Inglaterra | República Democrática do Congo | Mercedes-Benz Stadium | Atlanta |
| 13 | 2026-07-03 | 19:00 | Argentina | Cabo Verde | Hard Rock Stadium | Miami |
| 14 | 2026-07-03 | 15:00 | Austrália | Egito | AT&T Stadium | Dallas |
| 15 | 2026-07-03 | 00:00 | Suíça | Argélia | BC Place | Vancouver |
| 16 | 2026-07-03 | 22:30 | Colômbia | Gana | Arrowhead Stadium | Kansas City |

Oitavas de Final (`Fase = "Oitavas de Final"`, mandante/visitante sempre = vencedor da Segunda Fase de ordem X):
| Ordem | Data | Hora | Origem Mandante | Origem Visitante | Estádio | Cidade |
|---|---|---|---|---|---|---|
| 1 | 2026-07-04 | 18:00 | Segunda Fase 1 | Segunda Fase 2 | Lincoln Financial Field | Filadélfia |
| 2 | 2026-07-04 | 14:00 | Segunda Fase 3 | Segunda Fase 4 | NRG Stadium | Houston |
| 3 | 2026-07-06 | 16:00 | Segunda Fase 5 | Segunda Fase 6 | AT&T Stadium | Dallas |
| 4 | 2026-07-06 | 21:00 | Segunda Fase 7 | Segunda Fase 8 | Lumen Field | Seattle |
| 5 | 2026-07-05 | 17:00 | Segunda Fase 9 | Segunda Fase 10 | MetLife Stadium | Nova York/Nova Jersey |
| 6 | 2026-07-05 | 21:00 | Segunda Fase 11 | Segunda Fase 12 | Estádio Azteca | Cidade do México |
| 7 | 2026-07-07 | 13:00 | Segunda Fase 13 | Segunda Fase 14 | Mercedes-Benz Stadium | Atlanta |
| 8 | 2026-07-07 | 17:00 | Segunda Fase 15 | Segunda Fase 16 | BC Place | Vancouver |

`SeedJogosSegundaFase` roda antes de `SeedJogosOitavas` (dependência de dado, não só de ordem de leitura): `SeedJogosOitavas` recebe o dicionário `ordem -> Jogo` produzido pela primeira chamada para popular `JogoOrigemMandante`/`JogoOrigemVisitante` antes de `SaveChangesAsync` (mesmo padrão já usado hoje para resolver `Selecao`/`Grupo` por nome/código em memória antes de salvar).

### 4. Fase de Grupos, Segunda Fase e Oitavas ficam com o `Fase` de cada jogo como constante nomeada
Cada consulta por fase já usa uma constante (`JogosFaseGruposQuery.FaseGrupos = "Primeira Fase"`). Seguindo o mesmo padrão, `Fase = "Segunda Fase"` e `Fase = "Oitavas de Final"` (nomes oficiais de `RegrasCopa2026.md`) ficam como constantes em um novo `JogosFaseEliminatoriaQuery` (`Services/Copa`), evitando strings soltas nas páginas/serviços novos.

### 5. Um serviço por página, seguindo o padrão de `GruposService`
`IFaseEliminatoriaService`/`FaseEliminatoriaService` (`Services/FaseEliminatoria`) expõe `ObterSegundaFaseAsync()` e `ObterOitavasAsync()` (cada um devolvendo a lista de jogos já resolvida, com nome/código de seleção quando definida ou `null` quando pendente) e `AtualizarPlacarOficialAsync(jogoId, placarMandante, placarVisitante, placarPenaltisMandante, placarPenaltisVisitante)`, reaproveitado pelas duas páginas. Isso evita duplicar a lógica de resolução de vencedor em cada página, mantendo a regra "não acessar DbContext direto em página Razor" do projeto.

`AtualizarPlacarOficialAsync` rejeita (lança `InvalidOperationException`, mesmo padrão de erro já usado em `GruposService` para jogo inexistente) duas situações inválidas antes de persistir: um placar de pênaltis empatado (Decisão 2 — nunca pode haver empate em fase eliminatória) e um placar oficial informado para um jogo cujo mandante ou visitante ainda dependa de um jogo de origem sem vencedor definido (não há seleção real para associar ao placar ainda).

### 6. Componentes de UI reaproveitam o padrão visual existente
`FaseEliminatoriaJogoDto` inclui, para cada lado, além do nome/código de seleção (nulo quando pendente), a ordem do jogo de origem da Segunda Fase quando aplicável (`OrigemMandanteOrdem`/`OrigemVisitanteOrdem`, nulos quando o lado já é uma seleção real) — é esse número que permite montar o texto "Vencedor Segunda Fase N" na tela sem a página precisar conhecer o `JogoOrigemXId` interno.

`Fase2.razor` e `Oitavas.razor` compartilham um novo componente `FaseEliminatoriaJogoCard.razor` (`Components/Pages/FaseEliminatoria`, ao lado das páginas), que reaproveita a estrutura de `Jogos.razor`/`JogoCard.razor` (`lp-match-card`) para a listagem e o padrão de input de placar de `GrupoJogo.razor` (`sim-jogo__placares`) para o registro de resultado — sem introduzir novas classes CSS de layout e sem duplicar a marcação/lógica entre as duas páginas. Quando uma seleção ainda não é conhecida (`OrigemXOrdem` preenchido), o card exibe "Vencedor Segunda Fase {OrigemXOrdem}" no lugar do nome/bandeira, sem chamada à API de bandeiras da FIFA para esse lado, e mantém os campos de placar desabilitados até os dois lados serem resolvidos. Os inputs de pênaltis (Decisão 2) só aparecem quando o placar oficial informado está empatado, reaproveitando o mesmo padrão de `@onchange` incremental de `GrupoJogo.razor`; um placar de pênaltis empatado é bloqueado no próprio input (mesma validação da Decisão 5) antes de notificar o componente pai.

## Risks / Trade-offs

- [Nomes/cidades ajustados na extração do JSON (Decisão 3) divergem do texto literal de `matches.json` atual] → Documentados explicitmente na tabela acima; ambos os ajustes usam um nome já oficial e já semeado em outro lugar do próprio projeto (não é dado inventado).
- [Resolver recursivo (`ResolverSelecaoId`) sem limite de profundidade explícito] → Como só existem duas fases implementadas nesta mudança (profundidade máxima 1), o risco é teórico; ao implementar Quartas/Semis/Final no futuro, o encadeamento de `JogoOrigemXId` continua uma árvore (nunca um ciclo, porque um jogo só referencia jogos de fases anteriores), então a recursão sempre termina.
- [Remover as entradas `r32-*`/`r16-*` de `matches.json`] → É uma mudança em um arquivo existente fora do "novo" código desta proposal, mas necessária: mantê-las geraria jogos duplicados (uma cópia inerte pelo seed genérico atual, outra funcional pelos novos métodos). Fica registrado aqui para o revisor confirmar a decisão antes do apply.
- [O requisito-base "Consistência dos Dados Semeados" de `seed-data` (spec atual) descreve todo Jogo como associado a Seleções corretas, o que deixa de ser literalmente verdade para os 8 jogos das Oitavas (que referenciam um jogo de origem, não uma Seleção)] → O delta `specs/seed-data/spec.md` desta change inclui um `MODIFIED Requirements` atualizando esse requisito para cobrir explicitamente jogos que referenciam um jogo de origem em vez de uma seleção, preservando a garantia de integridade referencial para esse caso.

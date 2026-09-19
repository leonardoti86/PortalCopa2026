## Context

Ver proposal.md — Why. O que já existe e condiciona o desenho:

- `Jogo` (`Models/Copa/Jogo.cs`) já representa "seleção só conhecida depois do vencedor de outro jogo" via `JogoOrigemMandanteId`/`JogoOrigemVisitanteId` e já tem `Ordem` (número oficial do jogo dentro da fase) e placar de pênaltis. **Nada disso muda nesta change** — não há entidade nova, coluna nova nem migration.
- `JogoEliminatorioResolver.ObterVencedorId` (`Services/Copa`) já decide o vencedor (mais gols, ou pênaltis no empate) e já resolve os lados recursivamente **no nível de Id**, porque `ResolverSelecaoId` chama a si mesmo pelo jogo de origem.
- `FaseEliminatoriaService.ResolverSelecao` (o que devolve a entidade `Selecao` para a tela) resolve **um único nível**: depois de obter o `vencedorId` do jogo de origem, ele compara com `jogoOrigem.SelecaoMandanteId`/`SelecaoVisitanteId`. Nas Oitavas isso basta (a origem é da Segunda Fase, que sempre tem seleções diretas); nas Quartas a origem é um jogo das Oitavas, cujos `SelecaoXId` são sempre `null` — o método devolveria `null` para sempre. É a única limitação real de código a ser removida.
- A carga do grafo (`CarregarComOrigemAsync`) usa uma cadeia de `Include`/`ThenInclude` de profundidade fixa 1. Semifinal → Quartas → Oitavas → Segunda Fase exige profundidade 3.
- `SeedData.SeedAsync` começa com `if (await context.Selecoes.AnyAsync()) return;`. O `portalcopa26.db` versionado no repositório **já está populado**, então qualquer seed novo colocado dentro desse guard nunca rodaria nele.
- `matches.json` ainda contém as 4 entradas `qf-*` e as 2 `sf-*`, semeadas hoje pelo laço genérico como `Jogo` com `Fase` `"Quartas"`/`"Semifinal"`, sem seleções, sem `Ordem` e sem jogo de origem — inertes e invisíveis, mas presentes no banco atual.
- O protótipo indicado (`..\prototipo\jogos.html`) não existe como arquivo: o protótipo é uma SPA (`../prototipo/index.html` + `js/pages/jogos.js`). O card de jogo dele é a referência de leiaute e já usa exatamente o rótulo `Venc. {fase} {ordem}` para o lado pendente. No app, esse leiaute já está portado em `lp-match-card` / `sim-jogo` (`wwwroot/css/landing-page.css`) e encapsulado em `FaseEliminatoriaJogoCard`.

Sobre prorrogação: a premissa (placar oficial = resultado final, já incluída a prorrogação) está registrada na proposal e repete o Non-Goal da change das Oitavas. Nenhuma decisão aqui depende dela.

## Goals / Non-Goals

**Goals:**
- Resolver a seleção de um lado de confronto em profundidade arbitrária do chaveamento, com um único caminho de código, reutilizado por todas as fases eliminatórias (inclusive Final e 3º lugar no futuro).
- Carregar o grafo eliminatório sem cadeia de `Include` proporcional à profundidade da fase.
- Semear Quartas e Semifinais de forma idempotente **por fase**, funcionando tanto em banco vazio quanto no banco já populado do repositório, sem recriar nem duplicar jogo nenhum e sem apagar resultado já registrado pelo usuário.
- Páginas novas indistinguíveis de `Oitavas.razor` em estrutura e estilo.

**Non-Goals:**
- Alterar `JogoEliminatorioResolver` (a regra de decisão do vencedor está correta e é reutilizada como está).
- Alterar o comportamento observável das páginas Fase2 e Oitavas — o texto do placeholder exibido nas Oitavas continua "Vencedor Segunda Fase N".
- Persistir o vencedor, materializar o chaveamento ou introduzir cache/recálculo em background.
- Final, disputa de terceiro lugar e simulador de mata-mata.

## Decisions

### 1. Dados oficiais: Quartas e Semifinais sempre por jogo de origem, nunca por seleção

Extraídos de `./fontes/copa2026_jogos_quartas.txt` e `./fontes/copa2026_jogos_semifinal.txt`. Estádio de cada cidade conforme `./fontes/copa2026_cidades_sede_estadios.txt` (os mesmos pares cidade/estádio já usados nas entradas `qf-*`/`sf-*` de `matches.json` e nas fases anteriores).

Quartas de Final (`Fase = "Quartas de Final"`):

| Ordem | Data | Hora (BRT) | Origem mandante | Origem visitante | Cidade | Estádio |
|---|---|---|---|---|---|---|
| 1 | 2026-07-09 | 17:00 | Oitavas 1 | Oitavas 2 | Boston | Gillette Stadium |
| 2 | 2026-07-10 | 16:00 | Oitavas 3 | Oitavas 4 | Los Angeles | SoFi Stadium |
| 3 | 2026-07-11 | 18:00 | Oitavas 5 | Oitavas 6 | Miami | Hard Rock Stadium |
| 4 | 2026-07-11 | 22:00 | Oitavas 7 | Oitavas 8 | Kansas City | Arrowhead Stadium |

Semifinais (`Fase = "Semifinais"`):

| Ordem | Data | Hora (BRT) | Origem mandante | Origem visitante | Cidade | Estádio |
|---|---|---|---|---|---|---|
| 1 | 2026-07-14 | 16:00 | Quartas 1 | Quartas 2 | Dallas | AT&T Stadium |
| 2 | 2026-07-15 | 16:00 | Quartas 3 | Quartas 4 | Atlanta | Mercedes-Benz Stadium |

Os nomes de fase seguem `./fontes/copa2026_fases.txt` e o padrão já adotado (`"Segunda Fase"`, `"Oitavas de Final"`), como constantes `FaseQuartas`/`FaseSemifinais` em `JogosFaseEliminatoriaQuery` — e **não** os rótulos curtos `"Quartas"`/`"Semifinal"` que o `matches.json` usa hoje nas entradas inertes.

Os dois novos JSONs (`Data/SeedJson/matches_quartas.json`, `matches_semifinais.json`) reusam exatamente o contrato de `matches_oitavas.json` (`order`, `date`, `time`, `city`, `stadium`, `homeSourceOrder`, `awaySourceOrder`), então o `OitavasMatchSeedDto` existente passa a se chamar `MataMataMatchSeedDto` e serve às três fases — renomeação pura, sem mudança de contrato JSON nem de comportamento.

### 2. Resolução da seleção passa a ser recursiva, reusando a decisão de vencedor existente

`FaseEliminatoriaService.ResolverSelecao(Jogo, bool mandante)` passa a, quando o lado não tem seleção direta: obter `vencedorId` do jogo de origem via `JogoEliminatorioResolver.ObterVencedorId` (inalterado) e então **resolver recursivamente os dois lados do jogo de origem**, devolvendo aquele cujo `Id` bate com o `vencedorId`:

```csharp
var mandanteOrigem = ResolverSelecao(origem, mandante: true);
if (mandanteOrigem?.Id == vencedorId) return mandanteOrigem;
var visitanteOrigem = ResolverSelecao(origem, mandante: false);
return visitanteOrigem?.Id == vencedorId ? visitanteOrigem : null;
```

Para as Oitavas o resultado é idêntico ao de hoje (a origem é da Segunda Fase e a primeira recursão já devolve a seleção direta), o que preserva o Non-Goal "não alterar a lógica das Oitavas"; para Quartas e Semifinais o encadeamento passa a funcionar em qualquer profundidade.

Alternativa considerada: manter a comparação por `SelecaoXId` e adicionar um segundo nível manual. Rejeitada — resolveria as Quartas e quebraria de novo nas Semifinais, e o mesmo remendo teria de ser refeito na Final.

### 3. Grafo eliminatório carregado de uma vez, em vez de `Include` por profundidade

Substituir `CarregarComOrigemAsync` (cadeia fixa de `Include`/`ThenInclude`) por uma carga única de **todos** os jogos eliminatórios:

```csharp
context.Jogos.Where(j => j.Ordem != null)
    .Include(j => j.SelecaoMandante)
    .Include(j => j.SelecaoVisitante)
```

São 16 + 8 + 4 + 2 = 30 linhas. `Ordem != null` seleciona exatamente as fases eliminatórias (os jogos da Fase de Grupos têm `Ordem` nula). Como todos os jogos referenciados estão no mesmo conjunto rastreado, o *relationship fixup* do EF Core preenche `JogoOrigemMandante`/`JogoOrigemVisitante` sozinho, em qualquer profundidade e sem `Include` algum para essas navegações.

`ObterSegundaFaseAsync`, `ObterOitavasAsync`, `ObterQuartasAsync`, `ObterSemifinaisAsync` e `AtualizarPlacarOficialAsync` passam todos por essa carga e apenas filtram por `Fase` (ou por `Id`) em memória — o que também faz a validação "as duas seleções precisam ser conhecidas" funcionar nas fases profundas, hoje impossível pelo mesmo motivo da Decisão 2.

Alternativa considerada: estender a cadeia de `ThenInclude` até profundidade 3. Rejeitada — a expressão cresce quadraticamente com a profundidade, é ilegível e teria de ser estendida de novo na Final.

### 4. Rótulo do placeholder derivado da fase de origem

`FaseEliminatoriaJogoDto` troca `OrigemMandanteOrdem`/`OrigemVisitanteOrdem` (`int?`) por `OrigemMandanteRotulo`/`OrigemVisitanteRotulo` (`string?`), montados no serviço a partir da fase e da `Ordem` do jogo de origem: `"Vencedor Segunda Fase 1"`, `"Vencedor Oitavas 2"`, `"Vencedor Quartas 1"`. O mapa fase → rótulo curto (`"Oitavas de Final"` → `"Oitavas"`, `"Quartas de Final"` → `"Quartas"`, `"Segunda Fase"` → `"Segunda Fase"`) fica em `JogosFaseEliminatoriaQuery`, junto das constantes de fase.

`FaseEliminatoriaJogoCard` deixa de concatenar o literal `"Vencedor Segunda Fase"` e passa a exibir o rótulo pronto — o texto visto hoje nas Oitavas não muda, e o card serve as quatro fases sem `if` por fase. Bandeira e API da FIFA continuam sendo chamadas só para o lado já resolvido.

Alternativa considerada: expor a fase de origem e a ordem separadamente e montar o texto no componente. Rejeitada — empurraria conhecimento de nomes de fase para a camada de UI, que hoje não tem nenhum.

### 5. Seed das fases finais fora do guard global, idempotente por fase

`SeedAsync` passa a orquestrar dois passos:

```csharp
public static async Task SeedAsync(AppDbContext context, CancellationToken ct = default)
{
    await SeedBaseAsync(context, ct);              // corpo atual, com o guard `if (Selecoes.Any()) return;`
    await SeedQuartasSemifinaisAsync(context, ct); // idempotente por fase, sempre executa
}
```

`SeedQuartasSemifinaisAsync`:

1. Remove os jogos inertes herdados do `matches.json` antigo: `Fase` em (`"Quartas"`, `"Semifinal"`) **e** `Ordem == null` **e** sem jogo de origem. É o predicado mais restritivo possível que pega exatamente as linhas legadas, e nunca os jogos oficiais criados por esta change (que sempre têm `Ordem` e origem). Essas linhas nunca tiveram tela, logo não podem conter resultado informado pelo usuário.
2. Se não existir nenhum jogo com `Fase == FaseQuartas`, cria os 4 a partir de `matches_quartas.json`, ligando `JogoOrigemMandante`/`JogoOrigemVisitante` aos jogos das Oitavas buscados por `Fase` + `Ordem`; salva.
3. Idem para `FaseSemifinais` a partir de `matches_semifinais.json`, ligando aos jogos das Quartas por `Fase` + `Ordem` (que existem, criados no passo anterior ou em execução anterior); salva.

Em banco vazio, o passo 1 não acha nada e os passos 2–3 rodam logo após `SeedBaseAsync` ter gravado Segunda Fase e Oitavas. Em banco já populado, `SeedBaseAsync` retorna no guard e os passos 2–3 completam só o que falta. Em reinício com tudo presente, os três passos são no-op — que é o requisito "os jogos não podem ser recriados nem duplicados".

Como consequência, as 4 entradas `qf-*` e as 2 `sf-*` saem de `matches.json` (mesmo tratamento dado a `r32-*`/`r16-*` na change anterior). `final-1` e `third-1` **permanecem** intocadas: continuam inertes, como hoje, e são assunto de uma change futura.

`SeedJogosOitavas` **não muda de assinatura**: continua sem retorno, como hoje. O vínculo Quartas → Oitavas é feito por consulta a `Fase` + `Ordem` direto no banco (via `context.Jogos`), não por um dicionário em memória — é o que permite `SeedQuartasSemifinaisAsync` funcionar também quando as Oitavas já estavam persistidas de uma execução anterior, quando `SeedJogosOitavas` nem chega a rodar. Devolver `Dictionary<int, Jogo>` "por simetria" com `SeedJogosSegundaFase` não teria consumidor e só acrescentaria alocação sem necessidade funcional.

### 6. Páginas espelham `Oitavas.razor`

`Components/Pages/Quartas.razor` (`/quartas`) e `Components/Pages/Semifinais.razor` (`/semifinais`) são cópias estruturais de `Oitavas.razor`: `@rendermode InteractiveServer`, injeção de `IFaseEliminatoriaService`, `lp-section` / `lp-section-title`, grade `row-cols-1 row-cols-md-2 row-cols-lg-3` de `FaseEliminatoriaJogoCard`, e o mesmo `OnPlacarAlteradoAsync` que persiste e **recarrega a lista da própria fase** pelo serviço — é esse recarregamento que faz o recálculo do chaveamento aparecer sem recriar jogo nenhum. Como nenhum estado é cacheado, entrar em `/semifinais` depois de mudar um placar em `/quartas` já mostra o valor novo.

O cabeçalho de cada página leva um link para a fase anterior e a seguinte (`Oitavas` ↔ `Quartas` ↔ `Semifinais`), no mesmo padrão do botão "Ver Segunda Fase" já existente em `Oitavas.razor`. Sem CSS novo.

## Risks / Trade-offs

- [As datas de `copa2026_jogos_quartas.txt` (09–11/07) e `copa2026_jogos_semifinal.txt` (14–15/07) divergem das faixas de `copa2026_fases.txt` (11–13/07 e 15–16/07)] → Prevalecem os arquivos de jogos, que são específicos por confronto e batem com as entradas `qf-*`/`sf-*` já presentes em `matches.json`; nenhuma data é inventada ou ajustada.
- [O passo 1 da Decisão 5 apaga linhas de um banco já existente] → Predicado restrito a `Fase` em (`"Quartas"`, `"Semifinal"`) com `Ordem == null` e sem jogo de origem: só as linhas legadas inertes, que nunca foram exibidas nem editáveis. Nenhum resultado do usuário pode existir nelas.
- [Carregar todos os 30 jogos eliminatórios a cada consulta de página] → Volume fixo e trivial; evita N+1 e cadeias de `Include` frágeis. Se um dia crescer, a otimização é local ao serviço.
- [Recursão de `ResolverSelecao` sem limite de profundidade] → O encadeamento de jogos de origem é uma árvore e sempre aponta para fases anteriores, nunca formando ciclo; a profundidade máxima com Final incluída é 4.
- [`FaseEliminatoriaJogoDto` muda de forma (ordem → rótulo), afetando Fase2 e Oitavas] → Mudança compilada, sem consumidor externo; o texto renderizado nas Oitavas continua idêntico, e o card é o único consumidor dos campos trocados.
- [Prorrogação não tem placar próprio] → Premissa registrada na proposal; um placar separado de prorrogação exigiria nova coluna e migration, explicitamente fora do escopo desta change.
- [`./fontes/copa2026_fases.txt` fecha com "total de 102 jogos" (assim como `CLAUDE.md` — seção "Dados Oficiais"), mas a própria tabela desse arquivo, linha a linha (72 + 16 + 8 + 4 + 2 + 1 + 1), soma 104 — a mesma inconsistência já existe na fonte oficial, não foi introduzida por esta change] → O critério de aceite desta change (Migration Plan, abaixo) usa **104**, por ser a soma da própria tabela de fases e por já bater com as 6 entradas `qf-*`/`sf-*` inertes hoje presentes em `matches.json`; o rodapé "102" da fonte é tratado como divergência da própria fonte, não como valor a perseguir.

## Migration Plan

Sem migration de EF Core — nenhum schema muda. A "migração" é de dados, feita pelo próprio seed no startup (Decisão 5): a aplicação sobe, remove as linhas inertes de Quartas/Semifinal e cria os 6 jogos oficiais que faltarem. Rollback é reverter o código: o banco volta a ter 6 jogos a mais em fases sem tela, exatamente o estado inerte de hoje, sem impacto em nenhuma página.

Verificação pós-deploy: 4 jogos com `Fase = "Quartas de Final"` e `Ordem` 1–4, 2 com `Fase = "Semifinais"` e `Ordem` 1–2, nenhum jogo com `Fase` `"Quartas"`/`"Semifinal"`, e o total de jogos igual a 72 + 16 + 8 + 4 + 2 + 2 (Final e 3º lugar, ainda inertes) = 104.

## Open Questions

- O passo de seed idempotente por fase (Decisão 5) deve, na change da Final/3º lugar, ser generalizado para uma tabela de fases ou repetido? Decidir quando essa change existir — não muda nada aqui.

## Context

O chaveamento eliminatório já existente (`fase-eliminatoria`) segue um encadeamento estritamente de **vencedores**: `Jogo.JogoOrigemMandanteId`/`JogoOrigemVisitanteId` apontam para o jogo de origem, e `JogoEliminatorioResolver.ObterVencedorId` resolve o vencedor desse jogo de origem em qualquer profundidade (Segunda Fase → Oitavas → Quartas → Semifinais). `FaseEliminatoriaService` carrega o grafo inteiro de uma vez (`Ordem != null`) e usa `ResolverSelecao` para andar recursivamente por esse encadeamento de vencedores, sem duplicar leitura ao banco.

O Terceiro Lugar introduz um relacionamento que ainda não existe no código: um jogo alimentado pelo **perdedor** de dois jogos de origem, não pelo vencedor. O modelo `Jogo` já tem os FKs necessários (`JogoOrigemMandanteId`/`JogoOrigemVisitanteId` apontando para as Semifinais); falta apenas a regra de leitura "perdedor", nunca persistida.

Ver `proposal.md` para a motivação completa. Ver `specs/final-copa/spec.md` para os requisitos.

## Goals / Non-Goals

**Goals:**
- Resolver e exibir mandante/visitante do Terceiro Lugar como o **perdedor** da Semifinal 1 e da Semifinal 2, e da Final como o **vencedor** das mesmas, reaproveitando ao máximo `FaseEliminatoriaService`/`FaseEliminatoriaJogoCard`/`JogoEliminatorioResolver`.
- Semear os dois jogos oficiais (Terceiro Lugar, Final) uma única vez, de forma idempotente, sem migração de banco.
- Exibir o painel de destaque do campeão de forma reativa ao estado atual da Final (aparece/desaparece conforme o resultado é definido/alterado/removido).

**Non-Goals:**
- Não alterar `ObterVencedorId`, `ResolverSelecao` nem o comportamento de nenhuma página/fase já existente (Fase2, Oitavas, Quartas, Semifinais).
- Não introduzir fases após a Final, nem generalizar o conceito de "perdedor" para as demais fases (elas nunca precisam dele).
- Não perseguir um dado oficial de "quantidade de títulos mundiais" nas fontes do torneio — não existe nelas; é dado histórico público, mantido em código.

## Decisions

### Decisão 1 — Reaproveitar o protótipo já portado, não o arquivo `..\prototipo\jogos.html`
O protótipo indicado no PRD (`..\prototipo\jogos.html`) não existe como arquivo — a change `criar-jogos-quartas-semifinais` já identificou isso: o protótipo real é a SPA em `../prototipo` (`index.html` + `js/pages/jogos.js`), e o leiaute de card já foi portado para o app como `lp-match-card`/`sim-jogo` (`wwwroot/css/landing-page.css`), encapsulado em `FaseEliminatoriaJogoCard.razor`. A página Final reaproveita esse mesmo card para os dois confrontos, mantendo consistência visual com Semifinais/Quartas/Oitavas.

`FaseEliminatoriaJogoCard` ganha um parâmetro opcional `TituloJogo` (string?, padrão `null`). Quando informado, substitui o texto `"Jogo @Ordem"` do cabeçalho do card; quando omitido, o comportamento atual é preservado exatamente (nenhuma página existente passa esse parâmetro). A página Final passa `TituloJogo="Disputa de Terceiro Lugar"` e `TituloJogo="Final"`, respectivamente — evitando que os dois cards da mesma página exibam simultaneamente o rótulo genérico "Jogo 1" (ambos os jogos têm `Ordem = 1` dentro da sua própria `Fase`, ver Decisão 5).

### Decisão 2 — Novo método `ObterPerdedorId`, sem alterar `ObterVencedorId`
`JogoEliminatorioResolver` ganha um método estático `ObterPerdedorId(Jogo jogo)`, espelhando `ObterVencedorId`: calcula os dois lados (mandante/visitante, resolvidos recursivamente como hoje) e retorna o lado que **não** é o vencedor, usando a mesma regra de decisão (placar oficial, com pênaltis apenas em caso de empate). Retorna `null` nos mesmos casos em que `ObterVencedorId` retornaria `null` (seleção pendente, sem placar, ou pênaltis empatados). Isso mantém a regra de decisão do mata-mata em um único lugar (sem duplicar a lógica de desempate) e não toca no método existente, preservando a restrição de não alterar a lógica das Semifinais.

**Alternativa considerada**: calcular o perdedor como "o outro lado" fora do resolver, diretamente no serviço da página Final. Rejeitada porque duplicaria a regra de decisão (placar, pênaltis) em dois lugares.

### Decisão 3 — Serviço dedicado `IFinalCopaService`, não extensão de `IFaseEliminatoriaService`, com leitura de grafo compartilhada (não duplicada)
Em vez de acrescentar métodos de Terceiro Lugar/Final a `IFaseEliminatoriaService` (que hoje só expõe consultas simétricas por fase, todas resolvidas por vencedor), a página Final usa um novo serviço `IFinalCopaService`/`FinalCopaService`, no mesmo padrão de injeção de dependência dos demais (`AddScoped`, sem acesso direto a `DbContext` em páginas Razor).

Hoje, `CarregarGrafoEliminatorioAsync`, `ResolverSelecao` e `RotularOrigem` são **métodos privados** de `FaseEliminatoriaService` — reaproveitá-los tal como estão exigiria duplicar essas ~70 linhas em `FinalCopaService`, o que viola a diretriz do CLAUDE.md de evitar duplicação de código. Para evitar isso, esses três métodos são **extraídos** de `FaseEliminatoriaService` para uma nova classe interna compartilhada, `Services/Copa/GrafoEliminatorioReader.cs` (`internal static`, mesmo assembly), com a mesma assinatura e o mesmo comportamento de hoje (refatoração que preserva o comportamento das fases já existentes — não é uma alteração de requisito, e não toca na regra de decisão de vencedor). `FaseEliminatoriaService` passa a delegar a essa classe em vez de conter os métodos diretamente.

`FinalCopaService` reaproveita `GrafoEliminatorioReader` assim:
- Carrega o grafo com `GrafoEliminatorioReader.CarregarAsync` (mesmos `Include`s).
- **Final** (vencedor): resolve mandante/visitante com `GrafoEliminatorioReader.ResolverSelecao(jogo, mandante)` — reaproveitado sem alteração, incluindo o rótulo de origem via `RotularOrigem` (prefixo `"Vencedor"`).
- **Terceiro Lugar** (perdedor): a origem é sempre a Semifinal correspondente diretamente (sem recursão adicional — a Semifinal já resolve seu próprio mandante/visitante via `ResolverSelecao`). `FinalCopaService` resolve o mandante/visitante da Semifinal de origem com `ResolverSelecao`, obtém o vencedor com `JogoEliminatorioResolver.ObterVencedorId(jogoOrigemSemifinal)` e retorna o lado que **não** é o vencedor (usando o novo `ObterPerdedorId`, Decisão 2). O rótulo do lado pendente é montado diretamente como `$"Perdedor Semifinal {jogoOrigemSemifinal.Ordem}"` — não reaproveita `RotularOrigem` (que é fixo em `"Vencedor"`) porque a resolução de perdedor nunca é recursiva; não há necessidade de generalizar `RotularOrigem` para um prefixo parametrizável.
- Reaproveita `AtualizarPlacarOficialAsync` (mesma assinatura, mesma validação de pênaltis empatados e de seleções pendentes) para persistir o placar de qualquer um dos dois jogos — copiado do método equivalente de `FaseEliminatoriaService` (a validação em si é curta e já está acoplada ao carregamento do grafo do próprio serviço; não há duplicação de regra de decisão, só do wrapper de persistência).
- Expõe a seleção campeã, quando a Final tiver vencedor definido, como `CampeaoDto` (Nome, Codigo, TitulosMundiais) — não a entidade `Selecao` do EF Core crua — para o painel de destaque já receber pronto o dado que precisa (Decisão 8), sem reimplementar a consulta a `TitulosMundiaisCopa` no componente Razor.

**Alternativa considerada**: colocar tudo em `FaseEliminatoriaService`. Rejeitada para não misturar, num único serviço, duas regras de resolução (vencedor sempre vs. vencedor-ou-perdedor conforme o jogo), o que tornaria `ResolverSelecao` menos legível para as fases que continuam usando apenas vencedor.

**Alternativa considerada**: duplicar `CarregarGrafoEliminatorioAsync`/`ResolverSelecao` em `FinalCopaService`. Rejeitada por violar "evitar duplicação de código" (CLAUDE.md) e criar risco de os dois serviços divergirem silenciosamente no futuro.

### Decisão 4 — DTO próprio da página Final, reaproveitando `FaseEliminatoriaJogoDto` para cada card
Os dois jogos (Terceiro Lugar, Final) continuam sendo representados por `FaseEliminatoriaJogoDto` (mesmo shape, mesmo componente de card), com os rótulos de origem construídos como `"Perdedor Semifinal {Ordem}"` / `"Vencedor Semifinal {Ordem}"` (a tabela `RotuloCurtoPorFase` de `JogosFaseEliminatoriaQuery` ganha a entrada `FaseSemifinais = "Semifinal"`, hoje ausente porque nada ainda derivava dela). Um DTO adicional, específico (ex.: `FinalCopaDto` ou dois retornos simples), agrega os dois `FaseEliminatoriaJogoDto` mais o campeão (quando houver).

### Decisão 5 — Seed idempotente por fase, via JSON, mesmo padrão de `SeedQuartasSemifinaisAsync`
`./fontes/copa2026_jogo_terceiro_lugar.txt` e `./fontes/copa2026_jogo_final.txt` só contêm fase, cidade, data, horário e o placeholder do confronto — **não contêm o nome do estádio**. O nome de cada estádio vem de `./fontes/copa2026_cidades_sede_estadios.txt`, cruzado pela cidade (Miami → Hard Rock Stadium; Nova York/Nova Jersey → MetLife Stadium).

O shape "jogo eliminatório sem seleção fixa, apenas com dois jogos de origem" já existe como `MataMataMatchSeedDto` (`Order`, `Date`, `Time`, `City`, `Stadium`, `HomeSourceOrder`, `AwaySourceOrder`), reaproveitado hoje por Oitavas/Quartas/Semifinais. Terceiro Lugar e Final usam exatamente o mesmo DTO e o mesmo mecanismo (`LoadJson<T>` a partir de recurso embutido), em vez de valores digitados diretamente em C# — mantendo o padrão já estabelecido em vez de abrir uma exceção a ele:
- `Data/SeedJson/matches_terceiro_lugar.json`: um único registro, `order: 1`, `date`/`time` de `copa2026_jogo_terceiro_lugar.txt`, `city: "Miami"`, `stadium: "Hard Rock Stadium"` (de `copa2026_cidades_sede_estadios.txt`), `homeSourceOrder: 1`, `awaySourceOrder: 2`.
- `Data/SeedJson/matches_final.json`: um único registro, `order: 1`, `date`/`time` de `copa2026_jogo_final.txt`, `city: "Nova York/Nova Jersey"`, `stadium: "MetLife Stadium"`, `homeSourceOrder: 1`, `awaySourceOrder: 2`.

`Data/SeedJson/matches.json` (o seed base, `SeedBaseAsync`) já contém dois registros inertes com `"phase": "Final"` e `"phase": "Terceiro Lugar"` (ids `final-1`/`third-1`), usando placeholders `"type": "winner"` que `SeedBaseAsync` nunca resolveu — sem `Ordem` nem jogo de origem. Uma checagem ingênua de "já existe jogo com essa `Fase`?" encontraria esses dois registros legados e nunca criaria os jogos reais e ligados ao chaveamento. `SeedTerceiroLugarFinalAsync` precisa, portanto, de um passo de limpeza desses registros legados antes de checar existência — exatamente o mesmo padrão que `SeedQuartasSemifinaisAsync` (Passo 1) já usa para limpar os legados `"Quartas"`/`"Semifinal"` de antes da change `criar-jogos-quartas-semifinais`.

`SeedData` ganha um passo final (`SeedTerceiroLugarFinalAsync`), rodando sempre (idempotente por `Fase`, mesmo padrão de `SeedQuartasSemifinaisAsync`), que:
1. Remove os jogos legados de `Fase IN ("Terceiro Lugar", "Final")` sem `Ordem` e sem jogo de origem (mesmo padrão do Passo 1 de `SeedQuartasSemifinaisAsync`).
2. Só insere o Terceiro Lugar se ainda não existir jogo com `Fase == "Terceiro Lugar"` (idem para `"Final"`, cada um checado independentemente, após a limpeza do passo 1).
3. Busca as Semifinais já persistidas por `Ordem` (1 e 2).
4. Lê `matches_terceiro_lugar.json` e `matches_final.json` e cria os dois jogos, com `JogoOrigemMandante`/`JogoOrigemVisitante` apontando para a Semifinal de `HomeSourceOrder`/`AwaySourceOrder` (1 e 2) — a mesma referência de armazenamento usada pela Final (vencedor) e pelo Terceiro Lugar (perdedor); a diferença entre os dois está inteiramente na **leitura** (Decisão 3), não no grafo persistido. Cada jogo recebe `Ordem = 1` dentro da sua própria `Fase`, já que cada `Fase` tem um único jogo.

Sem migração nova: `Jogo` já tem todas as colunas necessárias.

### Decisão 6 — Fotos dos estádios: URLs fixas e verificadas do Wikimedia Commons
Não há foto de estádio nas fontes oficiais nem API da FIFA para isso (a API da FIFA, conforme CLAUDE.md, cobre apenas bandeiras/logotipos de seleções). Cada um dos dois jogos usa uma URL de foto fixa, definida em código (não em banco de dados), de uma fotografia real e de licença livre, verificada nesta sessão de design:
- Terceiro Lugar (Hard Rock Stadium, Miami): `https://upload.wikimedia.org/wikipedia/commons/9/94/Hard_Rock_Stadium.jpg` (Wikimedia Commons, CC BY-SA 4.0, A.J. Lipp).
- Final (MetLife Stadium, Nova York/Nova Jersey): `https://upload.wikimedia.org/wikipedia/commons/2/2a/MetLife_Stadium_Exterior%2C_2026_FIFA_World_Cup_%28June_20%2C_2026%29.jpg` (Wikimedia Commons, CC BY 4.0).

Ambas as URLs foram verificadas ao vivo durante esta sessão de design (2026-09-17), via busca web seguida de acesso direto à página do arquivo em `commons.wikimedia.org` (ferramenta `WebFetch`), confirmando existência do arquivo, licença e a URL de download direto em `upload.wikimedia.org` antes de serem escolhidas — nenhuma URL foi inventada ou montada por padrão de nomenclatura sem confirmação (uma tentativa de montar uma URL de miniatura por convenção, sem verificação direta, foi descartada por retornar erro ao ser checada). A exibição usa `<img>` com `loading="lazy"`, `onerror` ocultando a imagem (mesmo padrão defensivo já usado para bandeiras em `FaseEliminatoriaJogoCard`) e CSS (`object-fit: cover`, altura máxima) para enquadrar a foto de alta resolução no card.

### Decisão 7 — Quantidade de títulos mundiais: tabela estática em código, histórico até 2022, sem somar o título simulado de 2026
Sem coluna nova em `Selecao` (restrição de não alterar o banco de dados) e sem essa informação nas fontes oficiais do torneio, a quantidade de títulos mundiais por seleção é mantida como uma tabela estática somente-leitura em código (`Dictionary<string, int>` chaveado pelo `Codigo` da seleção), cobrindo as 48 seleções do torneio — qualquer uma pode teoricamente chegar à Final, já que o simulador não valida "força" das seleções em nenhuma fase anterior. É dado histórico público e verificável (não fictício, não relacionado a confrontos do torneio), portanto fora do escopo da regra "usar exclusivamente `./fontes`", que trata dos dados oficiais do torneio (jogos, grupos, seleções, ranking).

A tabela reflete os títulos conquistados **até a Copa de 2022** (o último torneio real disputado) e **nunca é incrementada pelo resultado simulado da Final de 2026** dentro do app: o resultado da Final é uma simulação do usuário, não um título mundial real, e o app não persiste histórico de campeões entre execuções (a cada nova simulação, ou remoção/alteração do resultado, o "campeão" pode mudar). Somar +1 à seleção vencedora tornaria a contagem inconsistente ao longo de sucessivas alterações de placar e misturaria dado real (histórico verificável) com dado simulado (fictício) na mesma exibição — o painel mostra a contagem histórica real da seleção campeã, não uma contagem hipotética "após este título".

### Decisão 8 — Painel do campeão como componente reativo, condicionado ao DTO
O painel de destaque (`CampeaoDestaque.razor` ou similar) recebe como parâmetro a seleção campeã (ou `null`) já resolvida pelo serviço; a página Final decide sua visibilidade (`@if (Campeao is not null)`), sem estado próprio de UI além do parâmetro — mesmo padrão dos demais componentes de apresentação do projeto (sem lógica de negócio no componente Razor).

Dois ajustes de implementação descobertos ao validar a página no navegador:
- **Ícone da taça como SVG inline, não `bi bi-trophy-fill`**: o projeto não carrega a fonte de ícones da Bootstrap Icons — `NavMenu.razor.css` só define, como imagens de fundo SVG, os ícones específicos que o menu usa (`.bi-house-door-fill-nav-menu` etc.), não a fonte genérica `bi-*`. A classe `bi bi-trophy-fill` sozinha não renderizava nada. Corrigido embutindo o glyph oficial `trophy-fill` da Bootstrap Icons (MIT) como `<svg>` inline, mesmo princípio dos ícones já existentes no projeto.
- **Confete com uma única queda, não em loop infinito**: a primeira versão usava `animation-iteration-count: infinite`, o que também não corresponde a um efeito de confete real (que cai uma vez, não chove para sempre enquanto a página estiver aberta). Corrigido para `animation-iteration-count: 1` com `animation-fill-mode: forwards` (a peça desaparece — opacidade 0 — ao final da queda).

## Risks / Trade-offs

- [Hotlink de imagem externa (Wikimedia)] → Se o arquivo for movido/removido no Wikimedia no futuro, a foto quebra. Mitigado pelo mesmo padrão defensivo (`onerror` ocultando a imagem) já usado para bandeiras da API da FIFA, então a quebra não afeta o restante da página.
- [Tabela de títulos mundiais desatualizável automaticamente] → Fica congelada no código até a próxima Copa. Aceitável: é dado histórico, não dado do torneio 2026, e o escopo do projeto já exclui atualização automática de dados (CLAUDE.md - Fora do Escopo).
- [Dois jogos com um único registro por `Fase`] → Diferente das demais fases eliminatórias (que têm vários jogos por `Fase` distinguidos por `Ordem`), Terceiro Lugar e Final têm exatamente um jogo cada. Mitigado usando `Fase` como identificador único de cada um (`"Terceiro Lugar"`, `"Final"`), com `Ordem = 1` apenas para reaproveitar o mesmo shape de dados/consulta já usado pelas demais fases. Como consequência, `FaseEliminatoriaJogoCard` exibiria "Jogo 1" nos dois cards da página Final — mitigado pelo parâmetro `TituloJogo` (Decisão 1).
- [Extração de `CarregarGrafoEliminatorioAsync`/`ResolverSelecao`/`RotularOrigem` de `FaseEliminatoriaService` para `GrafoEliminatorioReader`] → É uma refatoração de código já existente, ainda que comportamento-preservando. Mitigado por não alterar assinatura nem lógica desses métodos (puro "move", coberto pela mesma validação manual de Fase2/Oitavas/Quartas/Semifinais listada em `tasks.md`).

## Migration Plan

- Sem migração de banco de dados (schema inalterado).
- Seed idempotente: em bancos já existentes (como o `portalcopa26.db` já versionado neste repositório), o próximo start da aplicação insere os dois novos jogos automaticamente, sem apagar nem recriar dados existentes — mesmo mecanismo já validado por `SeedQuartasSemifinaisAsync`.
- Rollback: remover o passo de seed impede novos jogos de serem criados em bancos novos; em bancos que já os receberam, bastaria uma remoção manual dos dois registros (`Fase IN ('Terceiro Lugar', 'Final')`) — não é automatizado nesta change, por não haver necessidade de rollback de dados no escopo do projeto (sem ambiente de produção).

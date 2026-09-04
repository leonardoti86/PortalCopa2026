## Context

O domínio já expõe tudo que o simulador precisa, sem exigir schema novo: `Jogo` (Fase, GrupoId/Grupo, SelecaoMandanteId/VisitanteId) para os 72 jogos da fase de grupos; `Grupo`→`Selecao` para as 4 seleções de cada grupo; `RankingFifa` (Posicao, Pontos) por Seleção; e `Simulacao`/`SimulacaoJogo` (Id, DataCriacao / SimulacaoId, JogoId, PlacarMandante, PlacarVisitante) já modelados especificamente para guardar resultados simulados (ver `domain-model` e `data-persistence`, já cobrindo "simulações sobrevivem a reinício"). Este design cobre apenas a fase de grupos (72 jogos, `Fase == "Primeira Fase"`); os 30 jogos do mata-mata não são lidos por este serviço.

Critérios oficiais de desempate (RN-01/RN-02, `fontes/copa2026_regras_negocio.txt`): pontos → saldo de gols → gols marcados → confronto direto → saldo de gols no confronto direto → Fair Play (cartões) → Ranking FIFA. Não existe, em `./fontes` nem no domínio já modelado, nenhum dado de cartões/disciplina — logo o critério de Fair Play não pode ser aplicado sem inventar dado, o que é proibido pelo projeto (CLAUDE.md - "Não gerar dados fictícios"). Ver proposal.md - Why.

## Goals / Non-Goals

**Goals:**
- Definir o contrato de serviço (`ISimuladorService`) e os DTOs que a página do simulador consome.
- Definir o algoritmo de cálculo de classificação por grupo, incluindo a cascata de critérios de desempate e o ranking dos 12 terceiros colocados (wildcards).
- Definir a decomposição dos componentes solicitados (`SimuladorGrupo`, `SimuladorJogo`, `ClassificacaoGrupo`, `SimuladorResumo`) e a responsabilidade de cada um.
- Definir o modelo de persistência automática de uma única simulação "atual", sem UI de nomeação/gerenciamento de múltiplas simulações.

**Non-Goals:**
- Simular o mata-mata (oitavas em diante) — apenas a fase de grupos, conforme proposal.md.
- Implementar o critério de desempate "Fair Play (cartões)" — não há dado oficial disponível; o desempate pula esse critério e vai direto ao Ranking FIFA.
- Gerenciar múltiplas simulações nomeadas/salvas pelo usuário — existe sempre uma única simulação "atual".
- Alterar o schema do banco, as Migrations ou o Seed Data — as entidades `Simulacao`/`SimulacaoJogo` já existem para esse fim.
- Corrigir o texto do CTA em `SimuladorPainel.razor` (menciona mata-mata) — fora do escopo desta change.

## Decisions

### Decisão 1: Uma única simulação "atual", obtida via get-or-create
`ISimuladorService.ObterEstadoAtualAsync()` busca a `Simulacao` mais antiga existente (`OrderBy(s => s.DataCriacao).FirstOrDefault()`); se nenhuma existir, cria uma nova (sem nome, apenas `DataCriacao = DateTime.Now`) e a persiste antes de retornar. Toda a aplicação opera sempre sobre essa única simulação — não há seleção nem criação de simulações adicionais pela UI.
- **Alternativa considerada**: permitir múltiplas simulações nomeadas, com uma tela de gerenciamento. Rejeitada porque o pedido é explícito ("Não exigir nome da simulação", "a simulação atual") e nenhum componente de listagem/gerenciamento foi solicitado.

### Decisão 2: Placar é persistido imediatamente a cada alteração (sem botão salvar)
`ISimuladorService.AtualizarPlacarAsync(int jogoId, int? placarMandante, int? placarVisitante)` faz upsert do `SimulacaoJogo` correspondente (busca por `SimulacaoId` + `JogoId`; cria se não existir) e chama `SaveChangesAsync` antes de retornar. Quando ambos os placares vêm `null` (usuário limpou o campo), o registro `SimulacaoJogo` é removido, voltando o jogo ao estado "não simulado" (consistente com o Requirement "Classificação parcial"). O método retorna a classificação já recalculada, evitando um round-trip extra para o cliente.
- **Alternativa considerada**: manter o placar em memória no componente e só persistir com um botão "Salvar". Rejeitada — contraria o requisito explícito de persistência automática a cada alteração.

### Decisão 3: Classificação é sempre recalculada no servidor a partir dos dados brutos (sem coluna de posição armazenada)
Nenhuma tabela nova guarda "classificação" ou "posição" — `SimuladorService` recalcula a classificação de cada grupo (e o ranking de terceiros) a partir dos `SimulacaoJogo` existentes toda vez que é solicitada (carga da página) ou após uma alteração de placar (Decisão 2). Com 72 jogos no total, o custo é desprezível.
- **Alternativa considerada**: persistir a classificação calculada em uma tabela própria, atualizada incrementalmente. Rejeitada por complexidade desnecessária dado o volume de dados; recalcular do zero é simples, sempre correto e elimina risco de estado de classificação desatualizado.

### Decisão 4: Algoritmo de desempate — cascata com sub-ranking de confronto direto
Para cada grupo (sempre 4 seleções, round-robin de 3 jogos por seleção):
1. Calcular estatísticas base (J, V, E, D, GP, GC, SG, Pts) usando somente os `SimulacaoJogo` daquele grupo que têm ambos os placares preenchidos.
2. Ordenar por Pts desc. Dentro de cada grupo de seleções empatadas em Pts, ordenar por SG desc, depois GP desc.
3. Para cada subgrupo que permanecer empatado após o passo 2 (2, 3 ou 4 seleções): calcular uma "mini-tabela" usando apenas os jogos simulados entre as seleções desse subgrupo (confronto direto) — mini-Pts e, se ainda empatado, mini-SG — e ordenar por esses valores.
4. Se o empate persistir após o passo 3 (confronto direto também empatado): pular Fair Play (Decisão de escopo) e desempatar pela posição no Ranking FIFA (menor posição primeiro).
5. Atribuir posição 1–4 na ordem final.

Ranking dos 12 terceiros colocados (wildcards): mesma cascata, mas usando apenas Pts → SG → GP → Ranking FIFA (sem os passos 3/4 de confronto direto, pois seleções de grupos diferentes não jogaram entre si); os 8 primeiros do ranking geral são marcados como wildcard.
- **Alternativa considerada**: aplicar apenas Pts/SG/GP e pular direto para Ranking FIFA, ignorando confronto direto. Rejeitada — confronto direto é um critério oficial explícito (RN-01) e computável com os dados existentes (diferente de Fair Play).

**Fallback para seleção sem Ranking FIFA cadastrado**: 7 das 48 seleções não têm `RankingFifa` no Seed Data — `Data/SeedJson/teams.json` traz `"ranking": null` para Arábia Saudita, Bósnia e Herzegovina, Cabo Verde, Curaçao, Gana, Nova Zelândia e Uzbequistão, e `SeedData.cs` só cria o registro `RankingFifa` quando esse valor não é nulo (`Selecao.RankingFifa` fica `null` para essas 7). Isso atinge 6 dos 12 grupos (B, E, G, H — com duas seleções afetadas —, K, L) e também pode atingir o ranking de terceiros (Decisão 4). Como Ranking FIFA é o último critério oficial aplicado tanto na cascata de grupo (passo 4) quanto no ranking de terceiros, uma seleção sem `RankingFifa` SHALL ser tratada como a pior posição possível de forma determinística nesse critério (posição efetiva = `int.MaxValue`, sempre pior que qualquer posição real). Se o empate persistir mesmo assim — ambas as seleções empatadas sem `RankingFifa`, ou com a mesma posição efetiva —, o desempate final SHALL usar `Selecao.Codigo` em ordem alfabética, garantindo que a ordenação nunca fique indefinida nem lance exceção.
- **Alternativa considerada**: tratar a ausência de `RankingFifa` como erro/exceção e impedir o cálculo da classificação do grupo afetado. Rejeitada — inviabilizaria a classificação de 6 dos 12 grupos, que é o núcleo da funcionalidade; um fallback determinístico e documentado é preferível a bloquear a página.
- **Alternativa considerada**: pular Ranking FIFA e ir direto para `Selecao.Codigo` sempre que qualquer uma das seleções tiver ranking ausente. Rejeitada — descartaria o Ranking FIFA como critério mesmo quando ele poderia decidir entre uma seleção com ranking e outra sem (a seleção sem ranking já perde por definição nesse critério, não precisa de tratamento especial adicional).

### Decisão 5: Componentes e responsabilidades
- **`Simulador.razor`** (`@page "/simulador"`): página. Injeta `ISimuladorService`, carrega o estado atual em `OnInitializedAsync` (jogos por grupo + classificações dos 12 grupos + ranking de terceiros), mantém o grupo ativo (aba) selecionado, repassa a alteração de placar ao serviço e atualiza o estado local com o retorno.
- **`SimuladorGrupo.razor`**: representa um grupo completo — renderiza a lista de `SimuladorJogo` daquele grupo e a `ClassificacaoGrupo` correspondente lado a lado (responsivo: empilha em telas estreitas).
- **`SimuladorJogo.razor`**: uma linha/cartão por jogo, com os dois campos numéricos de placar (mandante/visitante) e as seleções envolvidas; expõe `EventCallback` de alteração de placar para o componente pai.
- **`ClassificacaoGrupo.razor`**: tabela Bootstrap responsiva (`table-responsive`) com posição, seleção, J/V/E/D/GP/GC/SG/Pts; aplica destaque visual (classe CSS) nas linhas de 1º/2º lugar (classificados) e condicionalmente na linha de 3º lugar quando `Wildcard == true`.
- **`SimuladorResumo.razor`**: painel cross-group — exibe o progresso da simulação (quantos dos 72 jogos já têm placar informado) e o ranking geral dos 12 terceiros colocados, destacando os 8 primeiros como wildcard (é o único lugar onde os terceiros de todos os grupos aparecem lado a lado, já que o destaque de wildcard depende da comparação entre grupos).

### Decisão 6: DTOs próprios da capability `simulador`
Novos DTOs em `Services/Simulador/Dtos/` (`SimuladorJogoDto`, `ClassificacaoTimeDto`, `ClassificacaoGrupoDto`, `SimuladorEstadoDto`), sem reaproveitar tipos de `Services/Jogos` ou `Services/LandingPage` — mesma justificativa da change `criar-jogos-grupos` (isolamento entre capabilities definido em CLAUDE.md - Organização Funcional).

## Risks / Trade-offs

- [Critério de Fair Play (cartões) não implementado] → Mitigação: documentado explicitamente na spec e no proposal; o desempate pula esse critério e vai direto ao Ranking FIFA.
- [7 das 48 seleções não têm `RankingFifa` cadastrado no Seed Data, afetando 6 dos 12 grupos e potencialmente o ranking de terceiros] → Mitigação: Decisão 4 define um fallback determinístico — seleção sem Ranking FIFA é tratada como pior posição possível nesse critério, com desempate final por `Selecao.Codigo` (ordem alfabética) caso o empate persista.
- [Terceira duplicação do literal `"Primeira Fase"` como filtro de fase — já presente em `LandingPageService` e `JogosService`] → Mitigação: aceito conscientemente para manter esta change isolada (mesmo trade-off documentado em `criar-jogos-grupos`); uma futura change de limpeza pode extrair esse valor para um local compartilhado caso a duplicação continue crescendo.
- [Recalcular a classificação inteira a cada alteração de placar] → Mitigação: volume fixo e pequeno (72 jogos, 12 grupos de 4 times), custo desprezível; evita manter estado de classificação derivado potencialmente desatualizado.
- [Algoritmo de confronto direto com sub-ranking recursivo pode ser mal implementado se tratado como caso especial só de 2 times] → Mitigação: Decisão 4 descreve o algoritmo genérico por subgrupo (2, 3 ou 4 times), não apenas o caso de 2 times.

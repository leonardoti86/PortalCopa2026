## Context

O `RankingFifa` já existe como entidade 1:1 com `Selecao` (FK `SelecaoId` obrigatória — ver `Models/Copa/RankingFifa.cs` e `Data/AppDbContext.cs`) e já é populado por `SeedData.cs` a partir do campo `Ranking` de cada item de `Data/SeedJson/teams.json`, que por sua vez foi gerado a partir de `fontes/copa2026_ranking_fifa.txt`. Isso significa que só existe (e só pode existir, sem alterar o schema) um `RankingFifa` para uma seleção que também seja uma `Selecao` da Copa 2026 — hoje 41 das 48 seleções.

A fonte oficial `fontes/copa2026_ranking_fifa.txt` lista 98 seleções do ranking mundial (não apenas as 48 da Copa), mas contém apenas posição, nome e pontuação — sem o código FIFA de 3 letras usado por toda a aplicação para montar a URL da bandeira (`https://api.fifa.com/api/v3/picture/flags-sq-4/{Codigo}`). Não há, em nenhuma fonte oficial da pasta `fontes/`, o código das ~57 seleções do ranking mundial que não disputam a Copa 2026.

Existe também `Data/SeedJson/ranking.json`, um dump com as ~98 posições do ranking mundial, mas nenhum código em `SeedData.cs` o lê — é resíduo do pipeline do protótipo estático, não uma fonte usada pela aplicação Blazor.

A Landing Page (`Components/Pages/LandingPage/RankingFifaChart.razor`) já linka para `/ranking` com o texto "Ver ranking completo →", ou seja, a rota já é esperada pelo restante do sistema.

O usuário pediu como referência visual `prototipo/ranking.html`, mas esse arquivo não existe: o protótipo é uma SPA (`prototipo/index.html` + `prototipo/js/pages/ranking.js` + `prototipo/data/ranking.js`). A view de ranking do protótipo exibe as 98 seleções do ranking mundial, destacando as participantes da Copa com uma badge de grupo e listando à parte as participantes sem posição no ranking — mas não tem busca nem destaque de Top 3 (esses dois pontos vieram apenas do pedido do usuário, não do protótipo).

## Goals / Non-Goals

**Goals:**
- Servir a rota `/ranking` já referenciada pela Landing Page.
- Reaproveitar a entidade `RankingFifa`/`Selecao` e o seed já existentes, sem alterar schema.
- Reaproveitar o componente de busca (`FiltroSelecao`) e a paleta de cores já definida (`landing-page.css`) em vez de criar novos padrões visuais.
- Tratar de forma visível e honesta as 7 seleções da Copa 2026 sem posição na fonte de ranking, sem inventar dado.

**Non-Goals:**
- Exibir o ranking mundial completo (98 seleções). Faria a página mostrar seleções sem bandeira/código oficial disponível em nenhuma fonte do projeto, o que violaria a diretriz do CLAUDE.md de não inventar dados. Se o ranking mundial completo for desejado no futuro, é uma decisão de produto separada (ex.: buscar os códigos FIFA das seleções restantes) e deve virar uma nova change.
- Adicionar gráfico (Chart.js) à página de Ranking. O usuário não pediu gráfico aqui — o gráfico do Top 10 já existe na Landing Page e reaproveita o componente `ChartJs` caso um gráfico seja pedido futuramente.
- Alterar `Data/SeedJson/ranking.json` ou o pipeline de seed.

## Decisions

### Escopo de dados: 48 seleções da Copa, não as 98 do ranking mundial
A página consulta `context.RankingsFifa.Include(r => r.Selecao).ThenInclude(s => s.Grupo)`, que naturalmente já se limita às seleções da Copa 2026 (única fonte de `RankingFifa` hoje). As 7 seleções da Copa sem `RankingFifa` (`Arábia Saudita`, `Bósnia e Herzegovina`, `Cabo Verde`, `Curaçao`, `Gana`, `Nova Zelândia`, `Uzbequistão`) são obtidas via `context.Selecoes.Where(s => s.RankingFifa == null)` e exibidas em uma seção separada ("sem posição no ranking"), replicando a mesma ideia já usada no protótipo, sem inventar posição/pontuação.

**Alternativa considerada**: importar as ~57 seleções restantes do ranking mundial a partir de `fontes/copa2026_ranking_fifa.txt`, tornando `RankingFifa.SelecaoId` opcional e adicionando um campo `NomeSelecaoExterna` para as que não têm `Selecao`. Rejeitada porque exigiria exibir bandeiras/códigos que não existem em nenhuma fonte oficial do projeto — inventar o código FIFA de uma seleção violaria a regra do CLAUDE.md ("Não gerar dados fictícios").

### Novo serviço `RankingService`
Segue o padrão já estabelecido (`GruposService`, `SelecaoService`, `JogosService`): classe com construtor primário recebendo `AppDbContext`, DTOs como `record` em `Services/Ranking/Dtos/`, registrado como `Scoped` em `Program.cs`. `docs/EstruturaDados.md` já antecipa esse nome (`RankingService`) na lista de serviços por capacidade.

```csharp
public interface IRankingService
{
    Task<RankingEstadoDto> ObterRankingAsync(CancellationToken cancellationToken = default);
}

public record RankingPosicaoDto(int Posicao, string SelecaoNome, string SelecaoCodigo, double Pontos, string Grupo);
public record SelecaoSemRankingDto(string SelecaoNome, string SelecaoCodigo, string Grupo);
public record RankingEstadoDto(IReadOnlyList<RankingPosicaoDto> Posicoes, IReadOnlyList<SelecaoSemRankingDto> SemPosicao);
```

Este DTO é local à capacidade `ranking` e não substitui o `RankingItemDto` já existente em `Services/LandingPage/Dtos` (que serve apenas o Top 10 da Landing Page e tem uma forma um pouco diferente). Foi nomeado `RankingPosicaoDto` (não `RankingItemDto`) porque `Components/_Imports.razor` já importa globalmente todos os namespaces `Services.*.Dtos` do projeto: um segundo tipo chamado `RankingItemDto` em `Services.Ranking.Dtos` tornaria a referência ambígua (`error CS0104`) em **todo** arquivo `.razor` do projeto, não só nos novos — namespaces diferentes não resolvem colisão de nome sob usings globais em Blazor. Confirmado na prática: a primeira tentativa com o nome `RankingItemDto` quebrou a build em `Home.razor` e `RankingFifaChart.razor`, que nem usam o novo tipo.

### Reaproveitar `FiltroSelecao` para busca e filtro por grupo
`Components/Pages/Selecoes/FiltroSelecao.razor` já implementa busca por nome + filtro por grupo de forma genérica (recebe `Grupos`, `Busca`, `GrupoSelecionado` como parâmetros, sem depender de `SelecaoResumoDto`). A página de Ranking reaproveita esse componente diretamente, filtrando a lista em memória no `@code` da página (mesmo padrão de `Selecoes.razor`), em vez de criar um novo componente de busca. O filtro por grupo fica no escopo desta change porque vem "de graça" ao reaproveitar o componente e é coberto por requisito próprio em `specs/ranking/spec.md`.

### Busca e filtro afetam só a tabela principal; Top 3 e "sem posição" ficam fixos
A busca por nome e o filtro por grupo (`FiltroSelecao`) só filtram `RankingTabela` (posições a partir da 4ª). `RankingTop3` sempre exibe as 3 primeiras posições do ranking completo, e a seção de seleções sem posição sempre exibe a lista completa — nenhum dos dois reage a `Busca`/`GrupoSelecionado`. Alternativa considerada: também filtrar o Top 3 e a seção "sem posição" pelo mesmo termo, fazendo-os sumir quando não há correspondência. Rejeitada porque um pódio que aparece e desaparece conforme o usuário digita é uma experiência confusa para um destaque editorial fixo, e a seção "sem posição" existe justamente para ser um inventário completo e estável, não um resultado de busca. Consequência direta em `Ranking.razor`: `RankingTop3` e a seção "sem posição" recebem os dados brutos de `RankingEstadoDto`, enquanto `RankingTabela` recebe a lista já filtrada por `_busca`/`_grupoSelecionado`.

### Destaque do Top 3 com fundo escuro
Novo componente `RankingTop3.razor`, estilizado em `wwwroot/css/ranking.css` reaproveitando os tokens de cor já definidos em `landing-page.css` (`--lp-color-navy`, `--lp-color-primary-dark`, `--lp-color-primary`, `--lp-color-accent`) no mesmo gradiente escuro usado em `.lp-hero`, com texto em branco/`--lp-color-accent` para o 1º lugar. Evita introduzir uma nova paleta de cores para a mesma ideia de "destaque escuro" que a Landing Page já usa.

### Estrutura de arquivos
```
Services/Ranking/
  IRankingService.cs
  RankingService.cs
  Dtos/RankingPosicaoDto.cs
  Dtos/SelecaoSemRankingDto.cs
  Dtos/RankingEstadoDto.cs
Components/Pages/Ranking/
  Ranking.razor            (@page "/ranking")
  RankingTop3.razor         (destaque das 3 primeiras posições)
  RankingTabela.razor       (tabela do restante do ranking)
wwwroot/css/ranking.css
```

## Risks / Trade-offs

- [Usuário pode esperar ver todo o ranking mundial, não só as seleções da Copa] → Mitigação: a página deixa claro no cabeçalho que lista o Ranking FIFA das seleções da Copa 2026, e a seção "sem posição no ranking" evita que pareça que dados foram omitidos por engano.
- [Dois DTOs de ranking coexistindo no projeto (`Services/LandingPage/Dtos/RankingItemDto` e `Services/Ranking/Dtos/RankingPosicaoDto`) pode confundir] → Mitigação: nomes diferentes (`RankingPosicaoDto`, não `RankingItemDto`) evitam ambiguidade de compilação sob os usings globais de `_Imports.razor`, e o namespace de cada um (`Services.LandingPage.Dtos` vs `Services.Ranking.Dtos`) já deixa a capacidade de origem explícita.

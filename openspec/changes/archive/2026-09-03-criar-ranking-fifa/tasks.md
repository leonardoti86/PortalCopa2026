## 1. Serviço de Ranking

- [x] 1.1 Criar `Services/Ranking/Dtos/RankingPosicaoDto.cs` (renomeado de `RankingItemDto` durante a implementação para evitar ambiguidade de compilação com `Services/LandingPage/Dtos/RankingItemDto` sob os usings globais de `_Imports.razor` — ver design.md), `SelecaoSemRankingDto.cs` e `RankingEstadoDto.cs` (records)
- [x] 1.2 Criar `Services/Ranking/IRankingService.cs` com `ObterRankingAsync`
- [x] 1.3 Implementar `Services/Ranking/RankingService.cs`: consultar `RankingsFifa` com `Include(Selecao).ThenInclude(Grupo)`, ordenar por `Posicao`, e montar a lista de seleções sem `RankingFifa` via `Selecoes.Where(s => s.RankingFifa == null)`, ordenadas por nome
- [x] 1.4 Registrar `IRankingService`/`RankingService` como `Scoped` em `Program.cs`

## 2. Componentes da Página de Ranking

- [x] 2.1 Criar `Components/Pages/Ranking/RankingTop3.razor`: recebe sempre as 3 primeiras posições do ranking completo (dado bruto, não filtrado) e renderiza o destaque com fundo escuro/texto claro
- [x] 2.2 Criar `Components/Pages/Ranking/RankingTabela.razor`: recebe a lista a partir da 4ª posição, já filtrada por busca/grupo, e renderiza a tabela com posição, bandeira, nome, pontuação e grupo
- [x] 2.3 Criar `Components/Pages/Ranking/Ranking.razor` com `@page "/ranking"`, `@rendermode InteractiveServer`, injetando `IRankingService`
- [x] 2.4 Na página, reaproveitar `Components/Pages/Selecoes/FiltroSelecao.razor` para busca por nome + filtro por grupo, aplicando o filtro em memória apenas sobre a lista passada a `RankingTabela` (posições 4+), seguindo o padrão de filtragem de `Selecoes.razor`. `RankingTop3` e a seção de "sem posição" recebem sempre os dados completos, não filtrados
- [x] 2.5 Exibir mensagem de "nenhuma seleção encontrada" quando a busca/filtro não retornar resultados na tabela principal, mantendo o Top 3 e a seção "sem posição" inalterados
- [x] 2.6 Exibir a seção separada das seleções sem posição no ranking (`SemPosicao`), sempre completa (não filtrada por busca/grupo), com texto indicando que a fonte oficial não traz posição para elas
- [x] 2.7 Aplicar `loading="lazy"` e `onerror="this.style.visibility='hidden'"` nas bandeiras, seguindo o padrão já usado em `CartaoSelecao.razor`/`RankingFifaChart.razor`

## 3. Estilo

- [x] 3.1 Criar `wwwroot/css/ranking.css` reaproveitando os tokens de cor de `landing-page.css` (`--lp-color-navy`, `--lp-color-primary-dark`, `--lp-color-primary`, `--lp-color-accent`) para o destaque do Top 3 com fundo escuro e texto claro
- [x] 3.2 Referenciar `ranking.css` no layout/host page (mesmo padrão de `grupos.css`, `selecoes.css`, `simulador.css`)

## 4. Navegação

- [x] 4.1 Adicionar item "Ranking" em `Components/Layout/NavMenu.razor`, apontando para `/ranking`
- [x] 4.2 Confirmar que o link "Ver ranking completo →" de `RankingFifaChart.razor` (Landing Page) navega corretamente para a nova página

## 5. Verificação

- [x] 5.1 Rodar a aplicação (`dotnet run`) e validar visualmente: Top 3 destacado, tabela ordenada por posição, grupo exibido, bandeiras carregando
- [x] 5.2 Validar a busca por nome (com e sem resultado) e o filtro por grupo, confirmando que ambos afetam apenas a tabela principal e que o Top 3 e a seção "sem posição" permanecem inalterados durante a busca/filtro
- [x] 5.3 Validar que as 7 seleções sem posição no ranking (Arábia Saudita, Bósnia e Herzegovina, Cabo Verde, Curaçao, Gana, Nova Zelândia, Uzbequistão) aparecem na seção separada e não na tabela principal
- [x] 5.4 Conferir que nenhuma seleção fora da Copa 2026 nem nenhum código FIFA inventado aparece na página

## Context

O domínio (`Selecao`, `Jogador`, `RankingFifa`, `Grupo`) e o seed (`Data/SeedData.cs` + `Data/SeedJson/teams.json`/`players.json`, já derivados das fontes oficiais `copa2026_selecoes_jogadores.txt` e `copa2026_pais_tecnicos.txt`) já existem e não serão alterados por esta change. Falta apenas a camada de apresentação: serviço de leitura e página/componentes.

O protótipo de referência (`../prototipo`) não tem um arquivo `equipes.html` isolado — é uma SPA (`index.html` + roteador em `js/router.js`) cuja página "Equipes" está implementada em `js/pages/equipes.js`, com estilos em `css/styles.css`. É essa página (rota `#/equipes`) que serve de referência visual/funcional. No protótipo, clicar em uma seleção navega para uma página de detalhe (não um modal); esta change diverge deliberadamente nesse ponto porque o pedido explícito do usuário é abrir uma **janela modal** com bandeira, nome, grupo, técnico, ranking FIFA e elenco — reaproveitando o visual da seção de detalhe do protótipo, mas em modal.

**Escopo reduzido em relação ao detalhe do protótipo**: a página de detalhe original (`equipes.js`) também exibe uma seção "Outras seleções do Grupo X" (links rápidos para os demais times do grupo) e um filtro por posição de jogador com contadores. Esta change deixa deliberadamente esses dois elementos fora de escopo — o modal exibe apenas bandeira, nome, grupo, técnico, ranking FIFA e o elenco completo (sem filtro por posição), conforme os Requirements em `specs/selecoes/spec.md`.

O projeto Blazor já reaproveita a paleta de cores do protótipo em `wwwroot/css/landing-page.css` (`--lp-color-primary`, `--lp-color-navy`, `--lp-color-border`, `--lp-color-text-muted`, etc.), consumida por `grupos.css`, `jogos.css` e `simulador.css`. O novo `selecoes.css` deve seguir o mesmo padrão.

**Divergência de dados vs. critério de aceitação**: a fonte oficial `copa2026_selecoes_jogadores.txt` (refletida em `players.json`) não tem exatamente 26 jogadores para todas as seleções — a contagem varia entre 22 e 26 por seleção. Por diretriz do CLAUDE.md ("não inventar dados", "utilizar exclusivamente os dados da pasta ./fontes"), o elenco exibido SHALL refletir fielmente a fonte (ver `specs/selecoes/spec.md` - Requirement "Elenco de Jogadores no Modal"), mesmo quando isso resultar em menos de 26 jogadores para uma seleção específica.

## Goals / Non-Goals

**Goals:**
- Página `/selecoes` com listagem, busca e filtro por grupo, reaproveitando Bootstrap 5.
- Modal Bootstrap com detalhe da seleção e elenco completo.
- Acesso a dados exclusivamente via `ISelecaoService` (sem `DbContext` em páginas/componentes).
- Visual alinhado ao protótipo (grid de cartões, cores, tipografia).

**Non-Goals:**
- Alterar o modelo de dados, o seed ou a lógica de outras capacidades (Grupos, Jogos, Ranking, Simulador).
- Edição/atualização de dados de seleção ou jogador (somente leitura).
- Paginação da listagem (48 itens é um volume pequeno o suficiente para renderizar tudo de uma vez, como no protótipo).

## Decisions

**1. Uma única consulta agregada no `SelecaoService`, com filtragem/busca no cliente (Blazor Server, em memória).**
O protótipo já usa esse padrão (carrega todas as 48 seleções e filtra em JS). Como o volume é fixo e pequeno (48 seleções, ~1.237 jogadores), buscar todas as seleções (com grupo e ranking) uma vez em `OnInitializedAsync` e filtrar em C# no componente é mais simples que refazer a query a cada tecla digitada, e é consistente com o padrão de `GruposService.ObterEstadoAtualAsync()` (carrega o estado completo, filtra na página). Os jogadores da seleção selecionada são carregados sob demanda ao abrir o modal (`ObterDetalheAsync(selecaoId)`), evitando trazer ~1.237 jogadores na carga inicial da página.

**2. Modal com marcação/classes do Bootstrap 5, mas abertura/fechamento controlados inteiramente por estado do Blazor (sem `data-bs-toggle`/`data-bs-target` e sem JSInterop).**
Tentativa inicial: `data-bs-toggle="modal"`/`data-bs-target="#modalSelecao"` no `CartaoSelecao`, deixando o próprio JS do Bootstrap abrir o modal. Descartada após teste manual no navegador: o clique não abria o modal porque o `@onclick` do Blazor Server (`InteractiveServer`) intercepta o evento antes do listener delegado do Bootstrap (confirmado via DevTools — chamar `new bootstrap.Modal(el).show()` manualmente funciona, mas o clique no botão com `data-bs-toggle` não dispara nada). Alternativa de JSInterop manual (chamar `bootstrap.Modal.show()`/`hide()` a partir do C#) também foi descartada, por CLAUDE.md ("Utilizar JSInterop apenas quando necessário"). Em vez disso, `ModalSelecao.razor` aplica as classes/estrutura visuais do Bootstrap (`modal`, `modal-dialog`, `modal-content`, `modal-backdrop`) mas alterna `show`/`display:block` apenas com base no parâmetro `SelecaoDetalheDto? Detalhe` vindo do Blazor; fechar (botão "×" ou clique no backdrop) dispara o `EventCallback Fechada`, que zera o estado no componente pai. O script do Bootstrap (`lib/bootstrap/dist/js/bootstrap.bundle.min.js`) continua carregado em `App.razor` pois nada mais na página depende dele hoje, mas não é mais estritamente necessário para este modal.

**3. URL da bandeira construída a partir do `Codigo` da seleção (`https://api.fifa.com/api/v3/picture/flags-sq-4/{Codigo}`), sem persistir a URL no banco.**
Mesmo padrão do protótipo (`teams.json` guarda `flagUrl`, mas é derivável do código). Persistir a URL duplicaria dado derivado; um helper (`FlagUrl` calculada no DTO ou em um `static` helper) evita duplicação e segue CLAUDE.md ("bandeiras podem utilizar o padrão da API pública da FIFA").

**4. Estrutura de componentes alinhada ao CLAUDE.md e ao precedente já estabelecido no código (`Components/Pages/Grupos`, `Components/Pages/Jogos`), sob `Components/Pages/Selecoes`.**
O CLAUDE.md lista explicitamente `Components/Pages/Selecoes` como diretório da capacidade, e o código já existente coloca os componentes reutilizáveis de cada capacidade dentro da própria pasta da página (`Components/Pages/Grupos/GrupoBloco.razor`, `Components/Pages/Jogos/JogoCard.razor`), não em uma pasta irmã separada. Por isso, tanto a página quanto os componentes reutilizáveis desta capacidade ficam em `Components/Pages/Selecoes/`: `Selecoes.razor` (página, `@page "/selecoes"`), `CartaoSelecao.razor` (cartão da listagem), `FiltroSelecao.razor` (busca + select de grupo), `ModalSelecao.razor` (modal de detalhe), `TabelaElencoSelecao.razor` (lista de jogadores dentro do modal). Não há mais uma pasta `Components/Times` separada.

**5. DTOs próprios em `Services/Selecoes/Dtos`** (`SelecaoResumoDto`, `SelecaoDetalheDto`, `JogadorDto`), em vez de expor as entidades EF Core diretamente às páginas — mesmo padrão de `Services/Grupos/Dtos`.

**6. Elenco de jogadores no modal ordenado alfabeticamente por nome, sem filtro por posição.**
O protótipo permite filtrar o elenco por posição, mas essa funcionalidade foi deixada fora de escopo (ver Context, "Escopo reduzido em relação ao detalhe do protótipo"). Para evitar ambiguidade de implementação, a ordem de exibição é definida como alfabética por nome do jogador — ordenação estável e previsível, sem depender da ordem de linhas da fonte oficial.

## Risks / Trade-offs

- [Nem toda seleção tem exatamente 26 jogadores na fonte oficial, divergindo do critério de aceitação literal do pedido] → Documentado como comportamento esperado no spec (exibir exatamente os dados da fonte); reportar a divergência ao usuário na conclusão da change.
- [Carregar as 48 seleções de uma vez em memória no componente pode ficar defasado se o usuário mantiver a aba aberta por muito tempo em uma futura tela com dados mutáveis] → Não é um risco real aqui porque seleção/técnico/ranking são dados estáticos de seed nesta versão (fora de escopo: atualização automática de resultados).
- [Nome de rota `/selecoes` vs. nome de arquivo da página] → Resolvido: a página é `Components/Pages/Selecoes/Selecoes.razor` com `@page "/selecoes"`, eliminando a divergência de nomes entre rota, spec (`specs/selecoes/`) e arquivos de componente que existia com `Equipes.razor`/`Components/Times`.

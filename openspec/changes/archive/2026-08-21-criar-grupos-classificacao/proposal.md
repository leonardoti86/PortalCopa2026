## Why

O protótipo `grupos.html` define a experiência esperada para a página de Grupos (abas por grupo, legenda, classificação e jogos), mas hoje o Portal não tem essa página nem um lugar para registrar os resultados oficiais da fase de grupos — a tabela `Jogos` não armazena placar. Sem isso, não existe uma classificação oficial calculada a partir de resultados reais, apenas a classificação hipotética do Simulador.

## What Changes

- Adicionar a página `/grupos`, fiel ao protótipo `grupos.html`: navegação por abas para os 12 grupos, legenda de classificação, tabela de classificação do grupo selecionado, lista dos jogos do grupo selecionado e botão "Simular" linkando para o Simulador.
- **BREAKING**: adicionar os campos `PlacarMandante` e `PlacarVisitante` (nullable) à entidade `Jogo` e gerar a migração EF Core correspondente, para persistir o resultado oficial diretamente no jogo (distinto do placar simulado, que continua em `SimulacaoJogo`).
- Criar `GruposService` (`IGruposService`) responsável por: montar a classificação oficial de cada grupo a partir dos jogos com placar preenchido, calcular estatísticas por seleção (J/V/E/D/GP/GC/SG/Pts) e aplicar a cascata de desempate oficial (RN-01), e registrar/atualizar o placar oficial de um jogo da fase de grupos.
- Extrair a lógica de cálculo de estatísticas e desempate hoje embutida em `SimuladorService` para um componente compartilhado, reutilizado tanto pelo `GruposService` (resultados oficiais) quanto pelo `SimuladorService` (resultados simulados), evitando duplicar a cascata de desempate.
- Extrair também a constante `"Primeira Fase"` e a consulta de jogos da fase de grupos hoje duplicadas em `JogosService` e `SimuladorService` para esse mesmo local compartilhado, evitando que `GruposService` crie uma terceira cópia.
- Permitir registrar/atualizar o placar oficial de um jogo diretamente na página de Grupos; ao salvar, o resultado é persistido em `Jogos`, a classificação do grupo e as estatísticas das seleções envolvidas são recalculadas e a interface é atualizada imediatamente, sem afetar as simulações existentes.
- Adicionar link "Grupos" ao menu de navegação.

## Capabilities

### New Capabilities
- `grupos`: página de Grupos com abas, classificação oficial calculada a partir dos resultados registrados, lista de jogos por grupo e registro/edição do placar oficial dos jogos da fase de grupos.

### Modified Capabilities
- `domain-model`: a entidade `Jogo` passa a incluir o placar oficial (`PlacarMandante`, `PlacarVisitante`), nulo até o resultado ser registrado.
- `data-persistence`: os resultados oficiais registrados na página de Grupos SHALL sobreviver a reinícios da aplicação, da mesma forma já garantida para as simulações.

## Impact

- **Models**: `Models/Copa/Jogo.cs` (novos campos de placar); nova migração EF Core em `Migrations/`.
- **Services**: novo `Services/Grupos/` (`IGruposService`, `GruposService`, DTOs — o DTO de jogo do grupo inclui `Data`/`Horario`, diferente de `SimuladorJogoDto`); extração da lógica de estatísticas/desempate e da consulta/constante de jogos da fase de grupos hoje duplicadas em `Services/Jogos/JogosService.cs` e `Services/Simulador/SimuladorService.cs` para um local compartilhado (ex.: `Services/Copa/` ou similar), consumido por `JogosService`, `SimuladorService` e `GruposService`.
- **Components**: novo `Components/Pages/Grupos/` (página `Grupos.razor` + subcomponentes de abas, tabela de classificação e lista/edição de jogos), reaproveitando o padrão visual e de interação já usado em `Components/Pages/Simulador/`.
- **Estilo**: novo `wwwroot/css/grupos.css`, seguindo o padrão já usado por `landing-page.css`/`jogos.css`/`simulador.css`, registrado como `<link>` em `Components/App.razor`.
- **Navegação**: `Components/Layout/NavMenu.razor` ganha o link para `/grupos` — ajuste mínimo pontual; nenhuma página existente (Jogos, Simulador, LandingPage) está linkada nesse menu hoje, e um redesenho completo da navegação global fiel ao protótipo fica fora do escopo desta change (ver design.md - Non-Goals).
- **Banco de dados**: `portalcopa26.db` recebe as novas colunas de placar em `Jogos` via migração; nenhuma tabela de simulação é afetada.
- **Fora do escopo**: exibir placar oficial na página de Jogos; critério de desempate "Fair Play (cartões)" (sem dados oficiais disponíveis, mesmo tratamento já adotado no Simulador) e "sorteio" (sem mecanismo real, resolvido por Ranking FIFA + código alfabético como já feito no Simulador); pré-seleção automática do grupo na página de Simulador ao clicar em "Simular" (o botão navega para `/simulador`, sem alterar a capacidade `simulador` para interpretar o grupo de origem); redesenho da navegação global do site.

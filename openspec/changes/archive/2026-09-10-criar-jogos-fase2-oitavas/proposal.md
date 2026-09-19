## Why

O PortalCopa26 hoje só apresenta a Fase de Grupos (Jogos e Grupos). A Copa do Mundo 2026 segue com fases eliminatórias — Segunda Fase e Oitavas de Final são as duas primeiras — e o modelo de dados atual (`Jogo`) não tem como representar um confronto cujas seleções ainda não são conhecidas (dependem do vencedor de outro jogo). Sem isso, não é possível carregar os 16 jogos oficiais da Segunda Fase nem os 8 jogos das Oitavas, nem propagar automaticamente os classificados.

## What Changes

- Adicionar ao modelo `Jogo` a capacidade de referenciar dois jogos de origem (`JogoOrigemMandanteId`/`JogoOrigemVisitanteId`) para quando a seleção de um lado só é conhecida após o resultado de outro jogo, mais os placares de pênaltis (`PlacarPenaltisMandante`/`PlacarPenaltisVisitante`) usados para desempatar um confronto eliminatório sem vencedor no tempo normal.
- Criar `SeedJogosSegundaFase(...)` e `SeedJogosOitavas(...)` em `SeedData`, carregando exclusivamente os dados de `.\fontes\copa2026_jogos_segunda_fase.txt` (16 jogos, seleções já definidas) e `.\fontes\copa2026_jogos_oitavas.txt` (8 jogos, sem seleções fixas — apenas referência ao jogo de origem da Segunda Fase).
- Criar um serviço de resolução de vencedor que, dado um `Jogo`, determina a seleção classificada (vitória no tempo normal ou nos pênaltis) e resolve recursivamente a seleção "vencedora de outro jogo" quando aplicável — reutilizável pelas fases futuras (Quartas, Semifinais, Terceiro Lugar, Final).
- Criar a capacidade `fase-eliminatoria`: páginas `Fase2` (`/fase2`) e `Oitavas` (`/oitavas`), com o mesmo padrão visual das páginas de Jogos/Grupos existentes, permitindo consultar os confrontos e registrar/atualizar o placar oficial. A tabela de Oitavas nunca guarda seleções fixas: os confrontos são sempre calculados a partir do vencedor atual do jogo de origem da Segunda Fase, refletindo qualquer alteração de placar imediatamente.
- Adicionar "Fase2" e "Oitavas" ao menu principal.
- Remover do `matches.json` existente as 16 entradas de Segunda Fase e as 8 de Oitavas (hoje inertes, sem seleções resolvidas), já que passam a ser carregadas pelos novos métodos dedicados — evita duplicidade de jogos no seed.

## Capabilities

### New Capabilities
- `fase-eliminatoria`: páginas e fluxo de consulta/registro de resultado da Segunda Fase e das Oitavas de Final, com propagação automática dos vencedores da Segunda Fase para os confrontos das Oitavas.

### Modified Capabilities
- `domain-model`: a entidade `Jogo` passa a suportar referência a jogos de origem (fase eliminatória) e placar de pênaltis; nova regra de domínio para determinação do vencedor de um jogo eliminatório.
- `seed-data`: carga inicial passa a incluir os jogos oficiais da Segunda Fase e das Oitavas de Final, a partir dos arquivos oficiais em `./fontes`.

## Impact

- Modelo: `Models/Copa/Jogo.cs`, `Data/AppDbContext.cs` (nova configuração de FK auto-referenciada), nova migration EF Core.
- Dados: `Data/SeedData.cs`, `Data/SeedDtos.cs`, novos `Data/SeedJson/matches_segunda_fase.json` e `Data/SeedJson/matches_oitavas.json`, remoção das entradas correspondentes de `Data/SeedJson/matches.json`.
- Serviços: novo serviço de resolução de vencedor/confrontos eliminatórios em `Services/Copa`; novo `Services/FaseEliminatoria` (ou equivalente) com DTOs para as páginas Fase2 e Oitavas.
- Interface: `Components/Layout/NavMenu.razor`, novas `Components/Pages/Fase2.razor` e `Components/Pages/Oitavas.razor` (+ componentes de apoio), reaproveitando estilos existentes (`lp-match-card`, `sim-jogo`, etc.).
- Fora do escopo: Quartas de Final, Semifinais, Terceiro Lugar, Final, e o simulador do mata-mata — o modelo e o serviço de vencedor devem ficar preparados para essas fases, mas nenhuma tela ou seed dessas fases é criada nesta mudança.

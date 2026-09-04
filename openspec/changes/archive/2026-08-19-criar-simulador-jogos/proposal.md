## Why

O PortalCopa26 já convida o visitante a "montar seu bolão" na Landing Page (`/simulador`), mas essa rota ainda não existe. Torcedores querem informar placares da fase de grupos e ver a classificação recalculada — incluindo quem se classifica direto e quem entra como um dos 8 melhores terceiros colocados — sem precisar nomear ou gerenciar simulações manualmente.

## What Changes

- Criar a página `/simulador` para simular a fase de grupos: entrada de placares por jogo, classificação automática por grupo e ranking dos terceiros colocados (wildcards) para a fase eliminatória.
- Aplicar os critérios oficiais de desempate (RN-01/RN-02 de `fontes/copa2026_regras_negocio.txt`): pontos, saldo de gols, gols marcados, confronto direto, saldo de gols no confronto direto e Ranking FIFA. **O critério de Fair Play (cartões) é omitido**: não há dados oficiais de cartões/disciplina em `./fontes` nem no domínio já modelado, e o projeto não pode inventar esse dado; o desempate salta do critério anterior diretamente para o Ranking FIFA.
- Manter automaticamente uma única simulação "atual" por instância da aplicação (sem nome, sem tela de gerenciamento de múltiplas simulações): cada alteração de placar é persistida imediatamente no SQLite, e a simulação é restaurada automaticamente ao reabrir a página.
- Destacar visualmente, na tabela de classificação, quem está classificado diretamente (1º e 2º de cada grupo) e quem está entre os 8 melhores terceiros colocados (wildcard) no momento.
- Criar os componentes reutilizáveis `SimuladorGrupo.razor`, `SimuladorJogo.razor`, `ClassificacaoGrupo.razor` e `SimuladorResumo.razor`.
- Criar um serviço próprio (`SimuladorService`) para ler/gravar a simulação via `AppDbContext`, seguindo a diretriz de não acessar o `DbContext` diretamente em páginas/componentes.
- Fora de escopo: simulação do mata-mata (oitavas em diante); a página cobre exclusivamente a fase de grupos.

## Capabilities

### New Capabilities
- `simulador`: Página e componentes do simulador da fase de grupos — entrada de placares, cálculo de classificação por grupo, aplicação dos critérios de desempate, ranking e destaque dos melhores terceiros colocados (wildcards), e persistência automática de uma simulação única.

### Modified Capabilities

## Impact

- Novo componente de página `Components/Pages/Simulador/Simulador.razor` e subcomponentes `SimuladorGrupo.razor`, `SimuladorJogo.razor`, `ClassificacaoGrupo.razor`, `SimuladorResumo.razor`.
- Novo serviço `Services/Simulador/SimuladorService.cs` (+ interface `ISimuladorService` e DTOs) registrado via injeção de dependência nativa.
- Nova rota `/simulador`, já referenciada pelo CTA existente em `SimuladorPainel.razor` (Landing Page).
- Leitura de `Jogo`/`Grupo`/`Selecao`/`RankingFifa` (fase de grupos apenas) e leitura/escrita de `Simulacao`/`SimulacaoJogo` — entidades e schema já existentes (`domain-model`, `data-persistence`); nenhuma Migration nova é necessária.
- Texto do CTA em `SimuladorPainel.razor` menciona simular "a fase de grupos e o mata-mata"; esta change entrega apenas a fase de grupos — o ajuste do texto da Landing Page fica fora do escopo desta change.

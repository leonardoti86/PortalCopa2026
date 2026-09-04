## Purpose

Definir as entidades de domínio da Copa do Mundo 2026 (grupos, seleções, jogadores, jogos, ranking FIFA e simulações) e os relacionamentos mínimos entre elas, servindo de base consistente para todas as funcionalidades futuras do portal.

## ADDED Requirements

### Requirement: Entidade Grupo
O sistema SHALL representar um Grupo da fase inicial da Copa, identificado por um código (ex.: "A") e associado às seleções que o compõem.

#### Scenario: Grupo agrega suas seleções
- **WHEN** um Grupo é consultado
- **THEN** é possível obter a lista de Seleções pertencentes a esse Grupo

### Requirement: Entidade Seleção
O sistema SHALL representar uma Seleção nacional, contendo nome, código, técnico, grupo ao qual pertence e informações de ranking FIFA associadas, além de sua lista de jogadores.

#### Scenario: Seleção pertence a exatamente um grupo
- **WHEN** uma Seleção é consultada
- **THEN** ela referencia exatamente um Grupo da fase inicial

#### Scenario: Seleção agrega seus jogadores
- **WHEN** uma Seleção é consultada
- **THEN** é possível obter a lista de Jogadores convocados para essa Seleção

### Requirement: Entidade Jogador
O sistema SHALL representar um Jogador convocado por uma Seleção, contendo nome, posição, idade, gols marcados e participação em copas do mundo.

#### Scenario: Jogador pertence a exatamente uma seleção
- **WHEN** um Jogador é consultado
- **THEN** ele referencia exatamente uma Seleção à qual está convocado

### Requirement: Entidade Jogo
O sistema SHALL representar um Jogo da Copa, contendo data, horário, fase, grupo (quando aplicável), estádio/cidade e as duas seleções envolvidas.

#### Scenario: Jogo referencia suas seleções e grupo
- **WHEN** um Jogo é consultado
- **THEN** é possível identificar as duas Seleções envolvidas e, quando o jogo pertence à fase de grupos, o Grupo correspondente

### Requirement: Entidade Ranking FIFA
O sistema SHALL representar a posição e a pontuação de uma Seleção no Ranking FIFA.

#### Scenario: Ranking associado a uma seleção
- **WHEN** o Ranking FIFA de uma Seleção é consultado
- **THEN** é possível obter sua posição e pontuação mais recentes

### Requirement: Entidade Simulação
O sistema SHALL representar uma Simulação criada por um usuário, agregando um conjunto de resultados simulados de jogos (SimulacaoJogo) e a data em que foi criada.

#### Scenario: Simulação agrega seus jogos simulados
- **WHEN** uma Simulação é consultada
- **THEN** é possível obter todos os registros de SimulacaoJogo associados a ela

### Requirement: Entidade SimulacaoJogo
O sistema SHALL representar o resultado simulado de um Jogo específico dentro de uma Simulação, contendo o placar simulado para cada seleção envolvida.

#### Scenario: SimulacaoJogo referencia o jogo original e a simulação
- **WHEN** um registro de SimulacaoJogo é consultado
- **THEN** é possível identificar a qual Simulação ele pertence e a qual Jogo real ele se refere, além do placar simulado de cada seleção

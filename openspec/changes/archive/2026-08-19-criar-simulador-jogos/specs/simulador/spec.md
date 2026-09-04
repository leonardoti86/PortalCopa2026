## Purpose

Permitir que o visitante simule os resultados da fase de grupos da Copa do Mundo FIFA 2026, calculando automaticamente a classificação de cada grupo, aplicando os critérios oficiais de desempate e destacando classificados diretos e melhores terceiros colocados (wildcards), com a simulação persistida e restaurada automaticamente.

## ADDED Requirements

### Requirement: Entrada de Placares
O Simulador SHALL permitir ao usuário informar o placar (gols do mandante e gols do visitante) de qualquer jogo da fase de grupos, um jogo por vez.

#### Scenario: Informar o placar de um jogo
- **WHEN** o usuário digita um placar para o mandante e para o visitante de um jogo da fase de grupos
- **THEN** o sistema registra o placar simulado para aquele jogo

#### Scenario: Limpar o placar de um jogo
- **WHEN** o usuário apaga o placar informado de um jogo
- **THEN** o sistema volta a tratar aquele jogo como não simulado, sem considerá-lo no cálculo da classificação

### Requirement: Classificação Automática por Grupo
O Simulador SHALL calcular e exibir, para cada grupo, a classificação das 4 seleções com base nos placares simulados até o momento, considerando apenas os jogos daquele grupo que já possuem placar informado.

#### Scenario: Classificação recalculada ao informar um placar
- **WHEN** o usuário informa ou altera o placar de um jogo de um grupo
- **THEN** o sistema recalcula e exibe a classificação atualizada daquele grupo, com número de jogos, vitórias, empates, derrotas, gols pró, gols contra, saldo de gols e pontos de cada seleção

#### Scenario: Classificação parcial com jogos ainda não simulados
- **WHEN** um grupo tem jogos sem placar informado
- **THEN** o sistema calcula a classificação considerando somente os jogos já simulados daquele grupo, sem atribuir resultado aos jogos pendentes

### Requirement: Critérios de Desempate
Quando duas ou mais seleções de um mesmo grupo estiverem empatadas em pontos, o Simulador SHALL desempatá-las aplicando, em ordem: saldo de gols, gols marcados, resultado do confronto direto entre as seleções empatadas, saldo de gols nos confrontos diretos entre as seleções empatadas e, por fim, a posição no Ranking FIFA (menor posição classificada primeiro). O critério de Fair Play (cartões) SHALL ser ignorado por falta de dados oficiais disponíveis, avançando diretamente do critério anterior para o Ranking FIFA. Quando uma seleção empatada não possuir Ranking FIFA cadastrado, ela SHALL ser tratada como a pior posição possível nesse critério; se o empate persistir, o desempate final SHALL usar a ordem alfabética do código da seleção.

#### Scenario: Empate em pontos resolvido pelo saldo de gols
- **WHEN** duas seleções do mesmo grupo têm a mesma pontuação e saldos de gols diferentes
- **THEN** o sistema posiciona à frente a seleção com o maior saldo de gols

#### Scenario: Empate remanescente resolvido pelo confronto direto
- **WHEN** duas ou mais seleções do mesmo grupo permanecem empatadas em pontos, saldo de gols e gols marcados
- **THEN** o sistema aplica o resultado do confronto direto entre as seleções empatadas para defini-las, e o saldo de gols desses confrontos diretos caso o confronto direto também esteja empatado

#### Scenario: Empate total resolvido pelo Ranking FIFA
- **WHEN** duas ou mais seleções do mesmo grupo permanecem empatadas após todos os critérios anteriores
- **THEN** o sistema posiciona à frente a seleção com a melhor posição (menor número) no Ranking FIFA

#### Scenario: Seleção sem Ranking FIFA cadastrado
- **WHEN** o critério de Ranking FIFA precisa ser aplicado e uma ou ambas as seleções empatadas não têm Ranking FIFA cadastrado
- **THEN** o sistema trata a seleção sem Ranking FIFA como a pior posição possível nesse critério e, se o empate persistir, desempata pela ordem alfabética do código da seleção

### Requirement: Ranking dos Melhores Terceiros Colocados (Wildcards)
O Simulador SHALL calcular, entre as 12 seleções em 3º lugar de seus respectivos grupos, um ranking geral aplicando pontos, saldo de gols, gols marcados e Ranking FIFA (sem confronto direto, por serem seleções de grupos diferentes), identificando as 8 melhores como classificadas via wildcard. Uma seleção sem Ranking FIFA cadastrado SHALL ser tratada como a pior posição possível nesse critério, com desempate final pela ordem alfabética do código da seleção caso o empate persista.

#### Scenario: Atualização do ranking de terceiros ao simular um jogo
- **WHEN** o usuário informa ou altera o placar de um jogo que afeta a posição de um 3º colocado
- **THEN** o sistema recalcula o ranking geral dos 12 terceiros colocados e quais 8 estão classificados como wildcard no momento

#### Scenario: Terceiro colocado sem Ranking FIFA cadastrado
- **WHEN** o ranking geral dos terceiros colocados precisa aplicar o critério de Ranking FIFA e uma ou mais seleções empatadas não têm Ranking FIFA cadastrado
- **THEN** o sistema trata essas seleções como pior posição possível nesse critério e, se o empate persistir, desempata pela ordem alfabética do código da seleção

### Requirement: Destaque de Classificados e Wildcards
O Simulador SHALL destacar visualmente, na classificação de cada grupo, as seleções em 1º e 2º lugar como classificadas diretamente, e a seleção em 3º lugar como wildcard quando ela estiver entre as 8 melhores terceiras colocadas no momento.

#### Scenario: Destaque do 1º e 2º colocados
- **WHEN** a classificação de um grupo é exibida
- **THEN** o sistema destaca visualmente as seleções em 1º e 2º lugar como classificadas

#### Scenario: Destaque condicional do 3º colocado como wildcard
- **WHEN** a seleção em 3º lugar de um grupo está entre as 8 melhores terceiras colocadas no ranking geral no momento
- **THEN** o sistema destaca essa seleção como classificada via wildcard

#### Scenario: 3º colocado fora da zona de wildcard
- **WHEN** a seleção em 3º lugar de um grupo não está entre as 8 melhores terceiras colocadas no ranking geral no momento
- **THEN** o sistema exibe essa seleção sem o destaque de wildcard

### Requirement: Persistência Automática da Simulação
O Simulador SHALL manter automaticamente uma única simulação em andamento, sem exigir que o usuário forneça um nome ou salve manualmente: cada alteração de placar SHALL ser persistida imediatamente.

#### Scenario: Placar persistido sem ação explícita de salvar
- **WHEN** o usuário informa o placar de um jogo
- **THEN** o sistema persiste esse placar automaticamente, sem exigir clique em um botão de salvar ou o preenchimento de um nome para a simulação

### Requirement: Restauração Automática da Simulação
O Simulador SHALL restaurar automaticamente a simulação em andamento, incluindo todos os placares já informados, sempre que o usuário acessar ou retornar à página do simulador.

#### Scenario: Reabertura da página restaura os placares informados
- **WHEN** o usuário acessa a página do simulador após ter informado placares em uma visita anterior
- **THEN** o sistema exibe os mesmos placares já informados e a classificação correspondente, sem exigir nova ação do usuário

### Requirement: Escopo Restrito à Fase de Grupos
O Simulador SHALL restringir a simulação exclusivamente aos jogos da fase de grupos, sem exibir ou simular jogos das fases eliminatórias (mata-mata).

#### Scenario: Jogos do mata-mata não aparecem no simulador
- **WHEN** o usuário utiliza o simulador
- **THEN** apenas jogos da fase de grupos são exibidos e simuláveis

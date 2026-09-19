## MODIFIED Requirements

### Requirement: Entidade Jogo
O sistema SHALL representar um Jogo da Copa, contendo data, horário, fase, grupo (quando aplicável), estádio/cidade, as duas seleções envolvidas e o placar oficial (gols da seleção mandante e da seleção visitante), que permanece indefinido até o resultado ser registrado. Para um jogo de fase eliminatória cujas seleções ainda não são conhecidas, o Jogo SHALL poder referenciar, para o lado mandante e/ou visitante, o jogo de origem cujo vencedor ocupará aquela posição, em vez de armazenar uma seleção fixa. O Jogo SHALL também poder armazenar o placar de pênaltis (mandante e visitante), usado apenas para desempatar um jogo eliminatório que termina empatado no placar oficial.

#### Scenario: Jogo referencia suas seleções e grupo
- **WHEN** um Jogo é consultado
- **THEN** é possível identificar as duas Seleções envolvidas e, quando o jogo pertence à fase de grupos, o Grupo correspondente

#### Scenario: Jogo sem resultado oficial registrado
- **WHEN** um Jogo ainda não teve seu resultado oficial registrado
- **THEN** o placar da seleção mandante e o placar da seleção visitante são indefinidos, sem impedir a consulta das demais informações do jogo

#### Scenario: Jogo com resultado oficial registrado
- **WHEN** o resultado oficial de um Jogo é registrado
- **THEN** é possível consultar o placar da seleção mandante e o placar da seleção visitante junto às demais informações do jogo

#### Scenario: Jogo eliminatório referencia jogos de origem em vez de seleções fixas
- **WHEN** um Jogo de fase eliminatória é criado antes de as seleções participantes serem conhecidas
- **THEN** o Jogo referencia o(s) jogo(s) de origem correspondente(s) para o lado mandante e/ou visitante, sem armazenar nenhuma seleção fixa para esse(s) lado(s)

## ADDED Requirements

### Requirement: Determinação do Vencedor de um Jogo Eliminatório
Para um jogo de fase eliminatória com ambas as seleções conhecidas (diretamente ou pela resolução do vencedor de seus jogos de origem) e com o placar oficial registrado, o sistema SHALL determinar a seleção vencedora: pelo maior número de gols quando o placar oficial não estiver empatado, ou pelo resultado da disputa de pênaltis quando o placar oficial estiver empatado e o placar de pênaltis tiver sido registrado. Um jogo eliminatório empatado no placar oficial e sem placar de pênaltis registrado SHALL ser tratado como sem vencedor definido ainda.

#### Scenario: Vencedor definido pelo placar oficial
- **WHEN** um jogo eliminatório tem placar oficial registrado e as seleções mandante e visitante têm gols diferentes
- **THEN** o sistema determina como vencedora a seleção com mais gols

#### Scenario: Vencedor definido nos pênaltis
- **WHEN** um jogo eliminatório tem placar oficial empatado e o placar de pênaltis foi registrado
- **THEN** o sistema determina como vencedora a seleção com mais gols na disputa de pênaltis

#### Scenario: Vencedor ainda não definido
- **WHEN** um jogo eliminatório não tem placar oficial registrado, ou tem placar oficial empatado sem placar de pênaltis registrado, ou depende de um jogo de origem cujo vencedor ainda não foi determinado
- **THEN** o sistema trata o vencedor desse jogo como indefinido, sem impedir a consulta das demais informações do jogo

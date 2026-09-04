## MODIFIED Requirements

### Requirement: Entidade Jogo
O sistema SHALL representar um Jogo da Copa, contendo data, horário, fase, grupo (quando aplicável), estádio/cidade, as duas seleções envolvidas e o placar oficial (gols da seleção mandante e da seleção visitante), que permanece indefinido até o resultado ser registrado.

#### Scenario: Jogo referencia suas seleções e grupo
- **WHEN** um Jogo é consultado
- **THEN** é possível identificar as duas Seleções envolvidas e, quando o jogo pertence à fase de grupos, o Grupo correspondente

#### Scenario: Jogo sem resultado oficial registrado
- **WHEN** um Jogo ainda não teve seu resultado oficial registrado
- **THEN** o placar da seleção mandante e o placar da seleção visitante são indefinidos, sem impedir a consulta das demais informações do jogo

#### Scenario: Jogo com resultado oficial registrado
- **WHEN** o resultado oficial de um Jogo é registrado
- **THEN** é possível consultar o placar da seleção mandante e o placar da seleção visitante junto às demais informações do jogo

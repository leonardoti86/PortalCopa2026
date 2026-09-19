## MODIFIED Requirements

### Requirement: Consistência dos Dados Semeados
Os dados carregados pelo SeedData SHALL respeitar os relacionamentos do domínio: cada Seleção associada a um Grupo válido, cada Jogador associado a uma Seleção válida, e cada Jogo associado às Seleções (e Grupo, quando aplicável) corretos — exceto um jogo de fase eliminatória cujo mandante e/ou visitante ainda não sejam conhecidos, que SHALL referenciar, no lugar da Seleção, o jogo de origem correto do qual sairá o participante daquele lado.

#### Scenario: Dados semeados mantêm integridade referencial
- **WHEN** o SeedData é executado
- **THEN** todas as Seleções, Jogadores e Jogos criados referenciam Grupos e Seleções existentes no mesmo processo de carga, sem referências órfãs

#### Scenario: Jogo eliminatório referencia um jogo de origem existente
- **WHEN** o SeedData cria um jogo de fase eliminatória cujo mandante ou visitante depende do vencedor de outro jogo
- **THEN** o jogo de origem referenciado foi criado no mesmo processo de carga, sem referências órfãs, e nenhuma Seleção é atribuída a esse lado do jogo

## ADDED Requirements

### Requirement: Carga dos Jogos da Segunda Fase e das Oitavas de Final
O sistema SHALL popular automaticamente, quando o banco de dados estiver vazio, os 16 jogos oficiais da Segunda Fase (com as duas seleções já definidas) e os 8 jogos oficiais das Oitavas de Final (sem seleções fixas, apenas com a referência ao respectivo jogo de origem da Segunda Fase), utilizando exclusivamente os dados oficiais de `./fontes/copa2026_jogos_segunda_fase.txt` e `./fontes/copa2026_jogos_oitavas.txt`.

#### Scenario: Carga dos 16 jogos da Segunda Fase
- **WHEN** a aplicação inicia com o banco de dados vazio
- **THEN** o sistema cria os 16 jogos da Segunda Fase, cada um com as seleções mandante e visitante, data, horário, estádio e cidade oficiais

#### Scenario: Carga dos 8 jogos das Oitavas de Final sem seleções fixas
- **WHEN** a aplicação inicia com o banco de dados vazio
- **THEN** o sistema cria os 8 jogos das Oitavas de Final, cada um com data, horário, estádio e cidade oficiais e com a referência aos dois jogos de origem da Segunda Fase, sem atribuir nenhuma seleção fixa a esses jogos

#### Scenario: Reinício não duplica os jogos das fases eliminatórias
- **WHEN** a aplicação é reiniciada e o banco de dados já contém os jogos da Segunda Fase e das Oitavas de Final
- **THEN** o sistema não insere novamente esses jogos

## ADDED Requirements

### Requirement: Carga dos Jogos das Quartas de Final e das Semifinais
O sistema SHALL popular automaticamente os 4 jogos oficiais das Quartas de Final e os 2 jogos oficiais das Semifinais, sem seleções fixas, apenas com a referência aos respectivos jogos de origem (cada jogo das Quartas referencia dois jogos das Oitavas de Final; cada jogo das Semifinais referencia dois jogos das Quartas de Final), utilizando exclusivamente os dados oficiais de `./fontes/copa2026_jogos_quartas.txt` e `./fontes/copa2026_jogos_semifinal.txt`.

#### Scenario: Carga dos 4 jogos das Quartas de Final sem seleções fixas
- **WHEN** a aplicação inicia e o banco de dados ainda não contém os jogos das Quartas de Final
- **THEN** o sistema cria os 4 jogos das Quartas de Final, cada um com número oficial do jogo, data, horário, estádio e cidade oficiais e com a referência aos dois jogos de origem das Oitavas de Final, sem atribuir nenhuma seleção fixa

#### Scenario: Carga dos 2 jogos das Semifinais sem seleções fixas
- **WHEN** a aplicação inicia e o banco de dados ainda não contém os jogos das Semifinais
- **THEN** o sistema cria os 2 jogos das Semifinais, cada um com número oficial do jogo, data, horário, estádio e cidade oficiais e com a referência aos dois jogos de origem das Quartas de Final, sem atribuir nenhuma seleção fixa

#### Scenario: Reinício não duplica os jogos das Quartas e das Semifinais
- **WHEN** a aplicação é reiniciada e o banco de dados já contém os jogos das Quartas de Final e das Semifinais
- **THEN** o sistema não insere novamente esses jogos e não altera os resultados já registrados neles

## MODIFIED Requirements

### Requirement: Seed Não Duplica Dados Existentes
O sistema SHALL evitar duplicar seleções, grupos, jogadores, jogos ou ranking FIFA já existentes no banco quando a aplicação for reiniciada. Quando as Quartas de Final ou as Semifinais ainda não tiverem seus jogos carregados em um banco que já contém os demais dados, o sistema SHALL carregar apenas os jogos da fase ausente, preservando os dados e os resultados já persistidos e sem recriar nenhum jogo existente.

#### Scenario: Reinício não duplica dados já carregados
- **WHEN** a aplicação é reiniciada e o banco de dados já contém os dados de seed
- **THEN** o sistema não insere registros duplicados de seleções, grupos, jogadores, jogos ou ranking FIFA

#### Scenario: Quartas ou Semifinais ausentes são carregadas em banco já populado
- **WHEN** a aplicação inicia com um banco de dados que já contém os dados de seed, mas ainda não tem os jogos das Quartas de Final e/ou das Semifinais
- **THEN** o sistema cria apenas os jogos da fase (ou fases) ausente, mantendo intactos os demais jogos e os resultados já registrados pelo usuário

#### Scenario: Jogo inerte de uma fase sem confronto definido não convive com o jogo oficial
- **WHEN** a aplicação inicia com um banco de dados que contém, de cargas anteriores, jogos de uma fase sem número oficial do jogo e sem referência a jogo de origem
- **THEN** o sistema garante que essa fase fique apenas com os jogos oficiais, sem confrontos duplicados para a mesma fase

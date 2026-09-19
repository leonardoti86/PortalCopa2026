## ADDED Requirements

### Requirement: Listagem dos Jogos das Quartas de Final
A página Quartas SHALL exibir os 4 jogos oficiais das Quartas de Final persistidos no banco de dados, cada um com estádio, cidade, data e horário de Brasília, e com a seleção mandante e/ou visitante calculada a partir do vencedor atual do respectivo jogo de origem das Oitavas de Final. Os 4 confrontos SHALL estar visíveis desde a primeira execução, antes de qualquer resultado ser registrado.

#### Scenario: Exibição da listagem completa das Quartas sem nenhum resultado registrado
- **WHEN** um visitante acessa a página Quartas e nenhum jogo das Oitavas tem vencedor definido
- **THEN** o sistema exibe os 4 jogos das Quartas com estádio, cidade, data e horário, e cada lado de cada confronto indica o jogo das Oitavas de que aquele participante depende, sem atribuir seleção alguma

#### Scenario: Exibição de um confronto das Quartas com os dois vencedores já definidos
- **WHEN** os dois jogos de origem das Oitavas de um confronto das Quartas já têm vencedor definido
- **THEN** o sistema exibe, nesse confronto, o nome e a bandeira das duas seleções vencedoras desses jogos de origem

#### Scenario: Exibição de um confronto das Quartas com apenas um vencedor conhecido
- **WHEN** apenas um dos dois jogos de origem das Oitavas de um confronto das Quartas tem vencedor definido
- **THEN** o sistema exibe, nesse confronto, o nome e a bandeira da seleção já conhecida de um lado e, do outro, a indicação do jogo das Oitavas ainda pendente

### Requirement: Listagem dos Jogos das Semifinais
A página Semifinais SHALL exibir os 2 jogos oficiais das Semifinais persistidos no banco de dados, cada um com estádio, cidade, data e horário de Brasília, e com a seleção mandante e/ou visitante calculada a partir do vencedor atual do respectivo jogo de origem das Quartas de Final — inclusive quando esse vencedor das Quartas só é conhecido porque os vencedores das Oitavas correspondentes já foram definidos. Os 2 confrontos SHALL estar visíveis desde a primeira execução, antes de qualquer resultado ser registrado.

#### Scenario: Exibição da listagem completa das Semifinais sem nenhum resultado registrado
- **WHEN** um visitante acessa a página Semifinais e nenhum jogo das Quartas tem vencedor definido
- **THEN** o sistema exibe os 2 jogos das Semifinais com estádio, cidade, data e horário, e cada lado de cada confronto indica o jogo das Quartas de que aquele participante depende, sem atribuir seleção alguma

#### Scenario: Exibição de um confronto das Semifinais com os dois vencedores já definidos
- **WHEN** os dois jogos de origem das Quartas de um confronto das Semifinais já têm vencedor definido
- **THEN** o sistema exibe, nesse confronto, o nome e a bandeira das duas seleções vencedoras desses jogos de origem

#### Scenario: Exibição de um confronto das Semifinais com apenas um vencedor conhecido
- **WHEN** apenas um dos dois jogos de origem das Quartas de um confronto das Semifinais tem vencedor definido
- **THEN** o sistema exibe, nesse confronto, o nome e a bandeira da seleção já conhecida de um lado e, do outro, a indicação do jogo das Quartas ainda pendente

### Requirement: Registro do Resultado Oficial das Quartas e das Semifinais
As páginas Quartas e Semifinais SHALL permitir ao usuário registrar ou atualizar o placar oficial de um jogo cujas duas seleções já sejam conhecidas, persistindo o resultado imediatamente e refletindo a mudança na interface sem exigir recarregamento manual da página. O placar oficial registrado SHALL corresponder ao resultado final do jogo, já incluída a prorrogação. Quando o placar oficial informado terminar empatado, o sistema SHALL permitir o registro do placar de pênaltis, mas SHALL rejeitar um placar de pênaltis também empatado, já que um jogo eliminatório nunca pode terminar empatado. Um jogo cujo mandante ou visitante ainda dependa de um jogo de origem sem vencedor definido SHALL ter o registro de placar bloqueado até que ambas as seleções sejam conhecidas.

#### Scenario: Registro de um resultado decidido no tempo normal ou na prorrogação
- **WHEN** o usuário informa, em Quartas ou Semifinais, um placar de um jogo com as duas seleções conhecidas em que uma delas marca mais gols que a outra e confirma
- **THEN** o sistema persiste o placar do jogo e passa a considerá-lo com vencedor definido

#### Scenario: Registro de um resultado empatado exige pênaltis
- **WHEN** o usuário informa um placar oficial empatado para um jogo das Quartas ou das Semifinais
- **THEN** o sistema apresenta campos para o placar de pênaltis e só considera o jogo com vencedor definido depois que esse placar de pênaltis é registrado com um resultado decisivo

#### Scenario: Placar de pênaltis empatado é rejeitado
- **WHEN** o usuário informa um placar de pênaltis também empatado para um jogo das Quartas ou das Semifinais
- **THEN** o sistema rejeita o registro, sem persistir o placar de pênaltis informado, e mantém o jogo sem vencedor definido

#### Scenario: Registro de placar bloqueado enquanto uma seleção está pendente
- **WHEN** o usuário visualiza, em Quartas ou Semifinais, um confronto em que o mandante ou o visitante ainda depende do vencedor de um jogo sem resultado definido
- **THEN** o sistema não permite informar o placar oficial desse confronto até que as duas seleções sejam conhecidas

### Requirement: Propagação Automática em Cascata dos Vencedores até as Semifinais
As tabelas de Quartas e Semifinais SHALL sempre refletir o vencedor mais recente de cada jogo de origem, percorrendo o encadeamento completo do chaveamento (Segunda Fase → Oitavas → Quartas → Semifinais), sem copiar nem fixar seleções nos jogos dessas fases e sem recriar os jogos já existentes.

#### Scenario: Definir o vencedor de um jogo das Oitavas alimenta o confronto das Quartas
- **WHEN** o usuário registra, na página Oitavas, um placar que define o vencedor de um jogo das Oitavas
- **THEN** a página Quartas passa a exibir, no lado correspondente do confronto dependente, a seleção vencedora desse jogo das Oitavas, sem que nenhum jogo seja recriado ou duplicado

#### Scenario: Definir o vencedor de um jogo das Quartas alimenta o confronto das Semifinais
- **WHEN** o usuário registra, na página Quartas, um placar que define o vencedor de um jogo das Quartas
- **THEN** a página Semifinais passa a exibir, no lado correspondente do confronto dependente, a seleção vencedora desse jogo das Quartas

#### Scenario: Vencedor decidido nos pênaltis é propagado
- **WHEN** um jogo das Oitavas ou das Quartas termina empatado no placar oficial e tem o vencedor decidido pelo placar de pênaltis
- **THEN** a fase seguinte exibe essa seleção vencedora no lado correspondente do confronto dependente

#### Scenario: Alteração de um resultado recalcula as fases seguintes
- **WHEN** o usuário altera o placar de um jogo já registrado de forma que o vencedor passe a ser a outra seleção
- **THEN** todas as fases seguintes que dependem desse jogo passam a exibir a nova seleção vencedora no lado correspondente, sem intervenção manual

#### Scenario: Remoção de um resultado faz voltar o placeholder
- **WHEN** o usuário remove o placar de um jogo, ou o altera de forma que ele deixe de ter vencedor definido (por exemplo, voltando a um empate sem pênaltis registrados)
- **THEN** as fases seguintes que dependiam desse jogo voltam a exibir, no lado correspondente, a indicação do jogo de origem pendente, e o registro de placar desses confrontos volta a ficar bloqueado

### Requirement: Navegação para Quartas e Semifinais pelo Menu Principal
O menu principal SHALL exibir as opções "Quartas" e "Semifinais", direcionando o usuário para as páginas correspondentes.

#### Scenario: Acesso às Quartas pelo menu
- **WHEN** o usuário clica na opção "Quartas" do menu principal
- **THEN** o sistema navega para a página Quartas

#### Scenario: Acesso às Semifinais pelo menu
- **WHEN** o usuário clica na opção "Semifinais" do menu principal
- **THEN** o sistema navega para a página Semifinais

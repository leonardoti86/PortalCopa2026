## Purpose

Apresentar, em uma única página, o jogo da Disputa do Terceiro Lugar e o jogo da Final da Copa do Mundo FIFA 2026, propagando automaticamente os perdedores e vencedores das Semifinais para esses confrontos, e destacar o Campeão da Copa assim que a Final tiver um vencedor definido.

## ADDED Requirements

### Requirement: Exibição do Jogo de Disputa do Terceiro Lugar
A página Final SHALL exibir o jogo oficial da Disputa do Terceiro Lugar persistido no banco de dados, com estádio, cidade, data e horário de Brasília, e com a seleção mandante e/ou visitante calculada a partir do **perdedor** atual da Semifinal 1 e da Semifinal 2, respectivamente. O confronto SHALL estar visível desde a primeira execução, antes de qualquer resultado das Semifinais ser registrado.

#### Scenario: Exibição do Terceiro Lugar sem nenhuma Semifinal decidida
- **WHEN** um visitante acessa a página Final e nenhuma Semifinal tem vencedor definido
- **THEN** o sistema exibe o confronto do Terceiro Lugar com estádio, cidade, data e horário, e ambos os lados exibem o placeholder "Perdedor Semifinal 1" e "Perdedor Semifinal 2", sem atribuir seleção alguma

#### Scenario: Exibição do Terceiro Lugar com apenas um perdedor conhecido
- **WHEN** apenas uma das duas Semifinais tem resultado que já define um perdedor
- **THEN** o sistema exibe, no lado correspondente do Terceiro Lugar, o nome e a bandeira da seleção perdedora, e no outro lado o placeholder "Perdedor Semifinal N" pendente

#### Scenario: Exibição do Terceiro Lugar com os dois perdedores conhecidos
- **WHEN** as duas Semifinais já têm vencedor (e, portanto, perdedor) definido
- **THEN** o sistema exibe, no confronto do Terceiro Lugar, o nome e a bandeira das duas seleções perdedoras

### Requirement: Exibição do Jogo da Final
A página Final SHALL exibir o jogo oficial da Final persistido no banco de dados, com estádio, cidade, data e horário de Brasília, e com a seleção mandante e/ou visitante calculada a partir do **vencedor** atual da Semifinal 1 e da Semifinal 2, respectivamente. O confronto SHALL estar visível desde a primeira execução, antes de qualquer resultado das Semifinais ser registrado.

#### Scenario: Exibição da Final sem nenhuma Semifinal decidida
- **WHEN** um visitante acessa a página Final e nenhuma Semifinal tem vencedor definido
- **THEN** o sistema exibe o confronto da Final com estádio, cidade, data e horário, e ambos os lados exibem o placeholder "Vencedor Semifinal 1" e "Vencedor Semifinal 2", sem atribuir seleção alguma

#### Scenario: Exibição da Final com apenas um vencedor conhecido
- **WHEN** apenas uma das duas Semifinais tem vencedor definido
- **THEN** o sistema exibe, no lado correspondente da Final, o nome e a bandeira da seleção vencedora, e no outro lado o placeholder "Vencedor Semifinal N" pendente

#### Scenario: Exibição da Final com os dois vencedores conhecidos
- **WHEN** as duas Semifinais já têm vencedor definido
- **THEN** o sistema exibe, no confronto da Final, o nome e a bandeira das duas seleções vencedoras

### Requirement: Registro do Resultado Oficial do Terceiro Lugar e da Final
A página Final SHALL permitir ao usuário registrar ou atualizar o placar oficial do Terceiro Lugar e/ou da Final quando as duas seleções desse confronto já forem conhecidas, persistindo o resultado imediatamente e refletindo a mudança na interface sem exigir recarregamento manual da página. O placar oficial registrado SHALL corresponder ao resultado final do jogo, já incluída a prorrogação. Quando o placar oficial informado terminar empatado, o sistema SHALL permitir o registro do placar de pênaltis, mas SHALL rejeitar um placar de pênaltis também empatado, já que nenhum desses dois jogos pode terminar empatado. Um confronto cujo mandante ou visitante ainda dependa de uma Semifinal sem vencedor definido SHALL ter o registro de placar bloqueado até que ambas as seleções sejam conhecidas.

#### Scenario: Registro de um resultado decidido no tempo normal ou na prorrogação
- **WHEN** o usuário informa, no Terceiro Lugar ou na Final, um placar com as duas seleções conhecidas em que uma delas marca mais gols que a outra e confirma
- **THEN** o sistema persiste o placar do jogo e passa a considerá-lo com vencedor (e perdedor) definido

#### Scenario: Resultado decidido nos pênaltis é registrado
- **WHEN** o usuário informa um placar oficial empatado e, em seguida, um placar de pênaltis decisivo para o Terceiro Lugar ou para a Final
- **THEN** o sistema persiste os dois placares e passa a considerar o jogo com vencedor (e perdedor) definido pelo resultado dos pênaltis

#### Scenario: Placar de pênaltis empatado é rejeitado
- **WHEN** o usuário informa um placar de pênaltis também empatado para o Terceiro Lugar ou para a Final
- **THEN** o sistema rejeita o registro, sem persistir o placar de pênaltis informado, e mantém o jogo sem vencedor definido

#### Scenario: Registro de placar bloqueado enquanto uma seleção está pendente
- **WHEN** o usuário visualiza, na página Final, um confronto em que o mandante ou o visitante ainda depende do resultado de uma Semifinal sem vencedor definido
- **THEN** o sistema não permite informar o placar oficial desse confronto até que as duas seleções sejam conhecidas

### Requirement: Propagação Automática dos Vencedores e Perdedores das Semifinais
O Terceiro Lugar e a Final SHALL sempre refletir, respectivamente, o perdedor e o vencedor mais recentes de cada Semifinal de origem, sem copiar ou fixar seleções nesses dois jogos e sem recriar ou duplicar jogo algum. Alterar ou remover o resultado de uma Semifinal, do Terceiro Lugar ou da Final SHALL recalcular automaticamente os jogos dependentes, sem necessidade de recarregar a página.

#### Scenario: Definir o resultado de uma Semifinal alimenta o Terceiro Lugar e a Final
- **WHEN** o usuário registra, na página Semifinais, um placar que define o vencedor de uma Semifinal
- **THEN** a página Final passa a exibir, no lado correspondente, a seleção vencedora dessa Semifinal na Final e a seleção perdedora dessa Semifinal no Terceiro Lugar, sem que nenhum jogo seja recriado ou duplicado

#### Scenario: Vencedor e perdedor decididos nos pênaltis são propagados
- **WHEN** uma Semifinal termina empatada no placar oficial e tem o vencedor decidido pelo placar de pênaltis
- **THEN** a página Final exibe corretamente a seleção vencedora na Final e a seleção perdedora no Terceiro Lugar, no lado correspondente

#### Scenario: Alteração do resultado de uma Semifinal recalcula o chaveamento
- **WHEN** o usuário altera o placar de uma Semifinal já registrada de forma que o vencedor passe a ser a outra seleção
- **THEN** o Terceiro Lugar e a Final passam a exibir a nova seleção vencedora e a nova seleção perdedora no lado correspondente, sem intervenção manual

#### Scenario: Remoção do resultado de uma Semifinal faz voltar o placeholder
- **WHEN** o usuário remove o placar de uma Semifinal, ou o altera de forma que ela deixe de ter vencedor definido
- **THEN** o Terceiro Lugar e a Final voltam a exibir, no lado correspondente, o placeholder daquela Semifinal, e o registro de placar desses dois jogos volta a ficar bloqueado se a seleção removida era necessária

### Requirement: Destaque do Campeão da Copa do Mundo
A página Final SHALL exibir um painel de destaque do Campeão da Copa do Mundo FIFA 2026 assim que a Final tiver um vencedor definido, contendo fundo dourado, ícone de taça, bandeira da seleção campeã em tamanho ampliado, nome da seleção em fonte maior que a dos demais textos da página, e a quantidade de títulos mundiais dessa seleção. Essa quantidade SHALL refletir o histórico real de títulos da seleção até a Copa de 2022 (o último torneio efetivamente disputado), sem somar o resultado simulado desta Final. O painel SHALL deixar de ser exibido caso a Final deixe de ter vencedor definido.

#### Scenario: Painel do campeão ausente antes da Final ser decidida
- **WHEN** a Final ainda não tem vencedor definido
- **THEN** a página Final não exibe o painel de destaque do campeão

#### Scenario: Painel do campeão exibido após a Final ser decidida
- **WHEN** a Final passa a ter um vencedor definido (no tempo normal, na prorrogação ou nos pênaltis)
- **THEN** a página Final exibe o painel de destaque com fundo dourado, taça, bandeira ampliada, nome e quantidade de títulos mundiais da seleção vencedora

#### Scenario: Alteração do resultado da Final recalcula o campeão
- **WHEN** o usuário altera o placar já registrado da Final de forma que o vencedor passe a ser a outra seleção
- **THEN** o painel de destaque passa a exibir a nova seleção campeã

#### Scenario: Remoção do resultado da Final remove o destaque
- **WHEN** o usuário remove o resultado da Final (ou o altera de forma que ela deixe de ter vencedor definido)
- **THEN** o painel de destaque do campeão deixa de ser exibido

### Requirement: Exibição das Fotos dos Estádios do Terceiro Lugar e da Final
A página Final SHALL exibir a foto do estádio do jogo do Terceiro Lugar e a foto do estádio do jogo da Final, cada uma associada ao respectivo confronto.

#### Scenario: Fotos dos estádios exibidas ao acessar a página
- **WHEN** um visitante acessa a página Final
- **THEN** o sistema exibe a foto do estádio do Terceiro Lugar junto a esse confronto e a foto do estádio da Final junto a esse confronto

### Requirement: Navegação para a Final pelo Menu Principal
O menu principal SHALL exibir a opção "Final", direcionando o usuário para a página Final.

#### Scenario: Acesso à página Final pelo menu
- **WHEN** o usuário clica na opção "Final" do menu principal
- **THEN** o sistema navega para a página Final, exibindo o Terceiro Lugar e a Final com o estado atual do chaveamento

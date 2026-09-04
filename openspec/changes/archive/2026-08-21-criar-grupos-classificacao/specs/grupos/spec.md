## Purpose

Apresentar os 12 grupos da fase de grupos da Copa do Mundo FIFA 2026, com a classificação oficial calculada a partir dos resultados registrados e a possibilidade de registrar/atualizar esses resultados diretamente na página.

## ADDED Requirements

### Requirement: Navegação por Abas entre Grupos
A página de Grupos SHALL exibir uma aba para cada um dos 12 grupos oficiais da fase de grupos, permitindo ao usuário alternar entre eles para visualizar a classificação e os jogos do grupo selecionado.

#### Scenario: Exibição inicial com o primeiro grupo selecionado
- **WHEN** um visitante acessa a página de Grupos
- **THEN** o sistema exibe as 12 abas de grupo e apresenta, por padrão, a classificação e os jogos do primeiro grupo

#### Scenario: Troca de grupo pela aba
- **WHEN** o usuário clica na aba de outro grupo
- **THEN** o sistema exibe a classificação e os jogos do grupo selecionado, sem recarregar a página

### Requirement: Legenda de Classificação
A página de Grupos SHALL exibir uma legenda indicando o significado das posições de classificação (classificado para o mata-mata e possível vaga de melhor terceiro).

#### Scenario: Legenda visível independentemente do grupo selecionado
- **WHEN** a página de Grupos é exibida
- **THEN** a legenda de classificação é apresentada de forma visível, indicando a cor/indicador usado para 1º/2º colocado e para o 3º colocado

### Requirement: Classificação Oficial do Grupo
A página de Grupos SHALL exibir, para o grupo selecionado, a tabela de classificação com Posição, Seleção, Jogos, Vitórias, Empates, Derrotas, Gols Pró, Gols Contra, Saldo de Gols e Pontos de cada seleção do grupo, calculada exclusivamente a partir dos resultados oficiais registrados nos jogos daquele grupo (placar persistido em `Jogos`). Os resultados usados pelo Simulador SHALL permanecer independentes e não SHALL interferir nesta classificação.

#### Scenario: Classificação recalculada a partir dos resultados oficiais
- **WHEN** o grupo selecionado possui um ou mais jogos com placar oficial registrado
- **THEN** o sistema calcula e exibe, para cada seleção do grupo, os totais de jogos, vitórias, empates, derrotas, gols pró, gols contra, saldo de gols e pontos considerando apenas os jogos com placar oficial registrado

#### Scenario: Grupo sem nenhum resultado oficial registrado
- **WHEN** nenhum jogo do grupo selecionado possui placar oficial registrado
- **THEN** o sistema exibe todas as seleções do grupo com zero jogos, zero pontos e saldo de gols zero, sem erro

#### Scenario: Classificação independente do Simulador
- **WHEN** existem resultados simulados registrados para jogos do grupo selecionado
- **THEN** a classificação exibida na página de Grupos não é afetada pelos resultados simulados, refletindo apenas os resultados oficiais

### Requirement: Indicação de Vagas no Mata-Mata
A tabela de classificação SHALL indicar visualmente que o 1º e o 2º colocado do grupo avançam automaticamente ao mata-mata. Para o 3º colocado, o sistema SHALL calcular, entre os terceiros colocados dos 12 grupos, quais 8 estariam classificados pela vaga de melhor terceiro (mesmos critérios de desempate da seção "Critérios de Desempate da Classificação Oficial", exceto o confronto direto, que não se aplica a seleções de grupos diferentes) e SHALL indicar visualmente se o 3º colocado do grupo selecionado está ou não entre esses 8, sem afirmar classificação garantida — o resultado pode mudar conforme mais resultados oficiais forem registrados nos demais grupos.

#### Scenario: Destaque das duas primeiras posições
- **WHEN** a classificação de um grupo é exibida
- **THEN** o sistema destaca visualmente o 1º e o 2º colocado como classificados

#### Scenario: 3º colocado entre os 8 melhores terceiros
- **WHEN** o 3º colocado do grupo selecionado está, na comparação entre os terceiros colocados dos 12 grupos, entre os 8 melhores segundo os critérios de desempate
- **THEN** o sistema destaca visualmente esse 3º colocado como possível vaga de melhor terceiro

#### Scenario: 3º colocado fora dos 8 melhores terceiros
- **WHEN** o 3º colocado do grupo selecionado não está, na comparação entre os terceiros colocados dos 12 grupos, entre os 8 melhores segundo os critérios de desempate
- **THEN** o sistema exibe o 3º colocado sem o destaque de possível vaga de melhor terceiro

### Requirement: Critérios de Desempate da Classificação Oficial
Quando duas ou mais seleções de um mesmo grupo estiverem empatadas em pontos na classificação oficial, o sistema SHALL desempatá-las aplicando, em ordem: saldo de gols, gols marcados, resultado do confronto direto entre as seleções empatadas, saldo de gols nos confrontos diretos entre as seleções empatadas e, por fim, a posição no Ranking FIFA (menor posição classificada primeiro). O critério de Fair Play (cartões) SHALL ser ignorado por falta de dados oficiais disponíveis, avançando diretamente do critério anterior para o Ranking FIFA. Quando uma seleção empatada não possuir Ranking FIFA cadastrado, ela SHALL ser tratada como a pior posição possível nesse critério; se o empate persistir, o desempate final SHALL usar a ordem alfabética do código da seleção.

#### Scenario: Empate em pontos resolvido pelo saldo de gols
- **WHEN** duas seleções do mesmo grupo têm a mesma pontuação oficial e saldos de gols diferentes
- **THEN** o sistema posiciona à frente, na classificação oficial, a seleção com o maior saldo de gols

#### Scenario: Empate resolvido pelo confronto direto
- **WHEN** duas seleções empatadas em pontos, saldo de gols e gols marcados se enfrentaram entre si na fase de grupos com placar oficial registrado
- **THEN** o sistema posiciona à frente, na classificação oficial, a seleção vencedora desse confronto direto

### Requirement: Jogos do Grupo Selecionado
A página de Grupos SHALL exibir a lista dos jogos da fase de grupos pertencentes ao grupo selecionado, com as seleções mandante e visitante, data, horário e o placar oficial quando já registrado.

#### Scenario: Exibição dos jogos com resultado pendente
- **WHEN** um jogo do grupo selecionado ainda não possui placar oficial registrado
- **THEN** o sistema exibe o jogo com as seleções, data e horário, sem apresentar um placar

#### Scenario: Exibição dos jogos com resultado já registrado
- **WHEN** um jogo do grupo selecionado já possui placar oficial registrado
- **THEN** o sistema exibe o placar oficial junto ao jogo

### Requirement: Registro e Atualização do Resultado Oficial
A página de Grupos SHALL permitir ao usuário registrar ou atualizar o placar oficial de um jogo da fase de grupos diretamente na lista de jogos do grupo selecionado. Ao salvar um resultado, o sistema SHALL persistir o placar no jogo correspondente, recalcular imediatamente a classificação e as estatísticas das seleções envolvidas, e refletir a mudança na interface sem exigir recarregamento manual da página.

#### Scenario: Registro de um novo resultado oficial
- **WHEN** o usuário informa o placar de um jogo do grupo selecionado que ainda não possui resultado oficial e confirma
- **THEN** o sistema persiste o placar em `Jogos`, atualiza a classificação e as estatísticas do grupo exibidas na página

#### Scenario: Atualização de um resultado oficial já registrado
- **WHEN** o usuário altera o placar de um jogo do grupo selecionado que já possui resultado oficial e confirma
- **THEN** o sistema atualiza o placar persistido em `Jogos`, recalculando a classificação e as estatísticas do grupo com o novo resultado

### Requirement: Acesso ao Simulador a partir do Grupo
A página de Grupos SHALL exibir, para o grupo selecionado, um botão "Simular" que direciona o usuário para a página do Simulador.

#### Scenario: Navegação para o Simulador
- **WHEN** o usuário clica no botão "Simular" do grupo selecionado
- **THEN** o sistema navega para a página do Simulador

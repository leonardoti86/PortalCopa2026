# jogos Specification

## Purpose

Apresentar a listagem completa dos jogos da fase de grupos da Copa do Mundo FIFA 2026, agrupados por data e filtráveis por grupo, com acesso direto à página de Grupos, usando exclusivamente os dados oficiais persistidos no banco.

## Requirements

### Requirement: Listagem de Jogos
A página de Jogos SHALL exibir todos os jogos da fase de grupos cadastrados no banco de dados, sem dados fictícios ou confrontos inventados.

#### Scenario: Exibição da listagem completa
- **WHEN** um visitante acessa a página de Jogos
- **THEN** o sistema exibe todos os jogos da fase de grupos persistidos no banco de dados

### Requirement: Agrupamento por Data
A página de Jogos SHALL agrupar os jogos exibidos por data, apresentando um cabeçalho de data para cada grupo de jogos daquele dia.

#### Scenario: Jogos agrupados sob um cabeçalho de data
- **WHEN** existem múltiplos jogos ocorrendo em datas diferentes
- **THEN** o sistema exibe os jogos organizados em blocos, cada bloco precedido por um cabeçalho com a data correspondente

### Requirement: Ordenação por Data e Horário
A página de Jogos SHALL ordenar os jogos e os blocos de data em ordem crescente de data e, dentro de uma mesma data, em ordem crescente de horário.

#### Scenario: Ordem crescente de exibição
- **WHEN** a listagem de jogos é carregada
- **THEN** o sistema exibe os blocos de data da data mais próxima para a mais distante, e dentro de cada data os jogos aparecem do horário mais cedo para o mais tarde

### Requirement: Filtro por Grupo
A página de Jogos SHALL permitir ao usuário filtrar a listagem para exibir apenas os jogos de um grupo específico, com uma opção para remover o filtro e voltar a exibir todos os jogos.

#### Scenario: Filtragem por um grupo específico
- **WHEN** o usuário seleciona um grupo no filtro
- **THEN** o sistema exibe apenas os jogos daquele grupo, mantendo o agrupamento por data e a ordenação por data/horário

#### Scenario: Remoção do filtro
- **WHEN** o usuário remove a seleção de grupo no filtro (opção "Todos os grupos")
- **THEN** o sistema volta a exibir todos os jogos da fase de grupos

#### Scenario: Nenhum jogo encontrado para o grupo filtrado
- **WHEN** o usuário filtra por um grupo que não possui jogos cadastrados
- **THEN** o sistema exibe uma mensagem informando que não há jogos para o filtro selecionado, sem apresentar erro

### Requirement: Informações do Grupo e do Estádio
Cada jogo exibido na listagem SHALL apresentar as informações do grupo ao qual pertence e as informações do estádio onde será disputado (nome do estádio e cidade-sede).

#### Scenario: Exibição de grupo e estádio no jogo
- **WHEN** um jogo é exibido na listagem
- **THEN** o sistema mostra o código do grupo, o nome do estádio e a cidade-sede associados àquele jogo, além das seleções mandante e visitante e o horário

### Requirement: Navegação para Grupos
A página de Jogos SHALL exibir um botão "Ver Grupos" que direciona o usuário para a página de Grupos.

#### Scenario: Acesso à página de Grupos a partir da listagem de jogos
- **WHEN** o usuário clica no botão "Ver Grupos"
- **THEN** o sistema navega para a página de Grupos

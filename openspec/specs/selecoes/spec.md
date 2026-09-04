# selecoes Specification

## Purpose

Apresentar as 48 seleções classificadas para a Copa do Mundo FIFA 2026, permitindo localizar uma seleção por nome ou grupo e consultar, em detalhe, seu técnico, ranking FIFA e elenco convocado.

## Requirements

### Requirement: Acesso pelo Menu Principal
O sistema SHALL disponibilizar um item "Seleções" no menu principal, que leva à página de listagem das seleções.

#### Scenario: Navegação pelo menu
- **WHEN** o usuário clica no item "Seleções" do menu principal
- **THEN** o sistema exibe a página de listagem das seleções

### Requirement: Listagem das Seleções
A página de Seleções SHALL exibir as 48 seleções participantes da Copa do Mundo FIFA 2026 em formato de cartões, cada um apresentando a bandeira da seleção, o nome da seleção, o código FIFA e o grupo ao qual pertence.

#### Scenario: Exibição inicial da listagem
- **WHEN** um visitante acessa a página de Seleções
- **THEN** o sistema exibe 48 cartões de seleção, cada um com bandeira, nome, código FIFA (ex.: "BRA") e grupo (ex.: "Grupo A")

### Requirement: Busca por Seleção
A página de Seleções SHALL permitir buscar seleções pelo nome, filtrando os cartões exibidos conforme o texto digitado, sem recarregar a página.

#### Scenario: Busca retorna resultado correspondente
- **WHEN** o usuário digita um trecho do nome de uma seleção no campo de busca
- **THEN** o sistema exibe apenas os cartões das seleções cujo nome contém o texto digitado

#### Scenario: Busca sem resultado
- **WHEN** o texto digitado não corresponde ao nome de nenhuma seleção
- **THEN** o sistema exibe uma mensagem indicando que nenhuma seleção foi encontrada

### Requirement: Filtro por Grupo
A página de Seleções SHALL permitir filtrar os cartões exibidos por grupo (A a L), combinando o filtro com a busca por nome quando ambos estiverem preenchidos.

#### Scenario: Filtro por um grupo específico
- **WHEN** o usuário seleciona um grupo (ex.: "Grupo A") no filtro de grupo
- **THEN** o sistema exibe apenas os cartões das seleções pertencentes ao grupo selecionado

#### Scenario: Busca e filtro combinados
- **WHEN** o usuário preenche a busca por nome e seleciona um grupo simultaneamente
- **THEN** o sistema exibe apenas os cartões que atendem a ambos os critérios

#### Scenario: Remoção do filtro de grupo
- **WHEN** o usuário seleciona a opção "Todos os grupos"
- **THEN** o sistema volta a exibir as seleções de todos os grupos, respeitando a busca por nome se preenchida

### Requirement: Modal de Detalhe da Seleção
Ao selecionar um cartão de seleção, o sistema SHALL abrir uma janela modal exibindo a bandeira, o nome, o grupo, o nome do técnico e a posição/pontuação no ranking FIFA da seleção selecionada.

#### Scenario: Abertura do modal com dados da seleção
- **WHEN** o usuário clica em um cartão de seleção na listagem
- **THEN** o sistema abre uma janela modal exibindo bandeira, nome, grupo, técnico e ranking FIFA daquela seleção

#### Scenario: Fechamento do modal
- **WHEN** o usuário aciona o controle de fechamento do modal
- **THEN** o sistema fecha a janela modal e retorna à listagem de seleções

#### Scenario: Seleção sem ranking FIFA cadastrado
- **WHEN** a seleção selecionada não possui ranking FIFA registrado na fonte de dados
- **THEN** o sistema exibe o campo de ranking com indicação de dado não disponível, sem gerar erro

### Requirement: Elenco de Jogadores no Modal
O modal de detalhe da seleção SHALL exibir a lista de jogadores convocados registrados para aquela seleção na fonte oficial, ordenados alfabeticamente pelo nome, apresentando para cada jogador o nome, a posição, a idade e o total de gols marcados pela seleção.

#### Scenario: Exibição do elenco completo
- **WHEN** o modal de uma seleção é aberto
- **THEN** o sistema lista todos os jogadores convocados registrados para aquela seleção, em ordem alfabética pelo nome, cada um com nome, posição, idade e gols pela seleção

#### Scenario: Quantidade de jogadores conforme fonte oficial
- **WHEN** a fonte oficial de dados (`copa2026_selecoes_jogadores.txt`) registra uma quantidade de convocados diferente de 26 para determinada seleção
- **THEN** o sistema exibe exatamente os jogadores presentes na fonte para aquela seleção, sem completar nem remover jogadores para atingir um número fixo

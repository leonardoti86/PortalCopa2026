## Purpose

Apresentar o Ranking FIFA das seleções da Copa do Mundo FIFA 2026, permitindo consultar posição, pontuação e grupo de cada seleção e buscar uma seleção específica pelo nome.

## ADDED Requirements

### Requirement: Acesso pelo Menu Principal
O sistema SHALL disponibilizar um item "Ranking" no menu principal, que leva à página de Ranking FIFA.

#### Scenario: Navegação pelo menu
- **WHEN** o usuário clica no item "Ranking" do menu principal
- **THEN** o sistema exibe a página de Ranking FIFA

### Requirement: Listagem do Ranking FIFA
A página de Ranking SHALL exibir, para cada seleção da Copa 2026 com posição registrada na fonte oficial de Ranking FIFA, a posição, a bandeira, o nome da seleção, a pontuação e o grupo, ordenados pela posição no ranking (do 1º colocado em diante).

#### Scenario: Exibição inicial da listagem
- **WHEN** um visitante acessa a página de Ranking
- **THEN** o sistema exibe a lista das seleções da Copa 2026 com posição no Ranking FIFA, ordenada da melhor para a pior posição, cada linha com posição, bandeira, nome, pontuação e grupo

### Requirement: Destaque das Três Primeiras Posições
A página de Ranking SHALL destacar visualmente as seleções nas 3 primeiras posições do ranking exibido, com um plano de fundo escuro e texto em cor clara para garantir contraste. Esse destaque SHALL permanecer fixo (sempre as 3 primeiras posições do ranking completo), independentemente de qualquer busca por nome ou filtro por grupo aplicado à tabela principal.

#### Scenario: Exibição do destaque de Top 3
- **WHEN** a página de Ranking é carregada e há pelo menos 3 seleções com posição registrada
- **THEN** o sistema exibe as seleções da 1ª, 2ª e 3ª posição em um destaque com fundo escuro e texto em cor clara, distinto do restante da listagem

#### Scenario: Menos de 3 seleções com posição registrada
- **WHEN** a página de Ranking é carregada e há menos de 3 seleções com posição registrada
- **THEN** o sistema exibe o destaque apenas com as seleções que possuem posição, sem preencher os lugares restantes com dado inventado

#### Scenario: Destaque não é afetado por busca ou filtro
- **WHEN** o usuário digita um termo de busca ou seleciona um grupo no filtro
- **THEN** o destaque das 3 primeiras posições continua exibindo as mesmas 3 seleções de antes, sem ser filtrado ou ocultado

### Requirement: Busca por Seleção
A página de Ranking SHALL permitir buscar seleções pelo nome, filtrando a tabela principal (seleções a partir da 4ª posição) exibida conforme o texto digitado, sem recarregar a página. A busca não afeta o destaque de Top 3 nem a seção de seleções sem posição no ranking, que continuam sempre exibidos por completo.

#### Scenario: Busca retorna resultado correspondente
- **WHEN** o usuário digita um trecho do nome de uma seleção no campo de busca
- **THEN** o sistema exibe, na tabela principal, apenas as linhas das seleções cujo nome contém o texto digitado

#### Scenario: Busca sem resultado
- **WHEN** o texto digitado não corresponde ao nome de nenhuma seleção da tabela principal
- **THEN** o sistema exibe uma mensagem indicando que nenhuma seleção foi encontrada, mantendo o destaque de Top 3 e a seção de seleções sem posição inalterados

### Requirement: Filtro por Grupo
A página de Ranking SHALL permitir filtrar a tabela principal (seleções a partir da 4ª posição) por grupo (A a L), combinando o filtro com a busca por nome quando ambos estiverem preenchidos. Assim como a busca, o filtro por grupo não afeta o destaque de Top 3 nem a seção de seleções sem posição no ranking.

#### Scenario: Filtro por um grupo específico
- **WHEN** o usuário seleciona um grupo (ex.: "Grupo A") no filtro de grupo
- **THEN** o sistema exibe, na tabela principal, apenas as seleções pertencentes ao grupo selecionado

#### Scenario: Busca e filtro combinados
- **WHEN** o usuário preenche a busca por nome e seleciona um grupo simultaneamente
- **THEN** o sistema exibe, na tabela principal, apenas as seleções que atendem a ambos os critérios

#### Scenario: Remoção do filtro de grupo
- **WHEN** o usuário seleciona a opção "Todos os grupos"
- **THEN** a tabela principal volta a exibir as seleções de todos os grupos, respeitando a busca por nome se preenchida

### Requirement: Seleções sem Posição no Ranking FIFA
Quando uma seleção da Copa 2026 não possuir posição registrada na fonte oficial de Ranking FIFA, o sistema SHALL listá-la separadamente da tabela principal, sem inventar uma posição ou pontuação para ela. Essa seção SHALL ser sempre exibida por completo, independentemente de qualquer busca por nome ou filtro por grupo aplicado à tabela principal.

#### Scenario: Seleções participantes sem dado de ranking
- **WHEN** existem seleções da Copa 2026 sem posição registrada na fonte oficial de Ranking FIFA
- **THEN** o sistema exibe essas seleções em uma seção separada, indicando que não há posição de ranking disponível na fonte de dados, sem incluí-las na tabela ordenada por posição

#### Scenario: Seção não é afetada por busca ou filtro
- **WHEN** o usuário digita um termo de busca ou seleciona um grupo no filtro da tabela principal
- **THEN** a seção de seleções sem posição no ranking continua exibindo todas as seleções sem posição, sem ser filtrada ou ocultada

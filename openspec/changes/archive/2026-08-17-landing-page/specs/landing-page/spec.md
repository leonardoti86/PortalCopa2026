## Purpose

Apresentar a página inicial do PortalCopa26 com uma visão geral da Copa do Mundo FIFA 2026 — destaque visual, estatísticas do torneio, próximos jogos, Top 10 do ranking FIFA e uma chamada para o simulador — usando exclusivamente os dados oficiais persistidos no banco.

## ADDED Requirements

### Requirement: Hero Section
A Landing Page SHALL exibir uma seção de destaque (hero) com o nome do portal, uma chamada de identidade da Copa, os três países-sede e ações de navegação para a listagem de jogos e para o simulador.

#### Scenario: Exibição do hero ao carregar a página inicial
- **WHEN** um visitante acessa a página inicial do portal
- **THEN** o sistema exibe a seção hero com os três países-sede (Canadá, Estados Unidos, México) e um botão de chamada para o simulador

### Requirement: Estatísticas da Copa
A Landing Page SHALL exibir os totais oficiais do torneio — quantidade de seleções, quantidade de cidades-sede e quantidade de jogos — calculados a partir dos dados persistidos no banco.

#### Scenario: Totais refletem os dados persistidos
- **WHEN** a página inicial é carregada
- **THEN** o sistema exibe o número de seleções, o número de cidades-sede distintas e o número de jogos cadastrados no banco de dados, sem valores fictícios ou fixos no código

### Requirement: Próximos Jogos
A Landing Page SHALL exibir uma lista dos próximos jogos da fase de grupos, ordenados por data e horário crescente, incluindo as seleções envolvidas, o grupo, o estádio e a cidade.

#### Scenario: Lista ordenada dos próximos jogos
- **WHEN** a página inicial é carregada e existem jogos da fase de grupos cadastrados
- **THEN** o sistema exibe até um número limitado de jogos futuros da fase de grupos, ordenados da data/horário mais próximo para o mais distante, cada um mostrando as duas seleções, o grupo, o estádio e a cidade

#### Scenario: Ausência de próximos jogos
- **WHEN** não há jogos futuros cadastrados para a fase de grupos
- **THEN** o sistema exibe uma mensagem indicando que não há próximos jogos, sem apresentar erro

### Requirement: Ranking FIFA (Top 10)
A Landing Page SHALL exibir as 10 seleções mais bem colocadas no ranking FIFA, ordenadas pela posição, mostrando posição, nome da seleção e pontuação.

#### Scenario: Exibição das 10 melhores seleções
- **WHEN** a página inicial é carregada
- **THEN** o sistema exibe uma tabela e um gráfico de barras com as 10 seleções de melhor posição no ranking FIFA, ordenadas da posição 1 até a 10ª, cada uma com nome e pontuação

### Requirement: Gráfico do Ranking FIFA via Chart.js
A Landing Page SHALL renderizar o Top 10 do ranking FIFA como um gráfico de barras utilizando Chart.js, através de um componente de gráfico reutilizável por outras visualizações estatísticas do portal.

#### Scenario: Renderização do gráfico de barras
- **WHEN** a seção de Ranking FIFA é exibida na página inicial
- **THEN** o sistema renderiza um gráfico de barras com as 10 seleções e suas pontuações, atualizando o gráfico caso os dados exibidos mudem

### Requirement: Chamada para o Simulador
A Landing Page SHALL exibir uma chamada para ação (CTA) convidando o visitante a acessar o simulador, sem implementar a lógica de simulação nesta página.

#### Scenario: CTA de destaque para o simulador
- **WHEN** um visitante navega pela página inicial
- **THEN** o sistema exibe uma seção com uma chamada para ação que direciona à funcionalidade de simulador

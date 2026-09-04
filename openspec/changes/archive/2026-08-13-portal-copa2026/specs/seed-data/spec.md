## Purpose

Garantir que o banco de dados do PortalCopa26 seja populado automaticamente com os dados oficiais de referência da Copa (seleções, grupos, jogadores, jogos e ranking FIFA), para que a aplicação já esteja utilizável assim que iniciada.

## ADDED Requirements

### Requirement: Carga Inicial de Dados em Banco Vazio
O sistema SHALL popular automaticamente o banco de dados com seleções, grupos, jogadores, jogos e ranking FIFA quando o banco estiver vazio (sem esses dados).

#### Scenario: Primeira execução popula o banco
- **WHEN** a aplicação inicia e o banco de dados não contém seleções, grupos, jogadores, jogos ou ranking FIFA
- **THEN** o sistema carrega esses dados a partir do SeedData antes de a aplicação atender requisições

### Requirement: Seed Não Duplica Dados Existentes
O sistema SHALL evitar duplicar seleções, grupos, jogadores, jogos ou ranking FIFA já existentes no banco quando a aplicação for reiniciada.

#### Scenario: Reinício não duplica dados já carregados
- **WHEN** a aplicação é reiniciada e o banco de dados já contém os dados de seed
- **THEN** o sistema não insere registros duplicados de seleções, grupos, jogadores, jogos ou ranking FIFA

### Requirement: Consistência dos Dados Semeados
Os dados carregados pelo SeedData SHALL respeitar os relacionamentos do domínio: cada Seleção associada a um Grupo válido, cada Jogador associado a uma Seleção válida, e cada Jogo associado às Seleções (e Grupo, quando aplicável) corretos.

#### Scenario: Dados semeados mantêm integridade referencial
- **WHEN** o SeedData é executado
- **THEN** todas as Seleções, Jogadores e Jogos criados referenciam Grupos e Seleções existentes no mesmo processo de carga, sem referências órfãs

# data-persistence Specification

## Purpose

Definir como o PortalCopa26 persiste seus dados usando EF Core com SQLite, garantindo que o banco exista, esteja acessível pela aplicação e que simulações de usuários sobrevivam a reinicializações.

## Requirements

### Requirement: Contexto de Banco de Dados Único
O sistema SHALL expor um único contexto de banco de dados (DbContext) do EF Core, configurado para usar SQLite como provedor, responsável por todo o acesso a dados da aplicação.

#### Scenario: DbContext disponível para a aplicação
- **WHEN** a aplicação inicializa
- **THEN** o DbContext é registrado na injeção de dependência e pode ser resolvido por qualquer serviço que precise acessar dados

### Requirement: Criação Automática do Banco SQLite
O sistema SHALL garantir que o arquivo de banco de dados SQLite e seu schema existam antes de a aplicação atender requisições, criando-os automaticamente caso não existam.

#### Scenario: Primeira execução cria o banco
- **WHEN** a aplicação é executada pela primeira vez e o arquivo de banco de dados SQLite não existe
- **THEN** o sistema cria o arquivo de banco de dados e o schema necessário antes de responder a primeira requisição

#### Scenario: Execuções seguintes reutilizam o banco existente
- **WHEN** a aplicação é executada e o arquivo de banco de dados SQLite já existe com o schema atual
- **THEN** o sistema reutiliza o banco existente sem recriá-lo ou perder dados já armazenados

### Requirement: Persistência Duradoura de Simulações
Simulações de resultados criadas pelos usuários, incluindo os jogos simulados e as classificações geradas, SHALL ser persistidas no banco SQLite e SHALL continuar disponíveis após o encerramento e reinício da aplicação.

#### Scenario: Simulação sobrevive a um reinício da aplicação
- **WHEN** uma simulação é salva no banco de dados e a aplicação é reiniciada em seguida
- **THEN** a simulação e seus dados associados (jogos simulados e classificação) continuam recuperáveis a partir do banco de dados

### Requirement: Persistência Duradoura de Resultados Oficiais
Resultados oficiais de jogos da fase de grupos registrados na página de Grupos SHALL ser persistidos no banco SQLite, no próprio registro do jogo, e SHALL continuar disponíveis após o encerramento e reinício da aplicação.

#### Scenario: Resultado oficial sobrevive a um reinício da aplicação
- **WHEN** o placar oficial de um jogo é registrado e a aplicação é reiniciada em seguida
- **THEN** o placar oficial continua recuperável a partir do banco de dados, e a classificação oficial do grupo correspondente reflete esse resultado

## ADDED Requirements

### Requirement: Persistência Duradoura de Resultados Oficiais
Resultados oficiais de jogos da fase de grupos registrados na página de Grupos SHALL ser persistidos no banco SQLite, no próprio registro do jogo, e SHALL continuar disponíveis após o encerramento e reinício da aplicação.

#### Scenario: Resultado oficial sobrevive a um reinício da aplicação
- **WHEN** o placar oficial de um jogo é registrado e a aplicação é reiniciada em seguida
- **THEN** o placar oficial continua recuperável a partir do banco de dados, e a classificação oficial do grupo correspondente reflete esse resultado

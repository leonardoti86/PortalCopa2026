## 1. Solução e Projeto Base

- [x] 1.1 Criar a pasta `src/PortalCopa2026/` e a solução `PortalCopa2026.slnx`
- [x] 1.2 Criar o projeto Blazor Web App (.NET 10) `PortalCopa2026` dentro de `src/PortalCopa2026/PortalCopa2026/` e adicioná-lo à solução
- [x] 1.3 Criar as pastas `Pages`, `Components`, `Models`, `Services`, `Data` na raiz do projeto
- [x] 1.4 Criar subpastas `Models/Copa/` e `Models/Simulacao/` para agrupar as entidades por área
- [x] 1.5 Validar que o projeto compila e sobe com o layout padrão do template Blazor Web App (`dotnet run`)

## 2. EF Core e SQLite

- [x] 2.1 Adicionar os pacotes NuGet `Microsoft.EntityFrameworkCore.Sqlite` e `Microsoft.EntityFrameworkCore.Design`
- [x] 2.2 Configurar a connection string do SQLite (`Data Source=portalcopa26.db`) em `appsettings.json`
- [x] 2.3 Criar o `AppDbContext` em `Data/AppDbContext.cs`
- [x] 2.4 Registrar o `AppDbContext` na injeção de dependência em `Program.cs` via `AddDbContext` usando `UseSqlite`

## 3. Entidades de Domínio

- [x] 3.1 Criar a entidade `Grupo` (`Models/Copa/Grupo.cs`) com código e coleção de `Selecao`
- [x] 3.2 Criar a entidade `Selecao` (`Models/Copa/Selecao.cs`) com nome, código, técnico, `GrupoId`/`Grupo`, coleção de `Jogador` e referência a `RankingFifa`
- [x] 3.3 Criar a entidade `Jogador` (`Models/Copa/Jogador.cs`) com nome, posição, idade, gols, participação em copas e `SelecaoId`/`Selecao`
- [x] 3.4 Criar a entidade `Jogo` (`Models/Copa/Jogo.cs`) com data, horário, fase, cidade, estádio, `GrupoId` (nullable) e as duas seleções envolvidas (mandante/visitante)
- [x] 3.5 Criar a entidade `RankingFifa` (`Models/Copa/RankingFifa.cs`) com posição, pontos e `SelecaoId`/`Selecao`
- [x] 3.6 Criar a entidade `Simulacao` (`Models/Simulacao/Simulacao.cs`) com data de criação e coleção de `SimulacaoJogo`
- [x] 3.7 Criar a entidade `SimulacaoJogo` (`Models/Simulacao/SimulacaoJogo.cs`) com `SimulacaoId`, `JogoId`, placar simulado do mandante e do visitante
- [x] 3.8 Adicionar os `DbSet<T>` correspondentes a cada entidade no `AppDbContext`
- [x] 3.9 Configurar relacionamentos e restrições (chaves estrangeiras, obrigatoriedade, tipos de dado) via Fluent API em `OnModelCreating`

## 4. Migrations

- [x] 4.1 Criar a migration inicial (`dotnet ef migrations add InitialCreate`) cobrindo todas as entidades
- [x] 4.2 Configurar `Program.cs` para aplicar `Database.Migrate()` em um scope durante o startup, antes de `app.Run()`
- [x] 4.3 Validar que a aplicação cria o arquivo `portalcopa26.db` com o schema esperado na primeira execução
- [x] 4.4 Validar que uma segunda execução reutiliza o banco existente sem erros e sem recriar o schema

## 5. Seed Data

- [x] 5.1 Transcrever os dados de `copa2026/prototipo/data/groups.js` e `teams.js` para coleções C# de `Grupo`/`Selecao` em `Data/SeedData.cs`
- [x] 5.2 Transcrever os dados de `copa2026/prototipo/data/players.js` para `Jogador`, associando cada jogador à sua `Selecao`
- [x] 5.3 Transcrever os dados de `copa2026/prototipo/data/matches.js` para `Jogo`, associando cidade/estádio, grupo (quando aplicável) e as duas seleções
- [x] 5.4 Transcrever os dados de `copa2026/prototipo/data/ranking.js` para `RankingFifa`, associando cada posição à `Selecao` correspondente
- [x] 5.5 Implementar a rotina de seed idempotente (`if (!context.Selecoes.Any())`) que insere todos os dados acima em uma única operação
- [x] 5.6 Chamar a rotina de seed a partir do mesmo scope de startup usado para `Database.Migrate()`, após a migração ser aplicada
- [x] 5.7 Validar que o número de seleções, grupos, jogadores e jogos semeados corresponde às contagens dos arquivos-fonte do protótipo

## 6. Preparação para Funcionalidades Futuras

- [x] 6.1 Confirmar que o layout base (`Components/Layout`) e o roteamento padrão do Blazor Web App estão funcionais e prontos para receber novas páginas
- [x] 6.2 Documentar no `README` do projeto (ou comentário mínimo em `Program.cs`) como registrar novos serviços na DI nativa, para orientar as próximas specs (landing page, jogos, grupos, seleções, ranking, simulador)

## 7. Verificação Final

- [x] 7.1 Rodar `dotnet build` na solução e confirmar que não há erros ou warnings de compilação
- [x] 7.2 Rodar a aplicação (`dotnet run`) e confirmar que ela inicia, aplica as migrations, popula o banco e serve a página padrão sem erros
- [x] 7.3 Confirmar manualmente (via ferramenta de inspeção SQLite) que as tabelas e os dados semeados estão presentes no arquivo `portalcopa26.db`

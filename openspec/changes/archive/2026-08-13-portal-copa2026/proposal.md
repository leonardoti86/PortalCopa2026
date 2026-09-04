## Why

O PortalCopa26 precisa de uma fundação técnica antes que qualquer página ou funcionalidade possa ser construída. Já existe um protótipo HTML validado (`copa2026/prototipo`) que define o domínio (seleções, grupos, jogos, jogadores, ranking FIFA) e um `CLAUDE.md` com as decisões arquiteturais (Blazor Web App em .NET 10, EF Core, SQLite, projeto único). Sem a solução, o `DbContext`, as entidades e o SeedData criados de forma consistente, nenhuma das páginas do escopo maior (landing page, jogos, grupos, seleções, ranking, simulador) pode começar a ser implementada.

## What Changes

- Criar a solução `PortalCopa2026` no formato `.slnx` dentro de `src/PortalCopa2026/`, contendo o projeto Blazor Web App (.NET 10).
- Configurar o projeto Blazor Web App com a estrutura de pastas prevista (`Pages`, `Components`, `Models`, `Services`, `Data`), organizada para permitir futura migração para arquitetura em camadas sem grandes alterações.
- Adicionar e configurar EF Core com o provider SQLite.
- Criar o `AppDbContext` (ou nome equivalente) representando o acesso a dados da aplicação.
- Criar as entidades iniciais do domínio: `Grupo`, `Selecao`, `Jogador`, `Jogo`, `RankingFifa`, `Simulacao`, `SimulacaoJogo`, com seus relacionamentos mínimos necessários (grupo→seleções, seleção→jogadores, jogo→seleções/grupo/estádio, simulação→jogos simulados/classificação).
- Configurar a criação/migração do banco SQLite na inicialização da aplicação.
- Implementar SeedData para popular seleções, grupos, jogadores, jogos e ranking FIFA a partir dos dados de referência do protótipo (`copa2026/prototipo/data`).
- Registrar os serviços necessários (DbContext, serviços de acesso a dados) no container de injeção de dependência nativo do ASP.NET Core.
- Preparar a aplicação (layout base, roteamento inicial, configuração de projeto) para receber as futuras funcionalidades (landing page, listagens, ranking com Chart.js, simulador), sem implementá-las nesta especificação.

## Capabilities

### New Capabilities
- `app-foundation`: Estrutura da solução Blazor Web App, configuração de projeto e preparação para camadas futuras.
- `data-persistence`: Configuração do EF Core + SQLite, `DbContext` e migrações/criação do banco.
- `domain-model`: Entidades do domínio da Copa (Grupo, Seleção, Jogador, Jogo, RankingFifa, Simulacao, SimulacaoJogo) e seus relacionamentos.
- `seed-data`: Carga inicial de dados (seleções, grupos, jogadores, jogos, ranking FIFA) via SeedData.

### Modified Capabilities
- Nenhuma (projeto novo, sem specs existentes).

## Impact

- **Código**: novo diretório `src/PortalCopa2026/` com a solução `.slnx` e o projeto Blazor Web App; nenhum código de página/feature de negócio é implementado.
- **Dependências**: pacotes NuGet do EF Core (`Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`).
- **Dados**: criação do arquivo de banco SQLite local e da lógica de SeedData que consome os dados de referência do protótipo.
- **Fora do escopo**: implementação de LandingPage, páginas de Jogos/Grupos/Seleções, Ranking FIFA com Chart.js, Simulador, integração com APIs externas, área administrativa e autenticação.

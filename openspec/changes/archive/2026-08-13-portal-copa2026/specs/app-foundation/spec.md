## Purpose

Define a estrutura base do PortalCopa26 como um Blazor Web App em .NET 10, organizada de forma que futuras funcionalidades e uma eventual migração para arquitetura em camadas possam ser adotadas sem grandes reescritas.

## ADDED Requirements

### Requirement: Solução e Projeto Blazor Web App
O sistema SHALL ser disponibilizado como uma solução `.slnx` contendo um único projeto Blazor Web App (.NET 10), localizada em `src/PortalCopa2026/`.

#### Scenario: Aplicação inicia com sucesso
- **WHEN** a aplicação é executada
- **THEN** o servidor sobe sem erros e responde na página inicial com status HTTP 200

### Requirement: Organização de Código Preparada para Camadas
O código-fonte SHALL ser organizado nas pastas `Pages`, `Components`, `Models`, `Services` e `Data`, de modo que uma futura migração para uma arquitetura em camadas não exija reestruturação ampla do código existente.

#### Scenario: Estrutura de pastas presente no projeto
- **WHEN** o projeto é inspecionado
- **THEN** as pastas `Pages`, `Components`, `Models`, `Services` e `Data` existem na raiz do projeto Blazor

### Requirement: Injeção de Dependência Nativa
O sistema SHALL registrar seus serviços de acesso a dados através do mecanismo de injeção de dependência nativo do ASP.NET Core, sem depender de containers de terceiros.

#### Scenario: Serviços resolvidos via DI nativo
- **WHEN** a aplicação inicializa e um componente solicita um serviço de dados (ex.: contexto de banco de dados)
- **THEN** o serviço é resolvido pelo container de DI nativo configurado em `Program.cs`, sem erro de resolução de dependência

### Requirement: Preparação para Funcionalidades Futuras
A fundação da aplicação SHALL fornecer layout base e roteamento inicial suficientes para que páginas futuras (landing page, jogos, grupos, seleções, ranking, simulador) possam ser adicionadas sem alterar a configuração de projeto, DbContext ou entidades criadas nesta especificação.

#### Scenario: Nova página pode ser adicionada sem alterar a fundação
- **WHEN** uma nova página Blazor é adicionada em `Pages`
- **THEN** ela é servida pelo roteamento existente sem exigir mudanças em `Program.cs`, no `DbContext` ou nas entidades de domínio

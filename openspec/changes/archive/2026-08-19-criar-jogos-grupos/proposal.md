## Why

O PortalCopa26 ainda não tem uma página dedicada à listagem de jogos. A Landing Page só exibe um recorte dos próximos confrontos; torcedores precisam de uma tela própria para consultar todos os jogos da fase de grupos, agrupados por data, com filtro por grupo e acesso rápido à página de Grupos.

## What Changes

- Criar a página `/jogos` com listagem completa dos jogos da fase de grupos, usando exclusivamente os dados oficiais já carregados via Seed Data (sem dados fictícios).
- Agrupar os jogos por data e ordenar por data e horário crescente.
- Permitir filtrar a listagem por grupo (A–L).
- Exibir, em cada jogo, as informações do grupo e do estádio (nome do estádio e cidade).
- Adicionar um botão "Ver Grupos" que direciona o usuário para a página de Grupos.
- Criar os componentes reutilizáveis `Jogos.razor`, `JogosFiltro.razor`, `JogosDataHeader.razor` e `JogoCard.razor`.
- Criar um serviço próprio (`JogosService`) para acesso aos dados via `AppDbContext`, seguindo a diretriz de não acessar o `DbContext` diretamente em páginas/componentes.

## Capabilities

### New Capabilities
- `jogos`: Página e componentes de listagem de jogos da fase de grupos — agrupamento por data, ordenação por data/horário, filtro por grupo, exibição de grupo e estádio, e navegação para a página de Grupos.

### Modified Capabilities

## Impact

- Novo componente de página `Components/Pages/Jogos/Jogos.razor` e subcomponentes `JogosFiltro.razor`, `JogosDataHeader.razor`, `JogoCard.razor`.
- Novo serviço `Services/Jogos/JogosService.cs` (+ interface `IJogosService` e DTOs) registrado via injeção de dependência nativa.
- Nova rota `/jogos` (leitura apenas; sem alterações no `AppDbContext`, nas Migrations ou no Seed Data).
- O botão "Ver Grupos" aponta para a rota `/grupos`; a implementação da página de Grupos em si está fora do escopo desta change.

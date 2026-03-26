# API Football Scout

Backend desenvolvido para centralizar e processar dados de scouting futebolístico. A API integra dados em tempo real do SofaScore, permitindo a análise de atletas, gestão de destaques e comparação de métricas de desempenho.

O foco principal do projeto foi a criação de uma arquitetura resiliente, utilizando cache distribuído para otimizar o consumo de recursos externos e persistência NoSQL para alta disponibilidade de informações.

## Stack Técnica

* **Runtime:** .NET 9 (C# 12)
* **Orquestração:** .NET Aspire (Gerenciamento de containers e Service Discovery)
* **Comunicação:** Refit para consumo de API REST (Type-safe HTTP client)
* **Cache:** Redis (IDistributedCache)
* **Persistência:** MongoDB via Entity Framework Core (Provider oficial)

---

## Arquitetura e Decisões de Projeto

### Integração e Performance
A comunicação com a API do SofaScore é feita via **Refit**, garantindo contratos de dados fortemente tipados e facilitando a manutenção. Para evitar o overhead de rede e reduzir o consumo de créditos da API externa, implementamos uma estratégia de **Targeted Caching** no Redis:
* **Dados Estáticos:** Ligas e Torneios possuem maior tempo de vida no cache.
* **Dados Dinâmicos:** Perfis de jogadores são invalidados ou atualizados conforme a demanda, garantindo fluidez no consumo pelo frontend.

### Persistência e Histórico (FIFO)
Utilizamos o MongoDB para armazenar o histórico de buscas e perfis consultados. Foi implementado um algoritmo **FIFO (First-In, First-Out)** que limita o histórico aos últimos 10 jogadores visualizados.
* A lógica utiliza `Upsert` para evitar duplicidade de registros.
* Limpeza automática de registros excedentes via `RemoveRange` no contexto do EF Core, mantendo o banco otimizado.

### Infraestrutura com .NET Aspire
Toda a infraestrutura de suporte (banco de dados e cache) é orquestrada pelo **.NET Aspire**. Isso permite que o ambiente de desenvolvimento seja espelhado via Docker sem a necessidade de scripts manuais ou configurações externas de banco.

---

## Como Rodar o Projeto

O projeto depende do **Docker Desktop** e do **SDK do .NET 9**.

1.  **Clone o repositório.**
2.  **Configure a chave de API do SofaScore:**
    ```bash
    dotnet user-secrets set "SofaScoreKey" "SUA_CHAVE_AQUI" --project APIFootballScout.AppHost
    ```
3.  **Inicie o projeto via AppHost:**
    ```bash
    dotnet run --project APIFootballScout.AppHost
    ```
4.  **Acesso:** O Dashboard do Aspire abrirá no navegador, fornecendo acesso aos logs estruturados e à documentação Swagger da API.

---

## Roadmap

- [x] **V1.0:** Integração Core, Cache Redis e Histórico MongoDB.
- [ ] **V1.1:** **Scouting Reporter** — Módulo para geração de relatórios analíticos automáticos baseados em ratings de temporada.
- [ ] **V1.2:** **Shortlists** — Sistema de pastas para organização de alvos de transferência.
- [ ] **V1.3:** **Comparison Engine** — Endpoint para comparação direta (Head-to-Head) entre atletas.

---
**Desenvolvido por Leonardo dos Santos Mendes Ferreira**

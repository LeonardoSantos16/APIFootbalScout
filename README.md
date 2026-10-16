# API Football Scout

Backend de apoio ao trabalho do olheiro. A API consome dados de jogadores e torneios do SofaScore e, sobre eles, permite acompanhar jogadores, escrever relatórios de scouting, montar shortlists priorizadas e comparar métricas de desempenho.

## Stack Técnica

* **Runtime:** .NET 10 (C# 14)
* **Orquestração:** .NET Aspire (Gerenciamento de containers e Service Discovery)
* **Comunicação:** Refit para consumo de API REST (Type-safe HTTP client)
* **Cache:** Redis (IDistributedCache)
* **Persistência:** MongoDB, com o driver oficial nos repositórios dos agregados
* **Autenticação:** JWT com refresh token rotativo e senhas com BCrypt
* **Documentação da API:** OpenAPI nativo do ASP.NET Core com Swagger UI

---

## Arquitetura e Decisões de Projeto

### Domínio
O código segue DDD, com o domínio separado em contextos (Acompanhamento, Avaliação, Priorização, Análise e Catálogo de Jogador). As funcionalidades implementadas são:

* **F1 — Acompanhamento e dossiê:** o olheiro marca um jogador e o sistema guarda uma linha de base do estado dele naquele instante.
* **F2 — Detecção de mudança:** na consulta, compara a linha de base com o dado atual (clube, valor de mercado e minutagem).
* **F5 — Relatório de scouting:** rascunho editável e relatório finalizado imutável, corrigido por emissão de um novo relatório.
* **F7 — Shortlist priorizada:** lista ordenada de alvos de transferência, com limite de posições e prioridade sem empates.
* **F9 — Métricas por 90 minutos:** normalização de estatísticas, recusada quando a amostra de minutos é insuficiente.
* **F10 — Comparação direta:** head-to-head entre jogadores de posições compatíveis no mesmo recorte temporal.

A documentação do domínio está em `docs/`: [features](docs/features.md), [regras de negócio](docs/regras-de-negocio.md) e [mapeamento do modelo de domínio](docs/Mapeamento%20do%20Modelo%20de%20Domínio.md). As decisões de arquitetura ficam registradas em [`docs/adr/`](docs/adr/).

### Integração e Performance
A comunicação com a API do SofaScore é feita via **Refit**, garantindo contratos de dados fortemente tipados, e passa por uma anticorruption layer que traduz os dados externos para o modelo de domínio. Para reduzir o consumo de créditos da API externa, as respostas são guardadas no Redis com tempo de vida conforme o tipo de dado:
* **Jogadores e torneios principais** (listas configuradas em `ScoutConfig`): de 1 a 20 dias no cache, conforme o dado.
* **Perfil completo do jogador e página completa do torneio:** 12 horas no cache, para qualquer jogador ou torneio.
* **Demais jogadores e torneios:** consultados direto na fonte, exceto a lista de temporadas de um torneio, que fica 2 horas no cache.

O motivo de o cache valer só para as listas principais está no [ADR 0003](docs/adr/0003-cache-seletivo-da-fonte-externa.md).

### Infraestrutura com .NET Aspire
Toda a infraestrutura de suporte (banco de dados e cache) é orquestrada pelo **.NET Aspire**. Isso permite que o ambiente de desenvolvimento seja espelhado via Docker sem a necessidade de scripts manuais ou configurações externas de banco.

---

## Como Rodar o Projeto

O projeto depende do **Docker Desktop** e do **SDK do .NET 10**.

1.  **Clone o repositório.**
2.  **Configure os três segredos do AppHost:**
    ```bash
    dotnet user-secrets set "Parameters:SofaScoreKey" "SUA_CHAVE_DO_SOFASCORE" --project APIFootballScout.AppHost
    dotnet user-secrets set "Parameters:JwtSigningKey" "CHAVE_COM_PELO_MENOS_32_BYTES" --project APIFootballScout.AppHost
    dotnet user-secrets set "Parameters:PasswordPepper" "VALOR_ALEATORIO" --project APIFootballScout.AppHost
    ```
    * `SofaScoreKey`: chave da API do SofaScore.
    * `JwtSigningKey`: chave de assinatura dos tokens JWT (HMAC-SHA256), com no mínimo 32 bytes.
    * `PasswordPepper`: valor secreto combinado às senhas antes do hash. Trocá-lo invalida as senhas já cadastradas (ver [ADR 0002](docs/adr/0002-pepper-nas-senhas-e-refresh-token-rotativo.md)).
3.  **Inicie o projeto via AppHost:**
    ```bash
    dotnet run --project APIFootballScout.AppHost
    ```
4.  **Acesso:** O Dashboard do Aspire abrirá no navegador, com os logs estruturados e o link da API. A documentação fica em `/swagger`, e o contrato OpenAPI em `/openapi/v1.json`.
5.  **Testes:**
    ```bash
    dotnet test
    ```

---

## Qualidade e CI

Todo push e pull request para `master` e `develop` passa pelos workflows do GitHub Actions:

* **build and test** (`.github/workflows/build-and-test.yml`)
    * `build-and-test`: verifica a formatação, compila com warnings tratados como erro (regras em `Directory.Build.props` e `.editorconfig`), roda os testes e publica o relatório de cobertura no resumo do job.
    * `docker-build`: roda depois de `build-and-test` e constrói a imagem da API a partir do `APIFootballScout/Dockerfile`. Em pull requests e na `develop` a imagem só é validada; em push na `master` ela é publicada em `ghcr.io/leonardosantos16/apifootballscout` com a tag `sha-<commit>`. Em push de uma tag git `vX.Y.Z`, a imagem recebe também as tags `X.Y.Z`, `X.Y`, `X` e `latest`, que aponta sempre para a última release.
* **codeql** (`.github/workflows/codeql.yml`): análise de segurança do código C# e dos workflows.
* **Dependabot** (`.github/dependabot.yml`): atualizações mensais de pacotes NuGet, GitHub Actions e imagens Docker, abertas contra a `develop`.

### Rodando as verificações localmente

Antes do push, os mesmos checks podem ser reproduzidos na raiz do repositório:

```bash
dotnet format whitespace --verify-no-changes
dotnet format style --verify-no-changes
dotnet build --configuration Release
./coverage.ps1
docker build -f APIFootballScout/Dockerfile -t apifootballscout:local .
```

* `dotnet format` sem `--verify-no-changes` aplica as correções de formatação.
* `coverage.ps1` roda os testes e gera o relatório HTML em `coveragereport/` (requer `dotnet tool install -g dotnet-reportgenerator-globaltool`).

### Checks obrigatórios

A proteção das branches exige os checks `build-and-test` e `docker-build`. Eles são identificados pelo nome do job, então renomear um job no workflow exige atualizar a regra de proteção no GitHub; caso contrário, os pull requests ficam aguardando um check que nunca será reportado.

---

## Roadmap

- [x] **Integração:** consumo da API do SofaScore com cache no Redis.
- [x] **Autenticação:** cadastro, login, refresh token rotativo e encerramento de sessões.
- [x] **Acompanhamento e detecção de mudança** (F1 e F2).
- [x] **Relatório de scouting** (F5).
- [x] **Shortlist priorizada** (F7).
- [x] **Métricas por 90 minutos e comparação direta** (F9 e F10).
- [ ] **Histórico de jogadores consultados:** registro dos últimos jogadores vistos por olheiro. Hoje só existe a gravação do jogador na primeira consulta do perfil, sem limite e sem leitura.

---
**Desenvolvido por Leonardo dos Santos Mendes Ferreira**

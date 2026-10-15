# Termos do domínio em português, vocabulário técnico em inglês

Os nomes no código seguem a linguagem ubíqua do DDD: todo termo de domínio (`Dossie`, `Relatorio`, `Shortlist`, `Olheiro`, `Parecer`, `AbrirRascunho`) fica em português, como aparece no [Mapeamento do Modelo de Domínio](../Mapeamento%20do%20Modelo%20de%20Domínio.md), em qualquer camada onde apareça, e não só em `Domain/`. O vocabulário técnico, que não pertence ao domínio, fica em inglês: sufixos e padrões (`UseCase`, `Repository`, `Controller`, `Mapper`, `Document`, `Request`, `Result`, `Specification`), infraestrutura, autenticação (`SignInUserUseCase`, `TokenService`) e os DTOs da fonte externa. Assim, um nome como `AbrirRascunhoRelatorioUseCase` ou `RelatorioRepositoryMongo` junta as duas partes de propósito.

## Considered Options

- **Tudo em inglês.** Rejeitado: exigiria traduzir a linguagem ubíqua (`Dossie` → `Dossier`, `Olheiro` → `Scout`), e o glossário registra justamente termos em inglês como sinônimos a evitar (*scout*, *snapshot*, *watchlist*).
- **Português só dentro de `Domain/`, inglês no resto.** Rejeitado: os casos de uso e controllers passariam a usar outro vocabulário para os mesmos conceitos (`OpenReportDraftUseCase` chamando `Relatorio.AbrirRascunho`), quebrando a linguagem ubíqua na fronteira entre as camadas.

## Consequences

- A regra de nomenclatura do build (IDE1006) só verifica o formato dos nomes, não o idioma; esta convenção é verificada na revisão.
- Nomes que ainda não seguem a convenção: `HelperObterColecao`, `SofascoreTradutor` e `EnumSemNuloSchemaTransformer` (vocabulário técnico em português) e `FonteDeDadosSofascore` (adaptador que implementa a porta `ICatalogoDeJogador`). O sufixo `DTO` também aparece misturado com `Dto` nos DTOs do Sofascore.

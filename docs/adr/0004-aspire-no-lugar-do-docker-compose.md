# .NET Aspire no lugar do Docker Compose no ambiente de desenvolvimento

O ambiente local é orquestrado pelo AppHost do .NET Aspire (`APIFootballScout.AppHost/AppHost.cs`), não por um `docker-compose.yml`. O AppHost declara o MongoDB e o Redis como containers, sobe a API depois deles e passa a ela as connection strings e os segredos (`SofaScoreKey`, `JwtSigningKey`, `PasswordPepper`) como parâmetros. Com isso, a topologia fica descrita em C#, junto da solução, e o desenvolvimento ganha o dashboard de logs, traces e métricas que os ServiceDefaults alimentam, coisa que o compose não oferece.

## Considered Options

- **Docker Compose com Mongo, Redis e a API.** Rejeitado: as connection strings e os segredos teriam que ser repetidos à mão em YAML e num `.env`, e o dashboard de observabilidade ficaria de fora.
- **Compose só para Mongo e Redis, com a API rodando pela IDE.** Rejeitado: a configuração da API ficaria dividida entre o compose, o `launchSettings.json` e os user secrets.
- **Manter o AppHost e um compose escrito à mão em paralelo.** Rejeitado: seriam duas descrições da mesma topologia, que tendem a divergir.

## Consequences

- No desenvolvimento, a API roda como processo .NET a partir do código, e não a partir da imagem do `APIFootballScout/Dockerfile`. A imagem construída no CI não é exercitada localmente pelo AppHost.
- Para rodar fora do Aspire, a imagem precisa receber por variável de ambiente o que o AppHost injeta hoje: `ConnectionStrings__mongodb`, `ConnectionStrings__scoutdb`, `ConnectionStrings__cache`, `Jwt__Key`, `SofaScore__ApiKey` e `Password__Pepper`.
- Rodar o projeto exige o SDK do .NET além do Docker. Só o Docker não basta.
- Um `docker-compose.yml` escrito à mão pode existir só para rodar a imagem publicada com o Mongo e o Redis e verificar que ela sobe apenas com variáveis de ambiente. Ele não substitui o AppHost no desenvolvimento.

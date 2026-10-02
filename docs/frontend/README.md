# Guia do Front-end — APIFootballScout

Documentacao destinada a quem vai construir a interface. Endpoints, payloads e enums nao estao aqui: eles vem do contrato OpenAPI gerado pela propria API. Estes documentos cobrem o que o contrato nao expressa — como os erros devem ser tratados, como as regras de negocio afetam a tela e os valores de configuracao que a validacao do front replica.

## Indice

| Documento | Conteudo |
| --- | --- |
| [erros.md](erros.md) | Formato `ProblemDetails`, tabela de codigos de erro e mensagem sugerida |
| [fluxos-de-tela.md](fluxos-de-tela.md) | Jornadas por feature, sessao e refresh de token, estados de tela e regras de UX derivadas do dominio |

## Contrato da API

O contrato OpenAPI e a fonte da verdade para rotas, requests, responses e enums. Ele fica exposto em todos os ambientes, inclusive no deploy:

| Recurso | Rota |
| --- | --- |
| Swagger UI | `/swagger` |
| Documento OpenAPI (3.1) | `/openapi/v1.json` |

Nao escreva tipos a mao: gere o client a partir do documento e rode a geracao de novo sempre que o backend mudar. Exemplo com `orval`, que gera tipos e hooks do TanStack Query:

```ts
// orval.config.ts
export default {
  scout: {
    input: 'https://localhost:7163/openapi/v1.json',
    output: { target: 'src/api/scout.ts', client: 'react-query' },
  },
};
```

```json
"scripts": { "gen:api": "orval" }
```

Se preferir algo mais leve, `openapi-typescript` + `openapi-fetch` geram so os tipos e um fetch tipado.

Para testar na mao (Postman, Insomnia, Bruno), importe o mesmo documento em vez de manter uma colecao separada.

### O que o contrato nao mostra

**Payload do SofaScore.** `Players` e `Tournament` devolvem o payload da fonte sem traducao, e o formato muda conforme a fonte. Use os tipos gerados so para os campos que a tela usa. `entity.position` na busca e o codigo curto da fonte (`"M"`, `"F"`, `"D"`, `"G"`) e nao e o mesmo enum `Posicao` usado nos endpoints de analise.

**Valores de configuracao.** Vem de `appsettings.json`, secao `ScoutConfig`. Nenhum endpoint os expoe, entao se mudarem no backend a validacao do front precisa acompanhar.

| Chave | Valor | Efeito na tela |
| --- | --- | --- |
| `LimiteObservacoesJogadores` | 10 | teto de acompanhamentos ativos |
| `LimiarMinutagemMinutos` | 180 | mudanca de minutagem relevante |
| `LimiarValorDeMercadoPercentual` | 10 | mudanca de valor relevante |
| `MinimoDePros` | 2 | minimo de pontos positivos para finalizar |
| `MinimoDeContras` | 2 | minimo de pontos negativos para finalizar |
| `MinimoDeCaracteresDaObservacao` | 20 | soma dos caracteres dos pontos |
| `LimiteDeAlvosDaShortlist` | 25 | teto de alvos (tambem vem em `limiteDeAlvos`) |
| `AmostraMinimaDeMinutos` | 450 | minimo para uma metrica ser calculada |
| `PrincipaisTorneios` | 7, 8, 16, 17, 23, 35, 325 | destaques/sugestoes |

`limiteDeAlvos` chega na propria resposta da shortlist — prefira o valor da resposta a um literal no codigo.

**Tempo de vida dos tokens.** Access token de 15 minutos e refresh token de 7 dias (`appsettings.json`, secao `Jwt`). O fluxo de renovacao esta em [fluxos-de-tela.md](fluxos-de-tela.md) e a regra do `401` em [erros.md](erros.md).

## Base URL e ambiente

| Perfil | URL |
| --- | --- |
| `http` | `http://localhost:5273` |
| `https` | `https://localhost:7163` |
| Container | `http://localhost:8080` / `https://localhost:8081` |

O prefixo de todas as rotas e `/api`. A API redireciona HTTP para HTTPS
(`UseHttpsRedirection`), entao em desenvolvimento prefira apontar o front para o perfil
`https` para evitar um redirect em cada chamada.

Subir a stack completa (Mongo + Redis + API) depende do Docker Desktop e do .NET Aspire:
`dotnet run --project APIFootballScout.AppHost`.

## Convencoes de serializacao

- **Enums viajam como string.** Todos os enums dos contratos usam `JsonStringEnumConverter`, entao o valor no JSON e `"Clube"`, `"Rascunho"`,
  `"Contratar"` — nunca o numero. O contrato ja publica cada enum como union de strings.
- **Campos opcionais somem do JSON.** As respostas usam `JsonIgnore(WhenWritingNull)`; uma propriedade ausente significa "nao se aplica".
- **Datas sao ISO 8601.** `DateTime` em UTC nas respostas de acompanhamento/auth; `DateTimeOffset` com offset nos relatorios.
- **Rotas sao case-insensitive.** `/api/analise` e `/api/Analise` funcionam igual.

## Identidade do olheiro

Nenhum endpoint recebe o id do olheiro no corpo ou na URL: ele sai sempre do claim `sub` do access token. Relatorios, shortlists e dossies sao automaticamente escopados ao usuario autenticado — um recurso de outro olheiro responde `404`, nunca `403`.

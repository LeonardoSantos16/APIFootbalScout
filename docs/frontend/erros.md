# Erros

## Formato

Erros de dominio passam pelo `ExceptionHandlerGlobal` e chegam como
`application/problem+json`:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.21",
  "title": "Business rule violation",
  "status": 422,
  "detail": "O jogador já está presente na shortlist.",
  "instance": "POST /api/Shortlist/a9d0.../alvos",
  "code": "shortlist.jogador_ja_na_lista"
}
```

A extensao **`code`** e o contrato estavel para o front: e ela que deve decidir a mensagem e o comportamento da tela. `detail` e texto de diagnostico (parte em portugues, parte em ingles) e nao serve para exibir ao usuario.

Quando `code` **nao** vem, o erro e desconhecido pela API: `detail` vira `"An unexpected error occurred while processing the request."` e o status e `500`. Nesse caso mostre uma mensagem generica e registre o `instance`.

## Mapa de status

| Status | Title | Origem |
| --- | --- | --- |
| 400 | `Invalid request` | `ValorInvalidoException` ou validacao de `DataAnnotations` |
| 401 | `Unauthorized` | `NaoAutenticadoException` ou token ausente/expirado |
| 404 | `Resource not found` | `RecursoNaoEncontradoException` |
| 409 | `Operation conflict` | `ConflitoDeDominioException` |
| 422 | `Business rule violation` | `RegraDeNegocioException` |
| 500 | `Internal Server Error` | qualquer excecao nao mapeada |
| 502 | `External data source unavailable` | `FonteExternaIndisponivelException` (SofaScore) |

Leitura pratica da diferenca entre 409 e 422: **409** e um conflito com o estado atual do recurso (ja existe, ja esta finalizado); **422** e uma regra de negocio que o pedido violou (limite, conteudo minimo, ordem).

### Dois formatos de erro convivem

1. **`ProblemDetails`** — todos os endpoints de dominio (Auth, Acompanhamento, Analise,Relatorio, Shortlist).
2. **Texto simples** — `PlayersController` e `TournamentController` usam
   `BadRequest("Name is required")` e `NotFound("Player with ID 12 not found")`, que saem
   como string pura. O interceptor do front precisa tolerar um corpo que nao e JSON.

Erros de validacao de modelo (`[Range]`, `[Required]`, `[EmailAddress]`) sao gerados pelo proprio ASP.NET antes do controller, como `ValidationProblemDetails` — mesmo formato, com um `errors` adicional por campo e **sem** `code`:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "Password": ["The field Password must be a string with a minimum length of 8..."] }
}
```

## Tabela de codigos

### Autenticacao

| Codigo | Status | Mensagem sugerida |
| --- | --- | --- |
| `usuario.email_ja_cadastrado` | 409 | Este e-mail ja esta cadastrado. |
| `usuario.credenciais_invalidas` | 401 | E-mail ou senha incorretos. |
| `usuario.nao_encontrado` | 404 | Usuario nao encontrado. |
| `usuario.senha_atual_invalida` | 401 | A senha atual esta incorreta. |
| `usuario.senha_igual_a_atual` | 422 | A nova senha precisa ser diferente da atual. |
| `usuario.refresh_token_invalido` | 401 | Sua sessao expirou. Entre novamente. |
| `usuario.token_sem_identificacao` | 401 | Sessao invalida. Entre novamente. |

### Acompanhamento

| Codigo | Status | Mensagem sugerida |
| --- | --- | --- |
| `acompanhamento.jogador_ja_acompanhado` | 409 | Voce ja acompanha este jogador. |
| `acompanhamento.limite_atingido` | 422 | Limite de 10 acompanhamentos atingido. Encerre um para abrir outro. |
| `acompanhamento.dossie_nao_encontrado` | 404 | Nenhum acompanhamento ativo para este jogador. |
| `jogador.perfil_nao_encontrado` | 404 | Nao ha dados deste jogador para a competicao/temporada escolhida. |
| `jogador.informacoes_insuficientes` | 422 | O jogador nao tem dados suficientes para servir de linha de base. |
| `jogador.estatisticas_nao_encontradas` | 404 | Sem estatisticas para este recorte. |
| `dossie.ja_encerrado` | 409 | Este acompanhamento ja foi encerrado. |
| `dossie.encerrado_somente_leitura` | 409 | Acompanhamento encerrado nao pode ser alterado. |

### Relatorio

| Codigo | Status | Mensagem sugerida |
| --- | --- | --- |
| `relatorio.nao_encontrado` | 404 | Relatorio nao encontrado. |
| `relatorio.jogador_invalido` | 409 | Jogador invalido. |
| `relatorio.observacao_futura` | 409 | A data da observacao nao pode estar no futuro. |
| `relatorio.texto_obrigatorio` | 400 | O texto do relatorio e obrigatorio. |
| `relatorio.ja_finalizado` | 409 | Relatorio finalizado nao pode ser editado. Crie uma correcao. |
| `relatorio.correcao_de_rascunho` | 409 | So relatorio finalizado pode ser corrigido. |
| `relatorio.conclusao_ausente` | 422 | Preencha nota e parecer antes de finalizar. |
| `relatorio.conteudo_minimo_nao_atendido` | 422 | Inclua ao menos 2 pontos positivos e 2 negativos (20 caracteres no total). |
| `relatorio.parecer_invalido` | 400 | Parecer invalido. |
| `nota.fora_da_faixa` | 400 | A nota deve estar entre 0 e 10. |
| `nota.precisao_invalida` | 400 | A nota aceita no maximo uma casa decimal. |

### Shortlist

| Codigo | Status | Mensagem sugerida |
| --- | --- | --- |
| `shortlist.nao_encontrada` | 404 | Shortlist nao encontrada. |
| `shortlist.alvo_nao_encontrado` | 404 | Este jogador nao esta na shortlist. |
| `shortlist.jogador_ja_na_lista` | 422 | Este jogador ja esta na shortlist. |
| `shortlist.limite_de_alvos_atingido` | 422 | A shortlist atingiu o limite de 25 alvos. |
| `shortlist.prioridade_fora_da_ordem` | 422 | Prioridade fora da ordem da lista. |
| `shortlist.moeda_divergente` | 422 | Todos os custos da shortlist precisam usar a mesma moeda. |
| `prioridade.nao_positiva` | 400 | A prioridade deve ser maior que zero. |
| `dinheiro.moedas_distintas` | 400 | Nao e possivel somar valores em moedas diferentes. |

### Analise e recorte

| Codigo | Status | Mensagem sugerida |
| --- | --- | --- |
| `comparacao.jogador_consigo_mesmo` | 400 | Escolha dois jogadores diferentes. |
| `recorte.contexto_invalido` | 400 | Contexto invalido. Use `Clube` ou `Selecao`. |
| `posicao.invalida` | 400 | Posicao desconhecida. |
| `conjunto_de_estatisticas.vazio` | 400 | Sem estatisticas para este recorte. |
| `conjunto_de_estatisticas.recorte_divergente` | 400 | Estatisticas de recortes diferentes. |
| `conjunto_de_estatisticas.minutagem_divergente` | 400 | Minutagem inconsistente na fonte. |
| `janela_da_comparacao.intervalo_invalido` | 400 | Intervalo de comparacao invalido. |

Os codigos `atributo.*`, `afericao.*`, `estatistica_acumulavel.*`, `valor_derivado.*` e `comparacao.motivo_invalido` sinalizam inconsistencia interna de mapeamento. Nao devem aparecer em uso normal — trate como erro generico e reporte.

### Fonte externa (SofaScore) — 502

| Codigo | Mensagem sugerida |
| --- | --- |
| `sofascore.perfil_do_jogador_indisponivel` | Nao foi possivel carregar o perfil agora. |
| `sofascore.detalhes_do_jogador_indisponiveis` | Dados do jogador indisponiveis no momento. |
| `sofascore.estatisticas_do_jogador_indisponiveis` | Estatisticas indisponiveis no momento. |
| `sofascore.historico_de_estatisticas_indisponivel` | Historico indisponivel no momento. |
| `sofascore.historico_de_transferencias_indisponivel` | Historico de transferencias indisponivel. |
| `sofascore.temporadas_do_torneio_indisponiveis` | Temporadas do torneio indisponiveis. |

Todo `502` e transitorio: a tela deve oferecer **tentar de novo**, nao tratar como erro definitivo. Um retry com backoff curto (1 tentativa, ~2s) costuma resolver.

## Tratamento sugerido no client

```ts
export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly code: string | undefined,
    readonly detail: string,
    readonly instance?: string,
  ) { super(detail); }
}

async function lancarSeErro(res: Response) {
  if (res.ok) return;

  const tipo = res.headers.get('content-type') ?? '';
  if (tipo.includes('json')) {
    const p = await res.json();
    throw new ApiError(res.status, p.code, p.detail ?? p.title, p.instance);
  }
  // Players/Tournament respondem texto puro
  throw new ApiError(res.status, undefined, await res.text());
}
```

Com `code` em maos, a tela escolhe a mensagem por um dicionario, e cai numa generica quando o codigo e desconhecido:

```ts
const MENSAGENS: Record<string, string> = {
  'shortlist.jogador_ja_na_lista': 'Este jogador ja esta na shortlist.',
  'acompanhamento.limite_atingido': 'Limite de 10 acompanhamentos atingido.',
  // ...
};

const mensagem = (e: ApiError) =>
  (e.code && MENSAGENS[e.code]) ?? 'Nao foi possivel completar a operacao. Tente de novo.';
```

### Regra do 401

Um `401` tem dois significados distintos:

- **access token expirado** (o comum, a cada 15 min) — chame `/api/Auth/refresh` uma vez e repita a requisicao original;
- **refresh invalido** (`usuario.refresh_token_invalido`) ou refresh que tambem falhou — limpe o estado e redirecione para o login.

Enfileire as requisicoes concorrentes enquanto o refresh esta em voo, para nao disparar varias rotacoes de token em paralelo e invalidar a sessao sem querer.

# Fluxos de tela

Como as features da API se encadeiam em jornadas, e o que cada regra de dominio cobra da
interface. As features do backend estao descritas em [`../features.md`](../features.md) e as regras em [`../regras-de-negocio.md`](../regras-de-negocio.md).

## O conceito central: recorte

Quase tudo no dominio de analise acontece dentro de um **recorte** — a tripla `competicaoId` + `temporadaId` + `contexto` (`Clube` ou `Selecao`). Metricas, comparacao e acompanhamento pedem os tres.

Nao existe endpoint que liste competicoes e temporadas de um jogador: essa informacao vem de `GET /api/Players/{id}/profile`, dentro de `stats`. Consequencia pratica: o seletor de recorte e derivado do perfil, e precisa ser carregado **antes** de qualquer tela de analise.

Um bom padrao e guardar o recorte selecionado em estado global (ou na URL) e reaproveita-lo entre as telas de metricas, comparacao e abertura de acompanhamento.

## 1. Sessao

```
signup / signin → guarda o par de tokens → chamadas autenticadas
         ↑                                        │ 401
         └────────── refresh (rotaciona) ←────────┘
```

- O par `accessToken` / `refreshToken` vem inteiro em toda resposta de auth, com as duas expiracoes. Renove proativamente perto de `accessTokenExpiresAtUtc` ou reativamente no primeiro `401`.
- Cada `refresh` **rotaciona** o refresh token. Guardar o antigo quebra a proxima renovacao.
- `change-password` tambem devolve um par novo e derruba as demais sessoes.
- `GET /api/Auth/me` no boot confirma que o token sobreviveu ao reload.
- `signout` precisa do refresh token no corpo; `signout-all` nao precisa de corpo.

Onde guardar os tokens e decisao de arquitetura do front. `localStorage` e simples mas exposto a XSS; memoria + refresh em cookie exigiria mudanca no backend (hoje o refresh vai no corpo). Comecar com `localStorage` e aceitavel enquanto o projeto é de portfolio, so registre a decisao.

## 2. Busca e perfil do jogador

```
/api/Players/search?q= → escolhe entity.id → /api/Players/{id}/profile
                                                    │
                            stats → monta seletor de competicao/temporada
```

- Filtre `results` por `type === 'player'`: a busca da fonte pode trazer outros tipos.
- O perfil e a porta de entrada das outras acoes: acompanhar, ver metricas, comparar, escrever relatorio, adicionar a shortlist.

## 3. Acompanhamento e deteccao de mudanca

```
perfil → POST /api/Acompanhamento (congela linha de base)
              │
              └→ GET /api/Acompanhamento/{jogadorId} (a cada visita ao painel)
```

A tela precisa deixar claro que o acompanhamento **compara contra o momento em que foi aberto** — nao e um historico continuo. `janela.de`/`janela.ate` e `duracaoEmDias` existem para comunicar isso ("comparando os ultimos 27 dias").

Renderize as tres afericoes como cartoes independentes, cada um em um de tres estados:

| Estado | Como mostrar |
| --- | --- |
| `ComMudanca` | destaque com `anterior → atual` e a variacao |
| `SemMudancaRelevante` | neutro, "sem mudanca relevante" |
| `Indisponivel` | atenuado, explicando o `motivo` |

Os dois motivos de indisponibilidade merecem texto proprio, porque nao sao erro:

- `MoedaInesperada` — a fonte passou a publicar o valor em outra moeda; comparar seria
  enganoso.
- `TemporadaVirada` — a minutagem zerou porque comecou uma nova temporada; a leitura antiga
  nao e comparavel.

Limite de **10 acompanhamentos ativos**. Mostre o consumo ("7 de 10") e desabilite o botao de acompanhar antes de o usuario tomar um `422`.

## 4. Metricas por 90

Uma tabela de sete linhas fixas, nunca filtrada. Cada linha e calculada ou recusada:

| Situacao | Tela |
| --- | --- |
| `Calculada` | valor + `amostraEmMinutos` como contexto de confianca |
| `Recusada` / `AmostraInsuficiente` | "amostra insuficiente (minimo 450 min)" |
| `Recusada` / `FonteNaoAtribuiu` | "nao publicado pela fonte" |

Esconder as linhas recusadas destroi a informacao mais importante da feature: **por que** nao da para avaliar aquele atributo. Mostre-as apagadas, com o motivo.

Cuidado com a unidade: `Gols`, `Assistencias`, `PassesDecisivos`, `Desarmes` e `Interceptacoes` sao por 90 minutos; `Rating` e media e `PrecisaoDePasse` e percentual.
Rotule cada eixo/coluna de acordo.

## 5. Comparacao direta

```
seleciona dois jogadores + um recorte → GET /api/Analise/comparacao
                                              ├─ Realizada → tabela lado a lado
                                              └─ Recusada  → explicacao do motivo
```

A recusa e um resultado legitimo, com `200 OK` — nao e erro. A tela precisa de um estado proprio para ela, explicando o motivo:

| Motivo | Texto sugerido |
| --- | --- |
| `PosicoesIncompativeis` | "Goleiro e meio-campista nao sao comparaveis." |
| `PosicaoDesconhecida` | "A fonte nao informou a posicao de um dos jogadores." |
| `RecorteDivergente` | "Os dados dos dois jogadores nao sao da mesma competicao/temporada." |
| `AmostraInsuficiente` | "Minutos jogados insuficientes para comparar." |

Note que `jogadores` volta **mesmo na recusa** — da para mostrar nome e posicao dos dois e explicar a incompatibilidade de forma concreta.

Dentro de uma comparacao realizada, cada atributo pode ainda ser recusado **para um so dos jogadores**. A celula fica vazia com o motivo, e o atributo nao vira um "vencedor". Um grafico de radar so funciona se todos os atributos estiverem calculados dos dois lados, caso contrario, prefira tabela.

Posicoes compativeis: `Ataque` com `MeioCampo`, `Defesa` com `MeioCampo`, e cada posicao consigo mesma. `Goleiro` so com `Goleiro`. Com isso da para filtrar o segundo seletor de jogador e evitar a recusa antes de chamar a API.

## 6. Relatorio de scouting

```
POST /api/Relatorio (rascunho)
   └→ PUT /{id}  ... edicoes parciais ...
        └→ PUT /{id}/finalizar → imutavel
              └→ POST /{id}/correcoes → novo rascunho (corrigeRelatorioId)
```

Duas regras moldam a tela:

**Edicao e parcial, listas sao substituidas por inteiro.** Enviar
`{ "nota": 8.5 }` nao apaga o texto; mas enviar `{ "pontosPositivos": ["um"] }` descarta os demais pontos positivos. Mantenha a lista completa no estado local e envie-a inteira a cada mudanca.

**Finalizar tem pre-requisitos.** Nota, parecer, 2 pontos positivos, 2 negativos e 20 caracteres somados nos pontos. Valide no cliente e mostre o que falta — o `422` do servidor e a rede de seguranca, nao o mecanismo de feedback.

**Finalizado e imutavel.** Depois de finalizar, os campos viram somente leitura e o unico caminho e "Criar correcao", que abre um rascunho novo herdando jogador e data de observacao, mas **sem** nota, parecer e pontos — o olheiro preenche de novo.

A listagem por jogador (`GET /api/Relatorio/jogador/{jogadorId}`) traz tudo misturado. Ordene por `escritoEm` e use `corrigeRelatorioId` para agrupar original + correcoes numa linha do tempo. `observadoEm` nao pode ser no futuro (`409`): limite o date picker a hoje.

## 7. Shortlist priorizada

```
POST /api/Shortlist → POST /{id}/alvos → PUT /{id}/alvos/{jogadorId} (reordena)
                                       → DELETE /{id}/alvos/{jogadorId}
```

**Toda operacao devolve a shortlist inteira, ja renumerada.** Substitua o estado local pelo que voltou — prioridade e sempre `1..n` contiguo, e uma insercao no meio muda o numero de varios alvos de uma vez. Estado otimista aqui gera divergencia visivel.

Drag-and-drop mapeia direto em `PUT .../alvos/{jogadorId}` com a nova posicao. Limites:
`1..alvos.length` na repriorizacao e `1..alvos.length + 1` na insercao.

A **moeda do primeiro alvo trava a moeda da lista**. Depois do primeiro, fixe o seletor de moeda no formulario de novo alvo — o `422` `shortlist.moeda_divergente` chega tarde demais para ser boa experiencia.

`custoTotal` vem pronto do servidor (ausente quando a lista esta vazia). Nao recalcule no front.

A shortlist guarda so `jogadorId`: para exibir nome e foto, busque `GET /api/Players/{id}/profile` de cada alvo. Com 25 alvos isso sao 25 chamadas — cacheie os perfis por id e reaproveite entre telas.

## Mapa de estados de tela

Todo consumo de dados externos tem quatro estados, e tres deles sao faceis de esquecer:

| Estado | Origem |
| --- | --- |
| Carregando | latencia da fonte externa; nao e instantanea |
| Sucesso | `200`/`201` |
| Vazio / recusado | `Recusada`, `Indisponivel`, lista vazia — **`200 OK`, nao erro** |
| Erro | `4xx` com `code`, `502` transitorio, `500` generico |

O quarto e o terceiro sao diferentes: uma metrica recusada por amostra insuficiente nao e
uma falha, e um resultado. Misturar os dois faz a interface mentir sobre os dados.

# Mapeamento do Modelo de Domínio

**Persistência:** MongoDB (banco de documentos) · **Stack:** .NET 10

Features em [features.md](features.md). Regras e classificações em [regras-de-negocio.md](regras-de-negocio.md).

---

## 1. Classificação de Subdomínios

Subdomínio é área de conhecimento do negócio, não funcionalidade. Features que compartilham vocabulário e ciclo de vida pertencem ao mesmo subdomínio.

| Subdomínio | Features | Classificação | Decisão |
| --- | --- | --- | --- |
| Avaliação de jogador | F5 | Core | O parecer do olheiro é o produto do sistema. Não existe fora dele e não é obtível de nenhuma fonte externa |
| Acompanhamento de jogador | F1, F2 | Supporting | Guarda o estado do jogador no instante da marcação, que a fonte externa não retém. Concentra a maior densidade de regras do escopo |
| Priorização de alvos | F7 | Supporting | Ordenação de alvos de transferência. Regras próprias de ordem, sem interseção com as demais |
| Análise estatística | F9, F10 | Supporting | Interpretação das estatísticas: normalização, recusa e comparação. O critério de recusa e o de compatibilidade são definidos pelo negócio |
| Aquisição de dados de jogador | — | Generic | Obtenção de estatísticas, valor de mercado e dados cadastrais. Resolvido pela API SofaScore, que é a fonte de verdade |
| Identidade e acesso | — | Generic | Autenticação e criação do usuário. Resolvido pelo ASP.NET Core Identity |

---

## 2. Bounded Contexts

Contexto é fronteira de significado: dentro dele, cada termo tem um sentido único.

| Contexto | Subdomínio | Conteúdo |
| --- | --- | --- |
| **Acompanhamento** | Acompanhamento de jogador | Agregado `Dossiê`. Leitura de mudança como operação sobre o dossiê |
| **Avaliação** | Avaliação de jogador | Agregado `Relatório` |
| **Priorização** | Priorização de alvos | Agregado `Shortlist` |
| **Análise** | Análise estatística | Sem agregados. Value objects e domain services |
| **Catálogo de Jogador** | Aquisição de dados | Camada de tradução sobre a API SofaScore. Traduz o retorno externo para os tipos do domínio |
| **Identidade** | Identidade e acesso | Fora do mapa de domínio. Aparece apenas no mapa de infraestrutura |

---

## 3. Agregados

### 3.1 `Dossiê` — agregado raiz

Contexto: Acompanhamento

```jsonc
{
  "_id": "UUID",
  "olheiro_id": "UUID",                 // ref. ao contexto de Identidade
  "jogador_id": "int",                  // identificador externo
  "status": "Ativo | Encerrado",        // R1.5
  "aberto_em": "ISODate",
  "encerrado_em": "ISODate | null",     // R1.10 — posterior a aberto_em
  "linha_de_base": {                    // EMBUTIDO — Value Object imutável (R1.3)
    "medida_em": "ISODate",             // R1.4
    "clube": "string",
    "valor_de_mercado": {
      "quantia_em_centavos": "long",
      "moeda": "string"
    },
    "minutagem": {
      "minutos": "int",
      "recorte": {                      // R2.5, R9.6
        "competicao_id": "int",
        "temporada_id": "int",
        "contexto": "Clube | Selecao"
      }
    }
  }
}
```

**Encerramento.** Não existe operação de exclusão. `status` transita para `Encerrado` e o documento permanece consultável, sem aceitar nova leitura nem nova comparação (R1.8). Reacompanhar cria um novo documento, com nova linha de base (R1.6).

**Unicidade.** Índice único parcial em `(olheiro_id, jogador_id)` restrito a `status: "Ativo"` — R1.2. Nenhuma instância do agregado consegue garantir sozinha uma condição que abrange todas as instâncias.

### 3.2 `Relatório` — agregado raiz

Contexto: Avaliação

```jsonc
{
  "_id": "UUID",
  "olheiro_id": "UUID",
  "jogador_id": "int",
  "status": "Rascunho | Finalizado",       // R5.1
  "nota": "decimal | null",                // R5.4 — faixa validada pelo VO
  "pontos_positivos": ["string"],
  "pontos_negativos": ["string"],
  "texto": "string",
  "parecer": "Contratar | Monitorar | Reavaliar | Descartar | null",  // R5.7
  "observado_em": "ISODate",               // R5.5 — data da observação
  "escrito_em": "ISODate",                 // R5.5 — data da redação
  "finalizado_em": "ISODate | null",
  "corrige_relatorio_id": "UUID | null"    // R5.3 — ref. entre agregados
}
```

**Imutabilidade após finalização.** O agregado recusa alteração no estado `Finalizado`.

**Duas datas.** `observado_em` e `escrito_em` são campos independentes. O modelo atual (`ScoutingReporter`) tem apenas `UpdatedAt` e não consegue exprimir R5.5.

**Independência de F1.** Não há referência a dossiê. Relatório não exige acompanhamento prévio.

### 3.3 `Shortlist` — agregado raiz

Contexto: Priorização

```jsonc
{
  "_id": "UUID",
  "olheiro_id": "UUID",
  "nome": "string",
  "alvos": [                            // EMBUTIDO — ordem total (R7.3)
    {
      "jogador_id": "int",
      "prioridade": "int",              // 1..n contíguo, sem empate (R7.3, R7.4)
      "custo_estimado": {
        "quantia_em_centavos": "long",
        "moeda": "string"               // R7.6 — soma exige moeda única
      }
    }
  ]
}
```

**Alvos embutidos.** R7.2, R7.3 e R7.4 são condições sobre o conjunto inteiro: sem repetição, ordem total e sem lacuna. Inserir ou remover um alvo desloca os subsequentes, o que obriga a lista a mudar por inteiro na mesma transação. R7.1 limita o tamanho do array.

---

## 4. Value Objects

| VO | Onde aparece | Regra encapsulada |
| --- | --- | --- |
| `LinhaDeBase` | `Dossiê` | Imutável, sem operação de alteração exposta. Data de medição obrigatória (R1.3, R1.4) |
| `Dinheiro` | `Dossiê`, `Shortlist` | Inteiro em centavos e moeda. Recusa operação entre moedas distintas (R2.4, R7.6). Shared kernel |
| `Recorte` | Contexto de Análise, `LinhaDeBase` | Competição, temporada e contexto de clube ou seleção. Integra a identidade do conjunto estatístico (R9.6, R2.5, R10.5). Published language do Catálogo de Jogador |
| `Nota` | `Relatório` | Faixa de valores válida (R5.4) |
| `Parecer` | `Relatório` | Conjunto fechado de valores (R5.7) |
| `OlheiroId` | os três agregados | Identificador opaco do contexto de Identidade |
| `JogadorId` | os três agregados | Identificador do jogador na fonte externa |
| `Prioridade` | `Shortlist` | Inteiro positivo, único dentro da lista |
| `MetricaPor90` | Contexto de Análise | Valor calculado ou recusa justificada, nunca zero nem nulo. Declara o tamanho da amostra (R9.3, R9.5) |
| `MudancaDetectada` | Contexto de Acompanhamento | Três resultados possíveis: com mudança, sem mudança e indisponível. Intervalo decorrido obrigatório (R2.6, R2.7) |
| `ResultadoDeComparacao` | Contexto de Análise | Declara os atributos excluídos por recusa (R10.3) |

---

## 5. Domain Services e Specifications

| Domain service | Contexto | Motivo |
| --- | --- | --- |
| Comparação de jogadores | Análise | Atravessa dois jogadores e não pertence a nenhum deles (F10) |
| Verificação de unicidade de dossiê | Acompanhamento | Abrange todas as instâncias do agregado (R1.2) |
| Verificação de limite de dossiês | Acompanhamento | Conta os dossiês ativos do olheiro, condição que nenhum dossiê verifica sozinho (R1.7) |

| Specification | Contexto | Predicado |
| --- | --- | --- |
| Jogador acompanhável | Acompanhamento | Possui as informações mínimas para servir de base (R1.1) |
| Mudança relevante | Acompanhamento | Quantitativa acima do limiar, ou categórica (R2.1, R2.3) |
| Leituras comparáveis | Acompanhamento | Não atravessam a virada de temporada (R2.5) |
| Amostra suficiente | Análise | Minutos acima do mínimo (R9.1) |
| Posições compatíveis | Análise | Compatibilidade não é igualdade (R10.1) |
| Atributos suficientes | Análise | Número de atributos comparáveis acima do mínimo (R10.4) |
| Mesmo recorte temporal | Análise | Ambos os lados usam o mesmo recorte (R10.5) |

Os valores parametrizados dessas specifications — limiares, mínimos e o mapa de compatibilidade entre posições — são políticas. Alterá-los não altera o modelo.

---

## 6. Decisões

### 6.1 Olheiro é domínio; autenticação não é

Autenticação, criação de usuário e recuperação de senha são infraestrutura e ficam no ASP.NET Core Identity. O **olheiro**, porém, é conceito de domínio: é dono do dossiê, autor do relatório e discriminador de quatro regras — R1.2, R1.7, R5.6 e a decisão de titularidade do dossiê registrada em `features.md`.

Retirar o olheiro do mapa deixaria essas regras sem lugar onde valer. O que sai do mapa é o usuário autenticado, não o olheiro.

Consequência prática: `OlheiroId` é um value object opaco. O domínio sabe apenas que existe um identificador de olheiro. Não há foreign key entre os contextos nem referência a `Microsoft.AspNetCore.Identity` no projeto de domínio.

### 6.2 O contexto de Análise não tem agregados

`MetricaPor90` e `ResultadoDeComparacao` são resultados de cálculo: não têm identidade, não persistem e não têm ciclo de vida. São value objects produzidos por domain services.

### 6.3 Vocabulário: observação e leitura

Dois atos distintos vinham sendo chamados de observação:

| Termo | Significado | Onde |
| --- | --- | --- |
| **Observação** | Ato humano de assistir ao jogador, que embasa o relatório | `Relatório.observado_em` (R5.5) |
| **Leitura** | Medição automática do estado do jogador na fonte externa | `LinhaDeBase.medida_em` (R1.4), R1.8 |

### 6.4 Core domain

O core registrado é Avaliação de jogador: o parecer é o produto entregue e não existe fora do sistema.

### 6.5 Não há conversão monetária

A fonte externa entrega todos os valores em euro. Não existe provedor de taxa de câmbio no escopo e nenhum é previsto, o que retira a conversão do mapa: não há domain service de câmbio.

`Dinheiro` mantém a moeda e a recusa de operar entre moedas distintas (R2.4, R7.6). O campo não é redundante: é ele que transforma "tudo vem em euro" de suposição em fato verificável na fronteira. Valor em moeda diferente da esperada é recusado na tradução, e a mudança correspondente é reportada como indisponível pelo terceiro estado de `MudancaDetectada` (R2.6).

---

## 7. Relações entre Contextos

Critério de escolha do padrão: **Customer/Supplier** onde o downstream define o que a tradução precisa produzir; **Conformist** onde o downstream aceita o formato já existente.

| Upstream | Downstream | Padrão |
| --- | --- | --- |
| API SofaScore (externo) | Catálogo de Jogador | Anticorruption Layer |
| Catálogo de Jogador | Análise | Customer/Supplier |
| Catálogo de Jogador | Acompanhamento | Conformist |
| Catálogo de Jogador | Avaliação | Conformist |
| Catálogo de Jogador | Priorização | Conformist |
| Identidade | Acompanhamento, Avaliação, Priorização | Anticorruption Layer |

```
      API SofaScore (externo)
              │  ACL
              ▼
     Catálogo de Jogador ──── customer/supplier ────► Análise
              │
              ├── conformist ──► Acompanhamento ◄──┐
              ├── conformist ──► Avaliação      ◄──┤ ACL
              └── conformist ──► Priorização    ◄──┘
                                                Identidade
```

### 7.1 SofaScore e Catálogo de Jogador — anticorruption layer

O Catálogo de Jogador é a própria camada de tradução; não há um ACL adicional entre os dois. O padrão descreve a postura do Catálogo diante da fonte: nenhum tipo do retorno externo atravessa a fronteira do domínio.

Do lado da fonte, a relação é imposta: a API é publicada para todos os consumidores e não é negociável. Todo o custo de adaptação fica do lado de baixo.

### 7.2 Catálogo de Jogador e Análise — customer/supplier

Análise é o único downstream que dita a forma da tradução:

- R9.4 exige que a tradução separe estatística acumulável de valor derivado em tipos distintos. A fonte externa entrega os dois na mesma estrutura.
- R9.6 exige que o recorte integre a identidade do conjunto de estatísticas. A fonte externa expõe recortes distintos em endpoints distintos, sem identificação no retorno.

Nenhuma das duas regras é exprimível se o Catálogo devolver o formato da fonte. O requisito nasce em Análise e é atendido no Catálogo.

### 7.3 Catálogo de Jogador e os demais contextos — conformist

Acompanhamento, Avaliação e Priorização consomem o formato que o Catálogo já produz e não impõem exigências próprias sobre a tradução. O recorte de que Acompanhamento precisa em R2.5 já existe por força de R9.6.

### 7.4 Identidade e os contextos de domínio — anticorruption layer

O contexto de Identidade é genérico e tem modelo próprio, com conceitos que não pertencem à linguagem de scouting. A tradução reduz esse modelo a um único value object opaco, `OlheiroId`, conforme 6.1.

### 7.5 Tipos que atravessam fronteira

`Recorte` e `Dinheiro` são os dois únicos tipos que existem em mais de um contexto, e cada um recebe um tratamento distinto.

**`Recorte` — published language do Catálogo de Jogador.** O tipo nasce na tradução, por força de R9.6, e desce para Acompanhamento e Análise. A definição pertence ao Catálogo; contexto downstream que precise alterá-la negocia a mudança pelo padrão que o liga ao Catálogo.

**`Dinheiro` — shared kernel.** O tipo não tem dono natural. Em `Dossiê`, o valor vem da fonte externa pelo Catálogo; em `Shortlist`, o custo estimado é informado pelo olheiro e nunca passa pelo Catálogo. Atribuí-lo ao Catálogo criaria uma dependência de Priorização sobre um contexto que não participa daquele fluxo.

A invariante encapsulada é a mesma nos dois usos — R2.4 e R7.6 são a mesma recusa de operar entre moedas distintas —, o que sustenta um tipo único e compartilhado. O custo do shared kernel, obrigar revisão de todos os consumidores a cada alteração, é baixo aqui: o tipo é pequeno e estável.

### 7.6 Contextos sem consumidor

Análise não é upstream de nenhum contexto. `MetricaPor90` e `ResultadoDeComparacao` são consumidos pela camada de aplicação, não por outro contexto de domínio.

Acompanhamento, Avaliação e Priorização não se relacionam entre si. O único elo é a identidade do jogador, que cada um obtém do Catálogo.

---

## Fora de escopo desta versão

- **Eventos de domínio** — nenhum evento é publicado entre agregados no escopo atual.
- **Consolidação entre olheiros** — o dossiê pertence ao olheiro e não há visão agregada por clube.
- **Série acumulada de leituras** — registrada como evolução futura em `features.md`.

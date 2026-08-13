# Regras de negócio

Regras derivadas das features descritas em [features.md](features.md).

## Categorias

| Categoria | Definição | Critério |
| --- | --- | --- |
| **Invariante** | Condição sobre o estado, garantida na escrita pelo próprio agregado ou value object | Se puder ser falsa, ainda que momentaneamente, o modelo está inválido |
| **Specification** | Predicado que classifica um candidato. Aplicável tanto na filtragem quanto na validação | É uma pergunta de sim ou não sobre um objeto, reaproveitável em mais de um ponto |
| **Domain service** | Operação que atravessa agregados ou não pertence a nenhuma entidade | Exige mais de um agregado, ou não tem dono natural |
| **Política** | Parâmetro que o negócio ajusta sem alterar o modelo | Se o valor mudar, o modelo permanece íntegro e apenas o resultado muda |
| **Decisão de modelagem** | Restringe quais combinações são exprimíveis, não quais estados são válidos | Cumprida pelo desenho dos tipos, não por verificação em tempo de execução |

Regras que se decompõem recebem mais de uma categoria.

---

## F1 — Acompanhamento e dossiê

| ID | Regra | Classificação |
| --- | --- | --- |
| R1.1 | Só se acompanha jogador que possua as informações mínimas para servir de base comparável. Sem base, o acompanhamento é recusado | Specification |
| R1.2 | O mesmo olheiro não acompanha o mesmo jogador duas vezes simultaneamente | Domain service |
| R1.3 | A linha de base é imutável | Decisão de modelagem |
| R1.4 | A linha de base registra a data em que foi medida | Invariante |
| R1.5 | O encerramento do acompanhamento é uma transição de estado do dossiê | Decisão de modelagem |
| R1.6 | Reacompanhar um jogador cria uma nova linha de base, sem restaurar a anterior | Decisão de modelagem |
| R1.7 | Um olheiro tem limite de jogadores acompanhados simultaneamente *(opcional)* | Política + Invariante |
| R1.8 | Um dossiê encerrado não aceita nova leitura nem nova comparação | Invariante |
| R1.9 | Não se encerra um dossiê já encerrado | Invariante |
| R1.10 | A data de encerramento é posterior à data de abertura | Invariante |

**Notas**

- R1.1 — o predicado responde se um jogador é acompanhável. É aplicado como guarda na abertura do dossiê e reaproveitável na exibição do resultado de busca.
- R1.2 — unicidade que abrange múltiplas instâncias do agregado não pode ser garantida por nenhuma delas isoladamente. A classificação depende do mapeamento: se o olheiro for a raiz que detém os dossiês, a regra passa a ser invariante desse agregado.
- R1.3 — a imutabilidade é obtida modelando a linha de base como value object, sem operação de alteração exposta. Não há verificação em tempo de execução.
- R1.5 — a regra restringe o conjunto de operações existentes, não o estado. Encerrado é um estado do dossiê, e não existe operação de exclusão. As invariantes R1.8 a R1.10 decorrem dessa decisão.
- R1.7 — a existência do limite e seu respeito são invariante; o valor do limite é política.
- R1.8 — o dossiê encerrado preserva a linha de base e permanece consultável, mas deixa de ser alimentado.

## F2 — Detecção de mudança

| ID | Regra | Classificação |
| --- | --- | --- |
| R2.1 | Uma mudança quantitativa só é relevante quando ultrapassa um limiar | Specification |
| R2.2 | O limiar varia conforme o tipo de mudança | Política |
| R2.3 | Toda mudança categórica é relevante | Specification |
| R2.4 | Valores em moedas distintas exigem conversão explícita. Sem taxa, não há comparação | Invariante + Domain service |
| R2.5 | Não se compara estatística acumulada através da virada de temporada | Specification |
| R2.6 | Dado atual indisponível produz resultado "indisponível", nunca "sem mudança" | Decisão de modelagem |
| R2.7 | O intervalo de tempo decorrido integra o resultado da comparação | Invariante |

**Notas**

- R2.1 e R2.3 são cláusulas de uma mesma specification, que julga a relevância de uma diferença. Os valores dos limiares são política (R2.2).
- A distinção entre os dois tipos de mudança é o que sustenta as duas cláusulas. Mudança quantitativa tem magnitude e admite limiar: valor de mercado e minutagem. Mudança categórica não tem magnitude, e limiar é conceito inaplicável a ela: clube é o único caso no escopo atual.
- R2.4 — a recusa de operar entre moedas distintas é invariante do value object monetário. A conversão em si é domain service, por depender de uma taxa externa ao modelo.
- R2.5 — as estatísticas de temporada da fonte externa são acumulativas e reiniciam a cada nova temporada. Comparar através da virada produz queda aparente de minutagem sem correspondência na realidade. O predicado responde se duas leituras são comparáveis, mesma família de R10.5.
- R2.6 — a regra define que o resultado admite três estados distintos. É cumprida pelo tipo do retorno, não por verificação.
- R2.7 — uma variação percentual sem o intervalo em que ocorreu não é interpretável. O resultado não é construível sem o intervalo.

## F5 — Relatório de scouting

| ID | Regra | Classificação |
| --- | --- | --- |
| R5.1 | Rascunho é editável; relatório finalizado é imutável | Invariante |
| R5.2 | A finalização exige conteúdo mínimo | Invariante + Política |
| R5.3 | A correção de relatório finalizado se dá por novo relatório que referencia o anterior | Decisão de modelagem |
| R5.4 | A nota respeita uma faixa de valores válida | Invariante |
| R5.5 | O relatório registra a data da observação, distinta da data de escrita | Invariante |
| R5.6 | Relatórios de olheiros distintos sobre o mesmo jogador coexistem e não se fundem | Decisão de modelagem |
| R5.7 | O relatório conclui em um parecer: contratar, monitorar, reavaliar ou descartar *(opcional)* | Invariante |

**Notas**

- R5.1 — o agregado recusa alteração quando está no estado finalizado. Condição verificável sobre o estado.
- R5.2 — a exigência é invariante da transição; a definição do que constitui conteúdo mínimo é política.
- R5.3 — não existe operação de edição de relatório finalizado. A correção é a criação de um novo agregado que referencia o anterior.
- R5.5 — a observação e a redação ocorrem em momentos distintos. Registrar apenas a data de escrita impede aferir a atualidade do relatório e produz ordenação cronológica incorreta.
- R5.6 — não existe operação de fusão. Cada relatório pertence a um autor e permanece separado.
- R5.7 — a finalização exige parecer preenchido. O conjunto fechado de valores é definido pelo value object.

## F7 — Shortlist priorizada

| ID | Regra | Classificação |
| --- | --- | --- |
| R7.1 | A lista tem número máximo de alvos | Invariante + Política |
| R7.2 | Não há jogador repetido na mesma lista | Invariante |
| R7.3 | A prioridade é uma ordem total, sem empates | Invariante |
| R7.4 | A remoção de um alvo não deixa lacuna na ordem | Invariante |
| R7.6 | A soma do custo da lista exige moeda única | Invariante |

**Notas**

- R7.1 — a existência do limite é invariante; seu valor é política.
- R7.3 e R7.4 — juntas, determinam o comportamento da inserção e da remoção: manter ordem total e contígua obriga o deslocamento dos alvos subsequentes.
- R7.6 — mesma invariante de value object monetário de R2.4.

## F9 — Métricas por 90 minutos

| ID | Regra | Classificação |
| --- | --- | --- |
| R9.1 | Há uma amostra mínima de minutos para que o cálculo seja permitido | Specification + Política |
| R9.3 | O resultado é um valor ou uma recusa justificada, nunca zero e nunca nulo | Decisão de modelagem |
| R9.4 | Apenas estatísticas acumuláveis são normalizadas | Decisão de modelagem |
| R9.5 | A métrica declara o tamanho da amostra que a sustenta | Invariante |
| R9.6 | Numerador e denominador provêm do mesmo recorte | Decisão de modelagem |

**Notas**

- R9.1 — o predicado responde se a amostra sustenta o cálculo; o valor do mínimo é política.
- R9.3 — o retorno admite dois resultados distintos, e a recusa é um deles. Cumprida pelo tipo, mesma forma de R2.6.
- R9.4 — percentuais e médias, como precisão de passe e rating, não admitem normalização por 90 minutos, ainda que sejam entregues pela fonte externa na mesma estrutura das estatísticas acumuláveis. Separando contagem e valor derivado em tipos distintos, a operação passa a existir apenas no primeiro. A distinção entre os campos do DTO é decisão de mapeamento, na camada de tradução.
- R9.6 — o recorte é definido por competição, temporada e contexto de clube ou seleção. A fonte externa expõe recortes distintos em endpoints e parâmetros distintos, sem identificação no retorno. Incorporando o recorte à identidade do conjunto de estatísticas, a combinação incorreta deixa de ser exprimível.

## F10 — Comparação direta

A operação de comparação é, em si, um domain service: atravessa dois jogadores e não pertence a nenhum deles.

| ID | Regra | Classificação |
| --- | --- | --- |
| R10.1 | As posições dos jogadores precisam ser compatíveis; compatibilidade não é igualdade | Specification + Política |
| R10.2 | Um jogador não é comparado consigo mesmo | Invariante |
| R10.3 | Atributo cujo cálculo foi recusado é excluído da comparação, e a exclusão é declarada | Domain service |
| R10.4 | Número insuficiente de atributos comparáveis recusa a comparação inteira | Specification + Política |
| R10.5 | Ambos os lados da comparação usam o mesmo recorte temporal | Specification |
| R10.7 | A comparação é simétrica: a ordem dos jogadores não altera o resultado | Decisão de modelagem |

**Notas**

- R10.1 — o critério é specification; o mapa de quais posições são compatíveis entre si é definido pelo negócio e configurável, portanto política.
- R10.2 — diferente de R10.1, é guarda de caso degenerado, sem conteúdo de negócio e sem reaproveitamento. Fica como guarda de construção da comparação.
- R10.3 — regra sobre o comportamento do domain service de comparação: a recusa de R9.3 se propaga e é declarada no resultado.
- R10.4 — o critério é specification; o número mínimo de atributos é política.
- R10.7 — propriedade do algoritmo, garantida pelo desenho da operação e verificável por teste, não por guarda em tempo de execução.

---

## Distribuição

| Categoria | Ocorrências |
| --- | --- |
| Invariante | 19 |
| Decisão de modelagem | 10 |
| Specification | 8 |
| Política | 7 |
| Domain service | 3 |

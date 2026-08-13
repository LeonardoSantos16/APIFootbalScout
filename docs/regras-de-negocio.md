# Regras de negócio

Regras derivadas das features descritas em [features.md](features.md).

A coluna **Classificação** — invariante, regra de negócio, specification ou domain service — será preenchida na etapa de classificação, anterior ao mapeamento do domínio.

---

## F1 — Acompanhamento e dossiê

| ID | Regra | Classificação |
| --- | --- | --- |
| R1.1 | Só se acompanha jogador que possua as informações mínimas para servir de base comparável. Sem base, o acompanhamento é recusado | — |
| R1.2 | O mesmo olheiro não acompanha o mesmo jogador duas vezes simultaneamente | — |
| R1.3 | A linha de base é imutável | — |
| R1.4 | A linha de base registra a data em que foi medida | — |
| R1.5 | Encerrar o acompanhamento não apaga o dossiê | — |
| R1.6 | Reacompanhar um jogador cria uma nova linha de base, sem restaurar a anterior | — |
| R1.7 | Um olheiro tem limite de jogadores acompanhados simultaneamente *(opcional)* | — |

**Notas**

- R1.3 — a alteração da linha de base invalida toda comparação derivada dela.

## F2 — Detecção de mudança

| ID | Regra | Classificação |
| --- | --- | --- |
| R2.1 | Uma diferença só é relevante quando ultrapassa um limiar | — |
| R2.2 | O limiar varia conforme o tipo de mudança | — |
| R2.3 | Mudança de clube é sempre relevante, independentemente de limiar | — |
| R2.4 | Valores em moedas distintas exigem conversão explícita. Sem taxa, não há comparação | — |
| R2.5 | Não se compara estatística acumulada através da virada de temporada | — |
| R2.6 | Dado atual indisponível produz resultado "indisponível", nunca "sem mudança" | — |
| R2.7 | O intervalo de tempo decorrido integra o resultado da comparação | — |

**Notas**

- R2.5 — as estatísticas de temporada da fonte externa são acumulativas e reiniciam a cada nova temporada. Comparar através da virada produz queda aparente de minutagem sem correspondência na realidade.
- R2.7 — uma variação percentual sem o intervalo em que ocorreu não é interpretável.

## F5 — Relatório de scouting

| ID | Regra | Classificação |
| --- | --- | --- |
| R5.1 | Rascunho é editável; relatório finalizado é imutável | — |
| R5.2 | A finalização exige conteúdo mínimo | — |
| R5.3 | A correção de relatório finalizado se dá por novo relatório que referencia o anterior | — |
| R5.4 | A nota respeita uma faixa de valores válida | — |
| R5.5 | O relatório registra a data da observação, distinta da data de escrita | — |
| R5.6 | Relatórios de olheiros distintos sobre o mesmo jogador coexistem e não se fundem | — |
| R5.7 | O relatório conclui em um parecer: contratar, monitorar, reavaliar ou descartar *(opcional)* | — |

**Notas**

- R5.5 — a observação e a redação ocorrem em momentos distintos. Registrar apenas a data de escrita impede aferir a atualidade do relatório e produz ordenação cronológica incorreta.

## F7 — Shortlist priorizada

| ID | Regra | Classificação |
| --- | --- | --- |
| R7.1 | A lista tem número máximo de alvos | — |
| R7.2 | Não há jogador repetido na mesma lista | — |
| R7.3 | A prioridade é uma ordem total, sem empates | — |
| R7.4 | A remoção de um alvo não deixa lacuna na ordem | — |
| R7.5 | A inserção em uma posição desloca os alvos subsequentes, sem sobrescrever | — |
| R7.6 | A soma do custo da lista exige moeda única | — |

## F9 — Métricas por 90 minutos

| ID | Regra | Classificação |
| --- | --- | --- |
| R9.1 | Há uma amostra mínima de minutos para que o cálculo seja permitido | — |
| R9.3 | O resultado é um valor ou uma recusa justificada, nunca zero e nunca nulo | — |
| R9.4 | Apenas estatísticas acumuláveis são normalizadas | — |
| R9.5 | A métrica declara o tamanho da amostra que a sustenta | — |
| R9.6 | Numerador e denominador provêm do mesmo recorte | — |

**Notas**

- R9.4 — percentuais e médias, como precisão de passe e rating, não admitem normalização por 90 minutos, ainda que sejam entregues pela fonte externa na mesma estrutura das estatísticas acumuláveis.
- R9.6 — o recorte é definido por competição, temporada e contexto de clube ou seleção. A fonte externa expõe recortes distintos em endpoints e parâmetros distintos, sem identificação no retorno.

## F10 — Comparação direta

| ID | Regra | Classificação |
| --- | --- | --- |
| R10.1 | As posições dos jogadores precisam ser compatíveis; compatibilidade não é igualdade | — |
| R10.2 | Um jogador não é comparado consigo mesmo | — |
| R10.3 | Atributo cujo cálculo foi recusado é excluído da comparação, e a exclusão é declarada | — |
| R10.4 | Número insuficiente de atributos comparáveis recusa a comparação inteira | — |
| R10.5 | Ambos os lados da comparação usam o mesmo recorte temporal | — |
| R10.7 | A comparação é simétrica: a ordem dos jogadores não altera o resultado | — |

---

## Regras removidas

| ID | Regra | Motivo |
| --- | --- | --- |
| R9.2 | O mínimo de minutos varia por posição | Adotado limiar único |
| R10.6 | Comparação entre ligas distintas é declarada como tal | Depende do peso de liga, fora de escopo |

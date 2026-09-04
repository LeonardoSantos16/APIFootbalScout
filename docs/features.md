# Features — APIFootballScout

Escopo funcional definido para a implementação do modelo de domínio.

As regras de cada feature estão em [regras-de-negocio.md](regras-de-negocio.md). Os identificadores são estáveis e não sequenciais: features descartadas tiveram seus números preservados para manter a rastreabilidade com as regras.

---

## F1 — Acompanhamento e dossiê

O olheiro marca um jogador para acompanhar. No ato da marcação, o sistema registra uma **linha de base** com as informações do jogador naquele instante.

O acompanhamento é individual: o dossiê pertence ao olheiro que o abriu. Dois olheiros que acompanham o mesmo jogador mantêm dossiês independentes, cada um com sua própria linha de base e sua própria data de início.

O dossiê tem ciclo de vida: abre na marcação, permanece enquanto o acompanhamento está ativo e é encerrado sem ser apagado.

## F2 — Detecção de mudança

Na consulta aos jogadores acompanhados, o sistema compara a linha de base do dossiê com os dados atuais da fonte externa e apresenta as diferenças relevantes.

Mudanças acompanhadas: clube, valor de mercado e minutagem.

A comparação é calculada no momento da leitura. Nesta versão não há persistência de estados intermediários entre a linha de base e o dado atual.

## F5 — Relatório de scouting

O olheiro registra a avaliação de um jogador em formato de relatório, com nota, pontos positivos, pontos negativos e observações.

O relatório existe em dois estados: **rascunho**, editável, e **finalizado**, imutável. A correção de um relatório finalizado se dá pela emissão de um novo relatório que referencia o anterior.

O relatório distingue a data da observação que o embasou da data em que foi escrito.

Não exige acompanhamento prévio do jogador.

## F7 — Shortlist priorizada

Lista ordenada de alvos de transferência, com número máximo de posições e ordem de prioridade explícita.

A ordenação é total: não há empate de prioridade, remoções não deixam lacunas, e inserções e repriorizações deslocam os alvos subsequentes.

Não exige acompanhamento prévio do jogador.

## F9 — Métricas por 90 minutos

Normalização de estatísticas acumuláveis por 90 minutos jogados.

O cálculo é recusado de forma explícita quando a amostra de minutos é insuficiente para sustentar o resultado. A recusa é um resultado válido da operação, distinto de zero e de ausência de valor.

A métrica declara o tamanho da amostra que a sustenta.

## F10 — Comparação direta

Comparação head-to-head entre dois jogadores, restrita a posições compatíveis e ao mesmo recorte temporal.

Atributos cuja normalização foi recusada (F9) são excluídos da comparação, e a exclusão é declarada no resultado. Se restarem atributos comparáveis em número insuficiente, a comparação inteira é recusada.

---

## Decisões de escopo

| Decisão | Definição |
| --- | --- |
| Titularidade do dossiê | O dossiê pertence ao olheiro, não ao clube. Não há visão consolidada entre olheiros |
| Gatilho da observação | O registro ocorre no ato de acompanhar. Consultas a jogadores não acompanhados não geram registro |
| Modelo de comparação | Linha de base única confrontada com o dado atual, calculada na leitura |
| Acoplamento entre features | F5 e F7 são independentes de F1. O único elo entre os blocos é a identidade do jogador |

## Fora de escopo

| Feature | Motivo |
| --- | --- |
| Snapshot histórico | Absorvida pelo F1 |
| Peso da liga | Exige o corpus completo da liga em base local |
| Radar de atributos | Depende de cálculo de percentil, que depende do mesmo corpus |
| Unificação de fontes | Só se justifica com um segundo provedor de dados |
| Relatório de partida | Depende de dados de jogo não expostos pelo cliente atual |
| Etiquetas de perfil | Densidade de regra insuficiente |
| Shortlist colaborativa | Controle de permissão é responsabilidade da camada de aplicação |
| Linha do tempo do jogador | Depende de histórico acumulado, indisponível no modelo de linha de base |
| Alertas de contrato | Sem fonte de dados para data de fim de contrato |

## Evolução futura

**Série acumulada.** Substituição da linha de base única por uma série de leituras, anexando um registro a cada consulta a jogador acompanhado.

Habilita a datação da mudança, a análise de tendência e a detecção de variações que se revertem entre duas leituras — todas inacessíveis ao modelo de linha de base. O custo incremental é uma escrita por consulta já realizada, sem chamadas adicionais à fonte externa.

# Acompanhamento

Contexto do acompanhamento de jogador: guarda o estado do jogador no instante em que o olheiro o marcou e afere, na consulta, o que mudou desde então. Cobre F1 e F2.

Features em [features.md](../../../docs/features.md), regras em [regras-de-negocio.md](../../../docs/regras-de-negocio.md), mapa de contextos em [Mapeamento do Modelo de Domínio.md](../../../docs/Mapeamento%20do%20Modelo%20de%20Domínio.md).

## Language

### Acompanhamento

**Dossie**:
Registro de que um olheiro acompanha um jogador, dono da linha de base e do próprio ciclo de vida. Pertence ao olheiro que o abriu, nunca ao clube.
_Avoid_: Ficha, monitoramento, watchlist

**Olheiro**:
Quem abre dossiês e assina relatórios. É conceito de domínio, distinto do usuário autenticado, que vive no contexto de Identidade.
_Avoid_: Usuário, scout, analista

**LinhaDeBase**:
Retrato imutável do jogador no instante da marcação: clube, valor de mercado e minutagem, com a data em que foi medido. É o lado fixo de toda aferição.
_Avoid_: Snapshot, baseline, estado inicial

**Leitura**:
Medição automática do estado do jogador na fonte externa. Distinta de **observação**, que é o ato humano de assistir ao jogador e pertence ao contexto de Avaliação.
_Avoid_: Observação, coleta, sincronização

**LeituraAtual**:
A leitura recém-obtida do Catálogo de Jogador, confrontada com a linha de base. É o lado móvel da aferição, e não tem tipo próprio: circula como `PerfilDoJogador`.
_Avoid_: Dado atual, estado corrente

**Encerramento**:
Transição do dossiê para o estado encerrado. O dossiê encerrado preserva a linha de base e permanece legível, mas não aceita nova aferição; não existe exclusão de dossiê.
_Avoid_: Exclusão, arquivamento, cancelamento

### Detecção de mudança

**LeituraDeMudanca**:
Resultado inteiro do confronto entre um dossiê e a leitura atual: uma aferição por tipo de mudança, mais a janela em que o confronto se deu. Calculado na consulta e nunca persistido.
_Avoid_: MudancaDetectada, ResultadoDeComparacao, diff

**AfericaoDeMudanca**:
Veredito sobre um único tipo de mudança. Admite três resultados, e apenas três: com mudança, sem mudança relevante e indisponível.
_Avoid_: MudancaDetectada, comparação

**ComMudanca**:
Aferição que constatou mudança digna de ser apresentada ao olheiro. Sempre traz o valor anterior e o atual.
_Avoid_: Alterado, mudou

**MudancaQuantitativa**:
Mudança que tem magnitude e por isso admite limiar: valor de mercado e minutagem. Declara a variação entre anterior e atual.
_Avoid_: Mudança numérica, delta

**MudancaCategorica**:
Mudança sem magnitude, para a qual limiar é conceito inaplicável: clube é o único caso. É sempre relevante.
_Avoid_: Mudança textual, mudança discreta

**SemMudancaRelevante**:
Aferição que nada tem a apresentar, seja porque o valor não mudou, seja porque mudou aquém do limiar. Os dois casos não se distinguem.
_Avoid_: SemMudanca, inalterado, zero

**Indisponivel**:
Aferição que não pôde ser emitida porque o dado atual não sustenta o confronto. É resultado válido, distinto de "sem mudança", e nunca é usado para falha da fonte externa, que é erro da operação.
_Avoid_: Nulo, desconhecido, erro

**MotivoDeIndisponibilidade**:
A razão pela qual a aferição saiu indisponível: moeda inesperada, ou temporada virada. Conjunto fechado.
_Avoid_: Mensagem de erro, causa

**TipoDeMudanca**:
O que se acompanha em um dossiê: clube, valor de mercado e minutagem. É por tipo que o limiar varia e que a aferição é emitida.
_Avoid_: Atributo (pertence ao contexto de Análise), campo, métrica

**JanelaDaComparacao**:
O intervalo entre a medição da linha de base e a leitura atual, com as duas pontas. Sem ele o resultado não é interpretável, e por isso não é construível.
_Avoid_: Período, intervalo decorrido, idade do dossiê

**LimiarDeRelevancia**:
O valor a partir do qual uma mudança quantitativa passa a ser apresentada. Varia por tipo de mudança e é ajustável sem alterar o modelo.
_Avoid_: Tolerância, threshold, margem

**MudancaRelevante**:
Predicado que julga se uma mudança merece ser apresentada: quantitativa acima do limiar, ou categórica de qualquer magnitude.
_Avoid_: Mudança significativa, mudança material

**LeiturasComparaveis**:
Predicado que julga se duas leituras podem ser confrontadas. Governa apenas a minutagem, que é acumulativa e reinicia a cada temporada; clube e valor de mercado atravessam a virada sem prejuízo.
_Avoid_: Leituras compatíveis, mesma temporada

### Termos compartilhados

**Recorte**:
A competição, a temporada e o contexto de clube ou seleção a que um conjunto de estatísticas pertence. Nasce no Catálogo de Jogador e integra a identidade da minutagem.
_Avoid_: Filtro, escopo, período

**Minutagem**:
Minutos jogados dentro de um recorte. Só é comparável contra outra minutagem do mesmo recorte.
_Avoid_: Minutos, tempo de jogo

**Dinheiro**:
Quantia em centavos acompanhada da moeda. Recusa operação entre moedas distintas; não há conversão no escopo.
_Avoid_: Valor, preço, montante

### Fora do domínio

**AcompanhamentoConsultado**:
Forma de saída da consulta aos acompanhamentos, reunindo os dados do dossiê e a leitura de mudança. Existe na camada de aplicação e não é conceito de domínio: não tem invariante, não persiste e não deve ser promovido a agregado.
_Avoid_: Dossiê (é outra coisa), visão do dossiê

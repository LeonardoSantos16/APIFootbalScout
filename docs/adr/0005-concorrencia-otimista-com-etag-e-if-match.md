# Concorrência otimista nos agregados com versão, ETag e If-Match

Shortlist e Relatório são gravados substituindo o documento inteiro (carrega, altera em memória, `ReplaceOne`), e duas requisições simultâneas faziam uma sobrescrever a outra sem que ninguém percebesse. Cada agregado passou a ter uma `Version` (em `AggregateRootBase`), que nasce em 1 na inserção e sobe a cada gravação. O cliente recebe a versão no header `ETag` e a devolve no `If-Match` de toda escrita; o repositório filtra o `ReplaceOne` por `Id` e pela versão esperada, e nenhum documento casado significa que alguém gravou antes: o pedido é recusado com `ConflitoDeConcorrenciaException`, que a API responde como `412`. Escrita sem `If-Match` (ou com ETag fraco) é recusada com `428`.

A verificação é feita à mão no filtro do driver do MongoDB, porque os repositórios dos agregados não usam o EF Core e, por isso, não têm o concurrency token do provider.

## Considered Options

- **Lock pessimista.** Não foi rejeitado por limitação técnica: neste projeto as duas abordagens dariam o mesmo resultado, porque não há concorrência massiva e cada olheiro só escreve nos próprios recursos, então conflitos são raros. A concorrência otimista foi escolhida por ser a que ainda não tinha sido implementada no projeto, como exercício de aprendizado. Se surgir um recurso disputado por vários usuários com conflitos frequentes (uma shortlist compartilhada, por exemplo), a escolha deve ser revista.
- **Verificar a versão no use case** (comparar a do pedido com a carregada) **e no repositório.** Rejeitado: o filtro do repositório com a versão do cliente já cobre os dois casos, o cliente com versão velha e a corrida entre carregar e gravar. Duas verificações seriam duas regras para manter.
- **`409 Conflict` para versão divergente.** Rejeitado: `412 Precondition Failed` é o status que a especificação HTTP define para `If-Match` que não confere, e separa "o recurso mudou, recarregue" do `409` que a API já usa para conflito com regra de domínio (`relatorio.ja_finalizado`).
- **`If-Match` opcional, sem checagem quando ausente.** Rejeitado: o cliente que não envia o header continuaria sujeito à perda de atualização, e a proteção deixaria de valer justamente onde ninguém lembrou dela.
- **Versão só no header.** Rejeitado: as listagens devolvem vários agregados e um único `ETag` não representa todos. A versão também vai no corpo (`version`) para o front editar um item vindo da lista sem um `GET` extra.

## Consequences

- O escopo são as escritas que substituem o documento: Adicionar, Remover e Repriorizar alvo na Shortlist, Editar e Finalizar no Relatório. A correção de relatório não usa `If-Match`, porque cria um relatório novo e não grava no original. O Acompanhamento ficou de fora: `AcompanhamentoRepositoryMongo.AtualizarAsync` não tem chamador.
- Documentos gravados antes desta mudança não têm o campo `version`, e o filtro não os encontra: toda escrita sobre eles vira `412`. Não há migração; como ainda não existe ambiente publicado, a saída foi apagar o banco local. Um ambiente com dados exigirá migração antes do deploy.
- Uma edição atrasada sobre um relatório já finalizado é recusada pelo domínio (`409 relatorio.ja_finalizado`) antes de chegar à versão, porque o use case carrega o estado atual. A versão protege a janela entre carregar e gravar.
- Os testes de concorrência ficam no use case, com repositórios em memória que guardam documentos e passam pelos mappers de persistência. O filtro real do MongoDB e o tratamento do header no controller ainda não têm teste: ficam para os testes de integração (item 5.1 do plano de evolução).
- O front precisa ler o `ETag`, que o navegador só expõe a uma origem diferente se a política de CORS o liberar. A API ainda não tem política de CORS (pendência registrada no Roadmap do README).

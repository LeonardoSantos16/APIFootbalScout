# Limite de acompanhamentos garantido por um contador atômico por olheiro

A abertura de acompanhamento contava os dossiês ativos do olheiro e, se estivesse abaixo do limite, inseria um novo. Duas aberturas simultâneas liam a mesma contagem e as duas inseriam, ultrapassando o limite: é o write skew causado por phantom, em que a escrita de uma muda o resultado da busca da outra sem que as duas toquem num documento em comum. A versão do ADR 0005 não ajuda aqui, porque protege um documento que já existe, e o conflito está no dossiê que ainda vai ser criado.

A solução foi materializar o conflito num documento por olheiro na coleção `contadores_de_acompanhamentos`. A abertura reserva uma vaga com um único update atômico, `$inc: 1` com `ativos < limite` no filtro; nenhum documento modificado significa limite atingido. A reserva ocupa o lugar da antiga verificação, e o resto da abertura (busca do perfil, validação e inserção) roda num `try/catch` que devolve a vaga com `$inc: -1` em qualquer falha. O contador nasce na primeira reserva do olheiro, por upsert com `$setOnInsert`, já com a contagem real dos dossiês ativos.

## Considered Options

- **Isolamento serializable.** É a solução que o DDIA indica antes de materializar conflitos, mas o MongoDB não oferece: o nível mais forte das transações é snapshot isolation, que não impede write skew.
- **Transação multi-documento com escrita forçada num documento de lock** (o padrão `findOneAndUpdate` com `lockId` da documentação do MongoDB). Rejeitado: também materializa o conflito, e ainda exige replica set, que nem o `AddMongoDB` do AppHost nem o `docker-compose.yml` configuram. Mais uma infraestrutura para o mesmo resultado do contador.
- **Lock distribuído no Redis.** Rejeitado: o Redis já está na stack, mas é controle pessimista fora do banco que guarda os dados, com expiração e liberação do lock para tratar, e protege só quem lembrar de pegar o lock.
- **Manter a contagem e aceitar a violação rara.** Rejeitado: o limite é regra de negócio (R1.7), não uma estimativa.
- **Contagem prévia como fail-fast, mais o contador como garantia.** Rejeitado pelo mesmo motivo do ADR 0005: duas verificações da mesma regra para manter. Como a reserva acontece no lugar da contagem, a ordem dos erros da abertura não mudou.

## Consequences

- A verificação do limite saiu do domain service e passou para o repositório (`ReservarVagaDeAcompanhamentoAsync`). O use case decide o que fazer com a resposta, mas a regra só é garantida porque a condição está no filtro do update.
- Todo caminho que tira um dossiê de ativo precisa devolver a vaga. Hoje só a exclusão do usuário faz isso (apaga dossiês e contador). Não há endpoint de encerramento (`AtualizarAsync` não tem chamador), e quando houver ele precisa chamar `LiberarVagaDeAcompanhamentoAsync`, senão o contador só sobe e o olheiro fica bloqueado.
- Contador e dossiê são documentos diferentes, sem transação entre eles. Se o processo cair entre a reserva e a inserção, a vaga fica presa. A recontagem não é automática: corrigir exige recalcular o contador a partir dos dossiês ativos.
- Dados anteriores à mudança não precisam de migração: o contador é criado na primeira reserva com a contagem real.
- O mesmo raciocínio vale para a outra regra que abrange vários dossiês (R1.2, mesmo jogador acompanhado duas vezes), mas ali a garantia já existia: o índice único parcial `uniq_olheiro_jogador_ativo`. A chave duplicada na inserção agora é traduzida para `409 acompanhamento.jogador_ja_acompanhado`, em vez de chegar como erro 500, e a vaga reservada é devolvida.
- O comportamento concorrente é testado no use case, com o repositório em memória. O update condicional real e o upsert do contador só serão exercitados nos testes de integração (item 5.1 do plano de evolução).

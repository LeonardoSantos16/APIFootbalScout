# Rotação do refresh token com revogação condicional antes da emissão

O refresh lia o token, emitia a nova sessão e só depois revogava o token antigo, ignorando se a revogação tinha acontecido. Duas requisições com o mesmo token liam o token como ativo e as duas saíam com uma sessão válida; um sign-out no meio do refresh também não impedia a emissão. A revogação passou a ser o ponto de decisão: `RevogarAsync` faz o update filtrando por `RevokedAtUtc == null` e devolve se modificou o documento, e só uma requisição consegue fazer esse campo deixar de ser nulo. A ordem do refresh virou gerar os tokens, revogar o antigo já apontando o hash do novo (`ReplacedByHash`), e gravar o novo só se a revogação passou. Para isso o `AuthSessionIssuer` foi separado em `GerarTokens` e `GravarAsync`; o `EmitirAsync` continua compondo as duas etapas para login, cadastro e troca de senha.

## Considered Options

- **Perdedor da corrida tratado como reuso de token, revogando todas as sessões do usuário.** Rejeitado: um duplo clique ou duas abas renovando juntas derrubariam o usuário legítimo. Quem perde a corrida recebe só `401 usuario.refresh_token_invalido`, e as outras sessões continuam valendo.
- **Revogar sem o hash do novo token e preencher `ReplacedByHash` num segundo update depois da emissão**, mantendo o `EmitirAsync` inteiro. Rejeitado: entre os dois updates o token aparece revogado sem substituto, igual a um sign-out, e um reuso nessa janela não dispararia a revogação geral.
- **Manter a ordem antiga e revogar a sessão recém-emitida quando a revogação do token antigo falhar.** Rejeitado: compensar depois de gravar mantém a janela em que as duas sessões existem, e a compensação também pode falhar.

## Consequences

- A falha passou a ser fechada. Se a gravação do token novo falhar depois da revogação, o usuário perde a sessão e precisa fazer login de novo; antes, uma falha na revogação deixava duas sessões válidas.
- A detecção de reuso continua valendo para um token que chega já rotacionado: a revogação de todas as sessões só acontece nesse caso. Um token roubado usado no mesmo instante que o legítimo cai no caminho do perdedor da corrida e não aciona a revogação geral. A janela é a duração de uma requisição, e quem chega depois dela é detectado normalmente.
- `refresh_tokens` ganhou índice único em `token_hash`, criado no startup. O `ObterPorHashAsync` de todo refresh e sign-out deixa de varrer a coleção, e dois documentos não podem ter o mesmo hash.
- Os testes de concorrência ficam no use case, com o repositório em memória devolvendo cópias do documento e um bloqueio entre a leitura e a revogação. O update condicional real e o índice só serão exercitados nos testes de integração (item 5.1 do plano de evolução).

# Cache da fonte externa só para jogadores e torneios principais

A API do SofaScore é paga por consumo, mas guardar no Redis todo jogador e torneio consultado encheria o cache de dados que quase ninguém volta a pedir. Por isso, a maior parte das respostas só vai para o cache quando o jogador ou o torneio está nas listas `PrincipaisJogadores` e `PrincipaisTorneios` do `ScoutConfig`, verificadas pelas specifications `JogadorPrincipal` e `TorneioPrincipal`. Os demais são consultados direto na fonte. As exceções são o perfil completo do jogador e a página completa do torneio, que ficam 12 horas no cache para qualquer um, e a lista de temporadas de um torneio, que fica 2 horas.

## Considered Options

- **Cache para tudo, com o mesmo tempo de vida.** Rejeitado: o tempo de vida longo que compensa para os jogadores mais consultados deixaria desatualizado o dado dos demais, e um tempo de vida curto pouco economizaria.
- **Sem cache.** Rejeitado: as telas mais acessadas consultariam a fonte paga a cada requisição.

## Consequences

- Colocar um jogador ou torneio no cache é uma mudança de configuração, não de código.
- Consultar um jogador fora da lista gasta cota da fonte a cada requisição.

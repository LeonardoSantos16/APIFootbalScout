using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Domain.Analise.ValueObject
{
    public sealed record JogadorParaComparar(PerfilDoJogador Perfil, ConjuntoDeEstatisticas Estatisticas);
}

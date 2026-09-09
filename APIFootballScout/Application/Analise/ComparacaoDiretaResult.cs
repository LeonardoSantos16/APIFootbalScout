using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Application.Analise
{
    public sealed record JogadorNaComparacao(int JogadorId, string Nome, Posicao? Posicao);

    public sealed record ComparacaoDiretaResult(
        IReadOnlyCollection<JogadorNaComparacao> Jogadores,
        ResultadoDaComparacao Resultado);
}

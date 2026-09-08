using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Domain.Analise.ValueObject
{
    public sealed record ComparacaoRealizada(
        Recorte Recorte,
        IReadOnlyCollection<AtributoComparado> Atributos) : ResultadoDaComparacao;
}

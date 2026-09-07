using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Domain.Analise.ValueObject
{
    public sealed record ConjuntoDeEstatisticas(
        Recorte Recorte,
        IReadOnlyCollection<EstatisticaAcumulavel> Acumulaveis,
        IReadOnlyCollection<ValorDerivado> Derivados);
}

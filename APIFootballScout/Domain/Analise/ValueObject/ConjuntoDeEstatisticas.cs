using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Domain.Analise.ValueObject
{
    public sealed record ConjuntoDeEstatisticas
    {
        public Recorte Recorte { get; init; }
        public IReadOnlyCollection<EstatisticaAcumulavel> Acumulaveis { get; init; }
        public IReadOnlyCollection<ValorDerivado> Derivados { get; init; }

        public ConjuntoDeEstatisticas(
            Recorte recorte,
            IReadOnlyCollection<EstatisticaAcumulavel> acumulaveis,
            IReadOnlyCollection<ValorDerivado> derivados)
        {
            if (acumulaveis.Any(estatistica => estatistica.Minutagem.Recorte != recorte)
                || derivados.Any(derivado => derivado.Minutagem.Recorte != recorte))
            {
                throw new ValorInvalidoException(
                    "conjunto_de_estatisticas.recorte_divergente",
                    "Toda estatistica do conjunto precisa vir do recorte do conjunto.");
            }

            Recorte = recorte;
            Acumulaveis = acumulaveis;
            Derivados = derivados;
        }
    }
}

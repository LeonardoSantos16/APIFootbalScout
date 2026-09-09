using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Domain.Analise.ValueObject
{
    public sealed record ConjuntoDeEstatisticas
    {
        public Recorte Recorte { get; init; }
        public Minutagem Minutagem { get; init; }
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

            var minutagens = acumulaveis.Select(estatistica => estatistica.Minutagem)
                .Concat(derivados.Select(derivado => derivado.Minutagem))
                .Distinct()
                .ToList();

            if (minutagens.Count == 0)
            {
                throw new ValorInvalidoException(
                    "conjunto_de_estatisticas.vazio",
                    "O conjunto precisa de ao menos uma estatistica.");
            }

            if (minutagens.Count > 1)
            {
                throw new ValorInvalidoException(
                    "conjunto_de_estatisticas.minutagem_divergente",
                    "O conjunto precisa ser sustentado por uma unica amostra de minutos.");
            }

            Recorte = recorte;
            Minutagem = minutagens.First();
            Acumulaveis = acumulaveis;
            Derivados = derivados;
        }
    }
}

using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.Specifications;

namespace APIFootballScout.Domain.Analise.ValueObject
{
    public sealed record EstatisticaAcumulavel(int Contagem, Minutagem Minutagem)
    {
        public MetricaPor90 PorNoventaMinutos(AmostraSuficienteSpecification amostraSuficiente)
            => throw new NotImplementedException();
    }
}

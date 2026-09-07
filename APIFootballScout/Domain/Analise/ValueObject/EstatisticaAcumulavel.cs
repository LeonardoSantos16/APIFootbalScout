using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.Specifications;

namespace APIFootballScout.Domain.Analise.ValueObject
{
    public sealed record EstatisticaAcumulavel(TipoDeEstatistica Tipo, int Contagem, Minutagem Minutagem)
    {
        private const int MinutosDeUmaPartida = 90;

        public MetricaPor90 PorNoventaMinutos(AmostraSuficienteSpecification amostraSuficiente)
            => amostraSuficiente.IsSatisfiedBy(Minutagem)
                ? new MetricaCalculada((decimal)Contagem * MinutosDeUmaPartida / Minutagem.Minutos, Minutagem)
                : new CalculoRecusado(MotivoDaRecusa.AmostraInsuficiente);
    }
}

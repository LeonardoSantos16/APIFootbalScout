using APIFootballScout.Domain.Acompanhamento.ValueObject;

namespace APIFootballScout.Domain.Analise.ValueObject
{
    public sealed record MetricaCalculada(decimal Valor, Minutagem Amostra) : MetricaPor90;
}

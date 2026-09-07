using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Application.Analise
{
    public sealed record MetricaDoJogador(TipoDeEstatistica Tipo, MetricaPor90 Metrica);

    public sealed record ConsultarMetricasPor90Result(
        int JogadorId,
        Recorte Recorte,
        IReadOnlyCollection<MetricaDoJogador> Metricas,
        IReadOnlyCollection<ValorDerivado> Derivados);
}

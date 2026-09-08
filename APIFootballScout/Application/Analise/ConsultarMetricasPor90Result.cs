using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Application.Analise
{
    public sealed record AtributoDoJogador(TipoDeAtributo Tipo, ResultadoDeAtributo Resultado);

    public sealed record ConsultarMetricasPor90Result(
        int JogadorId,
        Recorte Recorte,
        IReadOnlyCollection<AtributoDoJogador> Atributos);
}

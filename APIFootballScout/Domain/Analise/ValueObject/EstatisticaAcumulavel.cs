using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.Specifications;

namespace APIFootballScout.Domain.Analise.ValueObject
{
    public sealed record EstatisticaAcumulavel(TipoDeAtributo Tipo, int Contagem, Minutagem Minutagem)
    {
        private const int MinutosDeUmaPartida = 90;

        public ResultadoDeAtributo PorNoventaMinutos(AmostraSuficienteSpecification amostraSuficiente)
            => amostraSuficiente.IsSatisfiedBy(Minutagem)
                ? new AtributoCalculado((decimal)Contagem * MinutosDeUmaPartida / Minutagem.Minutos, Minutagem)
                : new AtributoRecusado(MotivoDaRecusa.AmostraInsuficiente);
    }
}

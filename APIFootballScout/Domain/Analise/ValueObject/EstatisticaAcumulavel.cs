using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.Specifications;
using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Domain.Analise.ValueObject
{
    public sealed record EstatisticaAcumulavel
    {
        private const int MinutosDeUmaPartida = 90;

        public TipoDeAtributo Tipo { get; init; }
        public int Contagem { get; init; }
        public Minutagem Minutagem { get; init; }

        public EstatisticaAcumulavel(TipoDeAtributo tipo, int contagem, Minutagem minutagem)
        {
            if (tipo.EhDerivado())
            {
                throw new ValorInvalidoException(
                    "estatistica_acumulavel.tipo_nao_acumulavel",
                    "Valor derivado nao e estatistica acumulavel.");
            }

            Tipo = tipo;
            Contagem = contagem;
            Minutagem = minutagem;
        }

        public ResultadoDeAtributo PorNoventaMinutos(AmostraSuficienteSpecification amostraSuficiente)
            => amostraSuficiente.IsSatisfiedBy(Minutagem)
                ? new AtributoCalculado((decimal)Contagem * MinutosDeUmaPartida / Minutagem.Minutos, Minutagem)
                : new AtributoRecusado(MotivoDaRecusa.AmostraInsuficiente);
    }
}

using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Domain.Analise.ValueObject
{
    public sealed record AtributoCalculado : ResultadoDeAtributo
    {
        public decimal Valor { get; init; }
        public Minutagem Amostra { get; init; }

        public AtributoCalculado(decimal valor, Minutagem amostra)
        {
            if (amostra.Minutos <= 0)
            {
                throw new ValorInvalidoException(
                    "atributo.amostra_nao_sustenta",
                    "A metrica precisa declarar uma amostra de minutos positiva.");
            }

            Valor = valor;
            Amostra = amostra;
        }
    }
}

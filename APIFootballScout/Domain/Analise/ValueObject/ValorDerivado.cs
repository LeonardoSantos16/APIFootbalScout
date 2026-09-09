using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Domain.Analise.ValueObject
{
    public sealed record ValorDerivado
    {
        public TipoDeAtributo Tipo { get; init; }
        public decimal? Valor { get; init; }
        public Minutagem Minutagem { get; init; }

        public ValorDerivado(TipoDeAtributo tipo, decimal? valor, Minutagem minutagem)
        {
            if (!tipo.EhDerivado())
            {
                throw new ValorInvalidoException(
                    "valor_derivado.tipo_nao_derivado",
                    "Estatistica acumulavel nao e valor derivado.");
            }

            Tipo = tipo;
            Valor = valor;
            Minutagem = minutagem;
        }

        public ResultadoDeAtributo Resultado()
        {
            if (Valor is null)
            {
                return new AtributoRecusado(MotivoDaRecusa.FonteNaoAtribuiu);
            }

            if (Minutagem.Minutos <= 0)
            {
                return new AtributoRecusado(MotivoDaRecusa.AmostraInsuficiente);
            }

            return new AtributoCalculado(Valor.Value, Minutagem);
        }
    }
}

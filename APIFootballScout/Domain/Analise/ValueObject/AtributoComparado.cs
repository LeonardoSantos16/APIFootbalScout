using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Domain.Analise.ValueObject
{
    public sealed record AtributoComparado
    {
        public TipoDeAtributo Tipo { get; init; }
        public IReadOnlyDictionary<int, ResultadoDeAtributo> Valores { get; init; }

        public AtributoComparado(TipoDeAtributo tipo, IReadOnlyDictionary<int, ResultadoDeAtributo> valores)
        {
            if (valores.Count != 2)
            {
                throw new ValorInvalidoException(
                    "atributo_comparado.par_incompleto",
                    "O atributo comparado exige exatamente dois jogadores.");
            }

            Tipo = tipo;
            Valores = valores;
        }
    }
}

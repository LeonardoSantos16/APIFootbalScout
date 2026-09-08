using APIFootballScout.Domain.Acompanhamento.ValueObject;

namespace APIFootballScout.Domain.Analise.ValueObject
{
    public sealed record ValorDerivado(TipoDeAtributo Tipo, decimal? Valor, Minutagem Minutagem)
    {
        public ResultadoDeAtributo Resultado() => throw new NotImplementedException();
    }
}

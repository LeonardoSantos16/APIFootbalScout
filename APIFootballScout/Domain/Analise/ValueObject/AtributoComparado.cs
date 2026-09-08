namespace APIFootballScout.Domain.Analise.ValueObject
{
    public sealed record AtributoComparado
    {
        public TipoDeAtributo Tipo { get; init; }
        public IReadOnlyDictionary<int, ResultadoDeAtributo> Valores { get; init; }

        public AtributoComparado(TipoDeAtributo tipo, IReadOnlyDictionary<int, ResultadoDeAtributo> valores)
        {
            Tipo = tipo;
            Valores = valores;
        }
    }
}

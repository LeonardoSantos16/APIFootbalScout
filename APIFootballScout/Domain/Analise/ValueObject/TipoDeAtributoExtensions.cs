namespace APIFootballScout.Domain.Analise.ValueObject
{
    public static class TipoDeAtributoExtensions
    {
        private static readonly TipoDeAtributo[] Derivados =
        [
            TipoDeAtributo.Rating,
            TipoDeAtributo.PrecisaoDePasse
        ];

        public static bool EhDerivado(this TipoDeAtributo tipo) => Derivados.Contains(tipo);
    }
}

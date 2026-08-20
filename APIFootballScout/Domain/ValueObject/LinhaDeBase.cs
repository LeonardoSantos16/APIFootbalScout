namespace APIFootballScout.Domain.ValueObject
{
    public sealed record LinhaDeBase(
        DateTime MedidaEm,
        string Clube,
        Dinheiro ValorDeMercado,
        Minutagem Minutagem)
    {
        public LinhaDeBase(string Clube, Dinheiro ValorDeMercado, Minutagem Minutagem)
            : this(DateTime.UtcNow, Clube, ValorDeMercado, Minutagem) { }
    }
}

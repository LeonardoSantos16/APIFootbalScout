namespace APIFootballScout.Domain.ValueObject
{
    public sealed record LinhaDeBase(
        DateTime MedidaEm,
        string Clube,
        Dinheiro ValorDeMercado,
        Minutagem Minutagem);
}

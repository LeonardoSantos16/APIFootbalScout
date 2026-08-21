using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Domain.Acompanhamento.ValueObject
{
    public sealed record LinhaDeBase(
        DateTime MedidaEm,
        string Clube,
        Dinheiro ValorDeMercado,
        Minutagem Minutagem);
}

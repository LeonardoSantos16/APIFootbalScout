using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Domain.Acompanhamento.ValueObject
{
    public sealed record MudancaDeValorDeMercado(Dinheiro Anterior, Dinheiro Atual)
    : MudancaQuantitativa
    {
        public decimal VariacaoPercentualAbsoluta =>
            Math.Abs((Atual.QuantiaEmCentavos - Anterior.QuantiaEmCentavos) * 100m
            / Anterior.QuantiaEmCentavos);
    }
}

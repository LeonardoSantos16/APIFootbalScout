using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Domain.SharedKernel
{
    public sealed record Dinheiro(long QuantiaEmCentavos, string Moeda)
    {
        public decimal VariacaoPercentualAbsolutaEmRelacaoA(Dinheiro anterior)
        {
            if (Moeda != anterior.Moeda)
                throw new ValorInvalidoException(
                    "dinheiro.moedas_distintas",
                    "Amounts in different currencies cannot be compared.");

            return Math.Abs((QuantiaEmCentavos - anterior.QuantiaEmCentavos) * 100m
                / anterior.QuantiaEmCentavos);
        }
    }
}

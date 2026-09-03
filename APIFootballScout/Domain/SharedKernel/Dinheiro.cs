using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Domain.SharedKernel
{
    public sealed record Dinheiro(long QuantiaEmCentavos, string Moeda)
    {
        public Dinheiro Somar(Dinheiro outro)
        {
            if (Moeda != outro.Moeda)
                throw new ValorInvalidoException(
                    "dinheiro.moedas_distintas",
                    $"Não é possível somar valores em {Moeda} e {outro.Moeda}.");

            return new Dinheiro(outro.QuantiaEmCentavos + QuantiaEmCentavos, Moeda);
        }

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

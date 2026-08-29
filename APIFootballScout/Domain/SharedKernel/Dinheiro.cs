namespace APIFootballScout.Domain.SharedKernel
{
    public sealed record Dinheiro(long QuantiaEmCentavos, string Moeda)
    {
        public decimal VariacaoPercentualAbsolutaEmRelacaoA(Dinheiro anterior) =>
            throw new NotImplementedException();
    }
}

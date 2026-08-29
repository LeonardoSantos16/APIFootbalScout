namespace APIFootballScout.Domain.Acompanhamento.ValueObject
{
    public sealed record JanelaDaComparacao
    {
        public DateTime De { get; init; }
        public DateTime Ate { get; init; }

        public JanelaDaComparacao(DateTime de, DateTime ate)
        {
            De = de;
            Ate = ate;
        }

        public TimeSpan Duracao() => throw new NotImplementedException();
    }
}

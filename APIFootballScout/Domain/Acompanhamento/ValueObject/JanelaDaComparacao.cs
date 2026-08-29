using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Domain.Acompanhamento.ValueObject
{
    public sealed record JanelaDaComparacao
    {
        public DateTime De { get; init; }
        public DateTime Ate { get; init; }

        public JanelaDaComparacao(DateTime de, DateTime ate)
        {
            if (de >= ate)
            {
                throw new ValorInvalidoException("janela_da_comparacao.intervalo_invalido", "Data invalida");
            }

            De = de;
            Ate = ate;
        }

        public TimeSpan Duracao() => Ate - De;
    }
}

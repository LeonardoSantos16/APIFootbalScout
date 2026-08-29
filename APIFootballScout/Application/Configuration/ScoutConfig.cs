namespace APIFootballScout.Application.Configuration
{
    public sealed class ScoutConfig
    {
        public int[] PrincipaisTorneios { get; init; } = [];
        public int[] PrincipaisJogadores { get; init; } = [];
        public int LimiteObservacoesJogadores { get; init; }
        public int LimiarValorDeMercadoPercentual { get; init; }
        public int LimiarMinutagemMinutos { get; init; }
    }
}

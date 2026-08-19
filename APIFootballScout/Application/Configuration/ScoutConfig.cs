namespace APIFootballScout.Application.Configuration
{
    public sealed class ScoutConfig
    {
        public int[] PrincipaisTorneios { get; init; } = [];
        public int[] PrincipaisJogadores { get; init; } = [];
        public int LimiteObservacoesJogadores { get; init; }
    }
}

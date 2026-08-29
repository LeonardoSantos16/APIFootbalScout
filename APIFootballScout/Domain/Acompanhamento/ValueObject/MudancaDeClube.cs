namespace APIFootballScout.Domain.Acompanhamento.ValueObject
{
    public sealed record MudancaDeClube(string Anterior, string Atual) : MudancaCategorica(Anterior, Atual);
}

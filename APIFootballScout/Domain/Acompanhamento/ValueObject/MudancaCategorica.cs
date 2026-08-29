namespace APIFootballScout.Domain.Acompanhamento.ValueObject
{
    public abstract record MudancaCategorica(string Anterior, string Atual) : ComMudanca
    {
    }
}

namespace APIFootballScout.Application.ShortlistPersonalizada
{
    public sealed record RemoverAlvoRequest(Guid OlheiroId, Guid ShortlistId, int JogadorId);
}

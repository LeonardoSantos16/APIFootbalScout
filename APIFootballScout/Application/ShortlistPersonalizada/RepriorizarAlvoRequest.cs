namespace APIFootballScout.Application.ShortlistPersonalizada
{
    public sealed record RepriorizarAlvoRequest(
        Guid OlheiroId,
        Guid ShortlistId,
        int JogadorId,
        int Prioridade);
}

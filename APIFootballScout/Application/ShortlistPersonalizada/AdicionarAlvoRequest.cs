using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Application.ShortlistPersonalizada
{
    public sealed record AdicionarAlvoRequest(
        Guid OlheiroId,
        Guid ShortlistId,
        int JogadorId,
        int Prioridade,
        Dinheiro CustoEstimado);
}

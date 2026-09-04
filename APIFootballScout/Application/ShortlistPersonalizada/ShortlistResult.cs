using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Application.ShortlistPersonalizada
{
    public sealed record ShortlistResult(
        Guid ShortlistId,
        Guid OlheiroId,
        string Nome,
        int LimiteDeAlvos,
        IReadOnlyList<AlvoResult> Alvos,
        Dinheiro? CustoTotal);

    public sealed record AlvoResult(int JogadorId, int Prioridade, Dinheiro CustoEstimado);
}

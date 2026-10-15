using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Domain.ShortlistPersonalizada.ValueObject;

namespace APIFootballScout.Domain.ShortlistPersonalizada.Aggregate
{
    public sealed record Alvo(int JogadorId, Prioridade Prioridade, Dinheiro CustoEstimado);
}
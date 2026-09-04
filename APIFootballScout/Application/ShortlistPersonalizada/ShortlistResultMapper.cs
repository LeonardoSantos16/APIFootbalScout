using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;

namespace APIFootballScout.Application.ShortlistPersonalizada
{
    internal static class ShortlistResultMapper
    {
        public static ShortlistResult ParaResult(this Shortlist shortlist) => new(
            ShortlistId: shortlist.Id,
            OlheiroId: shortlist.OlheiroId,
            Nome: shortlist.Nome,
            LimiteDeAlvos: shortlist.Limite.Valor,
            Alvos: shortlist.Alvos.Select(a => a.ParaResult()).ToList(),
            CustoTotal: shortlist.CustoTotal);

        public static AlvoResult ParaResult(this Alvo alvo) => new(
            JogadorId: alvo.JogadorId,
            Prioridade: alvo.Prioridade.Valor,
            CustoEstimado: alvo.CustoEstimado);

        public static List<ShortlistResult> ParaResult(this IEnumerable<Shortlist> shortlists) => [.. shortlists.Select(ParaResult)];
    }
}

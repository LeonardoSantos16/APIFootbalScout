using APIFootballScout.Application.ShortlistPersonalizada;
using APIFootballScout.Contracts.Acompanhamento;
using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;

namespace APIFootballScout.Contracts.ShortlistPersonalizada
{
    public static class ShortlistDtoMappers
    {
        public static CriarShortlistRequest ParaRequest(
            this CriarShortlistRequestDto dto, Guid olheiroId)
            => new(olheiroId, dto.Nome);

        public static RemoverAlvoRequest ParaRequest(Guid olheiroId, Guid shortlistId, int jogadorId)
            => new(olheiroId, shortlistId, jogadorId);

        public static ObterShortlistRequest ParaRequest(Guid olheiroId, Guid shortlistId)
            => new(olheiroId, shortlistId);

        public static ListarShortlistsDoOlheiroRequest ParaRequest(Guid olheiroId)
            => new(olheiroId);

        public static AdicionarAlvoRequest ParaRequest(
            this AdicionarAlvoRequestDto dto, Guid olheiroId, Guid shortlistId)
            => new(olheiroId, shortlistId, dto.JogadorId, dto.Prioridade, new Dinheiro(dto.CustoEstimado.QuantiaEmCentavos, dto.CustoEstimado.Moeda));

        public static RepriorizarAlvoRequest ParaRequest(
            this RepriorizarAlvoRequestDto dto, Guid olheiroId, Guid shortlistId, int jogadorId)
            => new(olheiroId, shortlistId, jogadorId, dto.Prioridade);

        public static ShortlistResponseDto ParaResponse(this ShortlistResult result)
            => new(result.ShortlistId, result.Nome, result.LimiteDeAlvos, ParaDto(result.Alvos), ParaDtoOuNulo(result.CustoTotal));

        public static IReadOnlyList<ShortlistResponseDto> ParaResponse(
            this IEnumerable<ShortlistResult> resultados)
            => [.. resultados.Select(resultado => ParaResponse(resultado))];

        private static DinheiroDto ParaDto(this Dinheiro dinheiro)
            => new(dinheiro.QuantiaEmCentavos, dinheiro.Moeda);

        private static DinheiroDto? ParaDtoOuNulo(this Dinheiro? dinheiro)
            => dinheiro is null ? null : dinheiro.ParaDto();

        private static List<AlvoResponseDto> ParaDto(this IEnumerable<AlvoResult> alvos)
        {
            return [.. alvos.Select(a => new AlvoResponseDto(a.JogadorId, a.Prioridade, ParaDto(a.CustoEstimado)))];
        }
    }
}

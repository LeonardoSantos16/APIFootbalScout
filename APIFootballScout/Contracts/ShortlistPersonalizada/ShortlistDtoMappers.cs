using APIFootballScout.Application.ShortlistPersonalizada;

namespace APIFootballScout.Contracts.ShortlistPersonalizada
{
    public static class ShortlistDtoMappers
    {
        public static CriarShortlistRequest ParaRequest(
            this CriarShortlistRequestDto dto, Guid olheiroId)
            => throw new NotImplementedException();

        public static AdicionarAlvoRequest ParaRequest(
            this AdicionarAlvoRequestDto dto, Guid olheiroId, Guid shortlistId)
            => throw new NotImplementedException();

        public static RepriorizarAlvoRequest ParaRequest(
            this RepriorizarAlvoRequestDto dto, Guid olheiroId, Guid shortlistId, int jogadorId)
            => throw new NotImplementedException();

        public static ShortlistResponseDto ParaResponse(this ShortlistResult result)
            => throw new NotImplementedException();

        public static IReadOnlyList<ShortlistResponseDto> ParaResponse(
            this IEnumerable<ShortlistResult> resultados)
            => throw new NotImplementedException();
    }
}

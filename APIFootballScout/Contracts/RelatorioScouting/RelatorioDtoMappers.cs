using APIFootballScout.Application.RelatorioScouting;

namespace APIFootballScout.Contracts.RelatorioScouting
{
    public static class RelatorioDtoMappers
    {
        public static AbrirRascunhoRelatorioRequest ParaRequest(
            this AbrirRascunhoRelatorioRequestDto dto, Guid olheiroId)
            => throw new NotImplementedException();

        public static EditarRascunhoRelatorioRequest ParaRequest(
            this EditarRascunhoRelatorioRequestDto dto, Guid olheiroId, Guid relatorioId)
            => throw new NotImplementedException();

        public static RelatorioResponseDto ParaResponse(this RelatorioResult result)
            => throw new NotImplementedException();
    }
}

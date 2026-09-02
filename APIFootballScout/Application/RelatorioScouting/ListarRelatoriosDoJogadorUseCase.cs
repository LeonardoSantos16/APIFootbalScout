using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.RelatorioScouting
{
    public class ListarRelatoriosDoJogadorUseCase(IRelatorioRepository relatorioRepository)
    {
        public async Task<IReadOnlyList<RelatorioResult>> ListarRelatorios(
            ListarRelatoriosDoJogadorRequest request, CancellationToken cancellationToken)
        {
            var relatorios = await relatorioRepository.ListarPorJogadorAsync(
                request.JogadorId, request.OlheiroId, cancellationToken);

            return [.. relatorios
                .OrderByDescending(r => r.ObservadoEm)
                .Select(r => r.ParaResult())];
        }
    }
}

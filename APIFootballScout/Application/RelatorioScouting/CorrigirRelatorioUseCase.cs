using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.RelatorioScouting
{
    public class CorrigirRelatorioUseCase(IRelatorioRepository relatorioRepository, TimeProvider tempo)
    {
        public Task<RelatorioResult> CorrigirRelatorio(
            CorrigirRelatorioRequest request, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}

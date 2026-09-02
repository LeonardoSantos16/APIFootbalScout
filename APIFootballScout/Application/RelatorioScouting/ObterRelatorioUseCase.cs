using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.RelatorioScouting
{
    public class ObterRelatorioUseCase(IRelatorioRepository relatorioRepository)
    {
        public Task<RelatorioResult> ObterRelatorio(
            ObterRelatorioRequest request, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}

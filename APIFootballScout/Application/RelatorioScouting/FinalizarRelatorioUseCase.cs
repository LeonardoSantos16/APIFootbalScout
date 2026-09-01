using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.RelatorioScouting
{
    public class FinalizarRelatorioUseCase(
        IRelatorioRepository relatorioRepository,
        ScoutSpecificationFactory especificacoes,
        TimeProvider tempo)
    {
        public Task<RelatorioResult> FinalizarRelatorio(
            FinalizarRelatorioRequest request, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}

using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.RelatorioScouting
{
    public class EditarRascunhoRelatorioUseCase(IRelatorioRepository relatorioRepository)
    {
        public Task<RelatorioResult> EditarRascunho(
            EditarRascunhoRelatorioRequest request, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}

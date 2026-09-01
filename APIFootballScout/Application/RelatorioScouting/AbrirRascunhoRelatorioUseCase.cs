using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.RelatorioScouting
{
    public class AbrirRascunhoRelatorioUseCase(IRelatorioRepository relatorioRepository, TimeProvider tempo)
    {
        public Task<RelatorioResult> AbrirRascunho(
            AbrirRascunhoRelatorioRequest request, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}

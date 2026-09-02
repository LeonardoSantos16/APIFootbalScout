using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.RelatorioScouting
{
    public class ListarRelatoriosDoJogadorUseCase(IRelatorioRepository relatorioRepository)
    {
        public Task<IReadOnlyList<RelatorioResult>> ListarRelatorios(
            ListarRelatoriosDoJogadorRequest request, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}

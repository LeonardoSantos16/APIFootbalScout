using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.RelatorioScouting
{
    public class ObterRelatorioUseCase(IRelatorioRepository relatorioRepository)
    {
        public async Task<RelatorioResult> ObterRelatorio(
            ObterRelatorioRequest request, CancellationToken cancellationToken)
        {
            var relatorio = await relatorioRepository.ObterPorIdAsync(
                request.RelatorioId, request.OlheiroId, cancellationToken)
                ?? throw new RecursoNaoEncontradoException(
                    "relatorio.nao_encontrado",
                    "O relatório não foi encontrado.");

            return relatorio.ParaResult();
        }
    }
}

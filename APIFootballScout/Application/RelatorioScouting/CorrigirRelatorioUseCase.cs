using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.RelatorioScouting
{
    public class CorrigirRelatorioUseCase(IRelatorioRepository relatorioRepository, TimeProvider tempo)
    {
        public async Task<RelatorioResult> CorrigirRelatorio(
            CorrigirRelatorioRequest request, CancellationToken cancellationToken)
        {
            var original = await relatorioRepository.ObterPorIdAsync(
                request.RelatorioId, request.OlheiroId, cancellationToken)
                ?? throw new RecursoNaoEncontradoException(
                    "relatorio.nao_encontrado",
                    "O relatório não foi encontrado.");

            var correcao = Relatorio.AbrirCorrecao(original, request.Texto, tempo.GetUtcNow());

            await relatorioRepository.AdicionarAsync(correcao, cancellationToken);

            return correcao.ParaResult();
        }
    }
}

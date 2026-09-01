using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;
using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.RelatorioScouting
{
    public class AbrirRascunhoRelatorioUseCase(IRelatorioRepository relatorioRepository, TimeProvider tempo)
    {
        public async Task<RelatorioResult> AbrirRascunho(
            AbrirRascunhoRelatorioRequest request, CancellationToken cancellationToken)
        {
            var relatorio = Relatorio.AbrirRascunho(
                request.JogadorId,
                request.OlheiroId,
                request.Texto,
                request.ObservadoEm,
                tempo.GetUtcNow()
                );

            await relatorioRepository.AdicionarAsync(relatorio, cancellationToken);

            return relatorio.ParaResult();
        }
    }
}

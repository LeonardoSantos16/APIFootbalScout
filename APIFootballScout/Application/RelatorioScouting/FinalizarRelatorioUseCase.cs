using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.RelatorioScouting.Specifications;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;
using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.RelatorioScouting
{
    public class FinalizarRelatorioUseCase
    {
        private readonly IRelatorioRepository _relatorioRepository;
        private readonly RelatorioComConteudoMinimoSpecification _relatorioComConteudoMinimoSpecification;
        private readonly TimeProvider _timeProvider;

        public FinalizarRelatorioUseCase(
            IRelatorioRepository relatorioRepository,
            ScoutSpecificationFactory especificacoes,
            TimeProvider timeProvider)
        {
            _relatorioRepository = relatorioRepository;
            _relatorioComConteudoMinimoSpecification = especificacoes.ConteudoMinimoDoRelatorio();
            _timeProvider = timeProvider;
        }

        public async Task<RelatorioResult> FinalizarRelatorio(
            FinalizarRelatorioRequest request, CancellationToken cancellationToken)
        {
            var relatorio = await _relatorioRepository.ObterPorIdAsync(
                request.RelatorioId, request.OlheiroId, cancellationToken);

            if (relatorio is null)
                throw new RecursoNaoEncontradoException(
                    "relatorio.nao_encontrado",
                    "O relatório não foi encontrado.");

            relatorio.Finalizar(_relatorioComConteudoMinimoSpecification, _timeProvider.GetUtcNow().UtcDateTime);

            await _relatorioRepository.AtualizarAsync(relatorio, cancellationToken);

            return relatorio.ParaResult();
        }
    }
}

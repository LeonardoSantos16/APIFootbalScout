using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;
using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.RelatorioScouting
{
    public class EditarRascunhoRelatorioUseCase(IRelatorioRepository relatorioRepository)
    {
        public async Task<RelatorioResult> EditarRascunho(
            EditarRascunhoRelatorioRequest request, CancellationToken cancellationToken)
        {
           var relatorio = await relatorioRepository.ObterPorIdAsync(request.RelatorioId, request.OlheiroId ,cancellationToken)
                ?? throw new RecursoNaoEncontradoException("relatorio.nao_encontrado", "O relatório não foi encontrado.");

            AplicarEdicao(relatorio, request);

            await relatorioRepository.AtualizarAsync(relatorio, cancellationToken);

            return relatorio.ParaResult();
        }

        private static void AplicarEdicao(Relatorio relatorio, EditarRascunhoRelatorioRequest request)
        {
            if (request.Texto is not null)
                relatorio.AlterarTexto(request.Texto);

            if (request.Nota is not null)
                relatorio.AtribuirNota(new Nota(request.Nota.Value));

            if (request.PontosPositivos is not null)
                relatorio.SubstituirPontosPositivos(request.PontosPositivos);

            if (request.PontosNegativos is not null)
                relatorio.SubstituirPontosNegativos(request.PontosNegativos);

            if (request.Parecer is not null)
                relatorio.DefinirParecer(request.Parecer.Value);
        }
    }
}

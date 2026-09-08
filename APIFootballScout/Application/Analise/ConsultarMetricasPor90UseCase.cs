using APIFootballScout.Domain.Analise.Specifications;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Application.Analise
{
    public class ConsultarMetricasPor90UseCase
    {
        private readonly ICatalogoDeJogador _catalogoDeJogador;
        private readonly AmostraSuficienteSpecification _amostraSuficiente;

        public ConsultarMetricasPor90UseCase(
            ICatalogoDeJogador catalogoDeJogador,
            AmostraSuficienteSpecification amostraSuficiente)
        {
            _catalogoDeJogador = catalogoDeJogador;
            _amostraSuficiente = amostraSuficiente;
        }

        public async Task<ConsultarMetricasPor90Result> ConsultarMetricasPor90(
            ConsultarMetricasPor90Request request,
            CancellationToken cancellationToken)
        {
            var recorte = new Recorte(request.CompeticaoId, request.TemporadaId, request.Contexto);

            var conjunto = await _catalogoDeJogador.ObterEstatisticas(request.JogadorId, recorte, cancellationToken)
                ?? throw new RecursoNaoEncontradoException(
                    "jogador.estatisticas_nao_encontradas",
                    "estatisticas do jogador nao encontradas para esse recorte");

            var metricas = conjunto.Acumulaveis
                .Select(estatistica => new AtributoDoJogador(
                    estatistica.Tipo,
                    estatistica.PorNoventaMinutos(_amostraSuficiente)))
                .ToList();

            return new ConsultarMetricasPor90Result(
                JogadorId: request.JogadorId,
                Recorte: conjunto.Recorte,
                Metricas: metricas,
                Derivados: conjunto.Derivados);
        }
    }
}

using APIFootballScout.Application.Analise;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.Specifications;
using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Tests.Acompanhamento;

namespace APIFootballScout.Tests.Analise
{
    public class FluxoDaConsultaDeMetricasUseCaseTests
    {
        private static readonly Recorte Brasileirao2024 = new(325, 63814, ContextoDeRecorte.Clube);

        private readonly CatalogoDeJogadorFake _catalogo = new();
        private readonly ConsultarMetricasPor90UseCase _useCase;

        public FluxoDaConsultaDeMetricasUseCaseTests()
        {
            _useCase = new ConsultarMetricasPor90UseCase(
                _catalogo,
                new AmostraSuficienteSpecification(new AmostraMinima(450)));
        }

        [Fact]
        public async Task A_consulta_normaliza_cada_acumulavel_e_preserva_os_derivados()
        {
            // Arrange
            _catalogo.Estatisticas = ConjuntoDaFonte(minutos: 2400);

            // Act
            var result = await _useCase.ConsultarMetricasPor90(Requisicao(), CancellationToken.None);

            // Assert
            Assert.Equal(Brasileirao2024, result.Recorte);
            Assert.All(result.Metricas, metrica => Assert.IsType<MetricaCalculada>(metrica.Metrica));
            Assert.Equal(
                new[] { TipoDeEstatistica.Gols, TipoDeEstatistica.Assistencias },
                result.Metricas.Select(metrica => metrica.Tipo));
            Assert.Equal(
                [new ValorDerivado(TipoDeValorDerivado.Rating, 7.42m)],
                result.Derivados);
        }

        [Fact]
        public async Task A_recusa_por_amostra_insuficiente_chega_ao_resultado_da_consulta()
        {
            // Arrange
            _catalogo.Estatisticas = ConjuntoDaFonte(minutos: 200);

            // Act
            var result = await _useCase.ConsultarMetricasPor90(Requisicao(), CancellationToken.None);

            // Assert
            Assert.All(result.Metricas, metrica =>
                Assert.Equal(
                    MotivoDaRecusa.AmostraInsuficiente,
                    Assert.IsType<CalculoRecusado>(metrica.Metrica).Motivo));
        }

        [Fact]
        public async Task Jogador_sem_estatisticas_no_recorte_pedido_nao_e_encontrado()
        {
            // Arrange
            _catalogo.Estatisticas = null;

            // Act
            var erro = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => _useCase.ConsultarMetricasPor90(Requisicao(), CancellationToken.None));

            // Assert
            Assert.Equal("jogador.estatisticas_nao_encontradas", erro.Codigo);
            Assert.Equal(Brasileirao2024, _catalogo.UltimoRecorte);
        }

        private static ConsultarMetricasPor90Request Requisicao()
            => new(JogadorId: 13812, CompeticaoId: 325, TemporadaId: 63814, Contexto: ContextoDeRecorte.Clube);

        private static ConjuntoDeEstatisticas ConjuntoDaFonte(int minutos)
        {
            var minutagem = new Minutagem(minutos, Brasileirao2024);

            return new ConjuntoDeEstatisticas(
                Brasileirao2024,
                [
                    new EstatisticaAcumulavel(TipoDeEstatistica.Gols, 12, minutagem),
                    new EstatisticaAcumulavel(TipoDeEstatistica.Assistencias, 7, minutagem)
                ],
                [new ValorDerivado(TipoDeValorDerivado.Rating, 7.42m)]);
        }
    }
}

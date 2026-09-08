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
        public async Task Acumulaveis_normalizados_e_derivados_chegam_numa_colecao_unica()
        {
            // Arrange
            _catalogo.Estatisticas = ConjuntoDaFonte(minutos: 2400);

            // Act
            var result = await _useCase.ConsultarMetricasPor90(Requisicao(), CancellationToken.None);

            // Assert
            Assert.Equal(Brasileirao2024, result.Recorte);
            Assert.Equal(
                new[] { TipoDeAtributo.Gols, TipoDeAtributo.Assistencias, TipoDeAtributo.Rating },
                result.Atributos.Select(atributo => atributo.Tipo));
            Assert.All(result.Atributos, atributo => Assert.IsType<AtributoCalculado>(atributo.Resultado));
        }

        [Fact]
        public async Task Amostra_insuficiente_recusa_os_acumulaveis_e_nao_o_derivado()
        {
            // Arrange — a amostra minima e politica nossa sobre a normalizacao; o derivado
            // chega da fonte com o corte dela, e nao passa pela nossa.
            _catalogo.Estatisticas = ConjuntoDaFonte(minutos: 200);

            // Act
            var result = await _useCase.ConsultarMetricasPor90(Requisicao(), CancellationToken.None);

            // Assert
            Assert.All(
                result.Atributos.Where(atributo => atributo.Tipo != TipoDeAtributo.Rating),
                atributo => Assert.Equal(
                    MotivoDaRecusa.AmostraInsuficiente,
                    Assert.IsType<AtributoRecusado>(atributo.Resultado).Motivo));

            var rating = Assert.Single(result.Atributos, atributo => atributo.Tipo == TipoDeAtributo.Rating);
            Assert.Equal(7.42m, Assert.IsType<AtributoCalculado>(rating.Resultado).Valor);
        }

        [Fact]
        public async Task O_derivado_que_a_fonte_nao_atribuiu_chega_como_recusa()
        {
            // Arrange
            _catalogo.Estatisticas = ConjuntoDaFonte(minutos: 2400, rating: null);

            // Act
            var result = await _useCase.ConsultarMetricasPor90(Requisicao(), CancellationToken.None);

            // Assert
            var rating = Assert.Single(result.Atributos, atributo => atributo.Tipo == TipoDeAtributo.Rating);
            Assert.Equal(
                MotivoDaRecusa.FonteNaoAtribuiu,
                Assert.IsType<AtributoRecusado>(rating.Resultado).Motivo);
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

        private static ConjuntoDeEstatisticas ConjuntoDaFonte(int minutos, decimal? rating = 7.42m)
        {
            var minutagem = new Minutagem(minutos, Brasileirao2024);

            return new ConjuntoDeEstatisticas(
                Brasileirao2024,
                [
                    new EstatisticaAcumulavel(TipoDeAtributo.Gols, 12, minutagem),
                    new EstatisticaAcumulavel(TipoDeAtributo.Assistencias, 7, minutagem)
                ],
                [new ValorDerivado(TipoDeAtributo.Rating, rating, minutagem)]);
        }
    }
}

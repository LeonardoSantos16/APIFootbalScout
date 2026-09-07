using APIFootballScout.Application.Analise;
using APIFootballScout.Contracts.Acompanhamento;
using APIFootballScout.Contracts.Analise;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Tests.Contracts
{
    public class MetricasPor90DtoMapperTests
    {
        private static readonly Recorte Brasileirao2024 = new(325, 63814, ContextoDeRecorte.Clube);

        [Fact]
        public void A_metrica_calculada_expoe_valor_e_amostra_sem_motivo_de_recusa()
        {
            // Arrange
            var result = Resultado(new MetricaCalculada(0.45m, new Minutagem(2400, Brasileirao2024)));

            // Act
            var dto = result.ParaResponse();

            // Assert
            var metrica = Assert.Single(dto.Metricas);
            Assert.Equal(ResultadoDoCalculoDto.Calculada, metrica.Resultado);
            Assert.Equal(0.45m, metrica.Valor);
            Assert.Equal(2400, metrica.AmostraEmMinutos);
            Assert.Null(metrica.Motivo);
        }

        [Fact]
        public void O_calculo_recusado_expoe_o_motivo_e_nenhum_valor()
        {
            // Arrange
            var result = Resultado(new CalculoRecusado(MotivoDaRecusa.AmostraInsuficiente));

            // Act
            var dto = result.ParaResponse();

            // Assert
            var metrica = Assert.Single(dto.Metricas);
            Assert.Equal(ResultadoDoCalculoDto.Recusada, metrica.Resultado);
            Assert.Equal(MotivoDaRecusaDto.AmostraInsuficiente, metrica.Motivo);
            Assert.Null(metrica.Valor);
            Assert.Null(metrica.AmostraEmMinutos);
        }

        [Fact]
        public void O_recorte_da_consulta_atravessa_o_contrato()
        {
            // Arrange
            var result = Resultado(new CalculoRecusado(MotivoDaRecusa.AmostraInsuficiente));

            // Act
            var dto = result.ParaResponse();

            // Assert
            Assert.Equal(new RecorteDto(325, 63814, ContextoDeRecorteDto.Clube), dto.Recorte);
        }

        private static ConsultarMetricasPor90Result Resultado(MetricaPor90 metrica)
            => new(
                JogadorId: 13812,
                Recorte: Brasileirao2024,
                Metricas: [new MetricaDoJogador(TipoDeEstatistica.Gols, metrica)],
                Derivados: [new ValorDerivado(TipoDeValorDerivado.Rating, 7.42m)]);
    }
}

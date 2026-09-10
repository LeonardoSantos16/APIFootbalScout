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
        public void O_atributo_calculado_expoe_valor_e_amostra_sem_motivo_de_recusa()
        {
            // Arrange
            var result = Resultado(new AtributoCalculado(0.45m, new Minutagem(2400, Brasileirao2024)));

            // Act
            var dto = result.ParaResponse();

            // Assert
            var atributo = Assert.Single(dto.Atributos);
            Assert.Equal(ResultadoDoCalculoDto.Calculada, atributo.Resultado);
            Assert.Equal(0.45m, atributo.Valor);
            Assert.Equal(2400, atributo.AmostraEmMinutos);
            Assert.Null(atributo.Motivo);
        }

        [Fact]
        public void O_atributo_recusado_expoe_o_motivo_e_nenhum_valor()
        {
            // Arrange
            var result = Resultado(new AtributoRecusado(MotivoDaRecusa.AmostraInsuficiente));

            // Act
            var dto = result.ParaResponse();

            // Assert
            var atributo = Assert.Single(dto.Atributos);
            Assert.Equal(ResultadoDoCalculoDto.Recusada, atributo.Resultado);
            Assert.Equal(MotivoDaRecusaDto.AmostraInsuficiente, atributo.Motivo);
            Assert.Null(atributo.Valor);
            Assert.Null(atributo.AmostraEmMinutos);
        }

        [Fact]
        public void A_recusa_por_ausencia_na_fonte_atravessa_o_contrato()
        {
            // Arrange
            var result = Resultado(
                new AtributoRecusado(MotivoDaRecusa.FonteNaoAtribuiu),
                TipoDeAtributo.Rating);

            // Act
            var dto = result.ParaResponse();

            // Assert
            var atributo = Assert.Single(dto.Atributos);
            Assert.Equal(MotivoDaRecusaDto.FonteNaoAtribuiu, atributo.Motivo);
            Assert.Equal(TipoDeAtributoDto.Rating, atributo.Tipo);
        }

        [Fact]
        public void Acumulavel_e_derivado_saem_na_mesma_colecao_do_contrato()
        {
            // Arrange
            var result = new ConsultarMetricasPor90Result(
                JogadorId: 13812,
                Recorte: Brasileirao2024,
                Atributos:
                [
                    new AtributoDoJogador(
                        TipoDeAtributo.Gols,
                        new AtributoCalculado(0.45m, new Minutagem(2400, Brasileirao2024))),
                    new AtributoDoJogador(
                        TipoDeAtributo.Rating,
                        new AtributoCalculado(7.42m, new Minutagem(2400, Brasileirao2024)))
                ]);

            // Act
            var dto = result.ParaResponse();

            // Assert
            Assert.Equal(
                new[] { TipoDeAtributoDto.Gols, TipoDeAtributoDto.Rating },
                dto.Atributos.Select(atributo => atributo.Tipo));
        }

        [Fact]
        public void O_recorte_da_consulta_atravessa_o_contrato()
        {
            // Arrange
            var result = Resultado(new AtributoRecusado(MotivoDaRecusa.AmostraInsuficiente));

            // Act
            var dto = result.ParaResponse();

            // Assert
            Assert.Equal(new RecorteDto(325, 63814, ContextoDeRecorteDto.Clube), dto.Recorte);
        }

        private static ConsultarMetricasPor90Result Resultado(
            ResultadoDeAtributo resultado,
            TipoDeAtributo tipo = TipoDeAtributo.Gols)
            => new(
                JogadorId: 13812,
                Recorte: Brasileirao2024,
                Atributos: [new AtributoDoJogador(tipo, resultado)]);
    }
}

using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Tests.Analise
{
    public class MetricaCalculadaTests
    {
        private static readonly Recorte Brasileirao2024 = new(325, 63814, ContextoDeRecorte.Clube);

        [Fact]
        public void A_metrica_declara_o_tamanho_da_amostra_que_a_sustenta()
        {
            // Act
            var metrica = new MetricaCalculada(0.45m, new Minutagem(2400, Brasileirao2024));

            // Assert
            Assert.Equal(2400, metrica.Amostra.Minutos);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Amostra_que_nao_sustenta_nenhum_calculo_e_recusada(int minutos)
        {
            // Act
            var erro = Assert.Throws<ValorInvalidoException>(
                () => new MetricaCalculada(0.45m, new Minutagem(minutos, Brasileirao2024)));

            // Assert
            Assert.Equal("metrica_por_90.amostra_nao_sustenta", erro.Codigo);
        }
    }
}

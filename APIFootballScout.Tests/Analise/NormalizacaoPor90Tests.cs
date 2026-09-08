using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.Specifications;
using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Tests.Analise
{
    public class NormalizacaoPor90Tests
    {
        private static readonly Recorte Brasileirao2024 = new(325, 63814, ContextoDeRecorte.Clube);

        private static readonly AmostraSuficienteSpecification AmostraSuficiente =
            new(new AmostraMinima(450));

        [Fact]
        public void Amostra_insuficiente_recusa_o_calculo_com_motivo()
        {
            // Arrange
            var gols = new EstatisticaAcumulavel(TipoDeAtributo.Gols, 3, new Minutagem(200, Brasileirao2024));

            // Act
            var metrica = gols.PorNoventaMinutos(AmostraSuficiente);

            // Assert
            var recusa = Assert.IsType<AtributoRecusado>(metrica);
            Assert.Equal(MotivoDaRecusa.AmostraInsuficiente, recusa.Motivo);
        }

        [Fact]
        public void Estatistica_zerada_com_amostra_suficiente_e_valor_calculado_nao_recusa()
        {
            // Arrange
            var gols = new EstatisticaAcumulavel(TipoDeAtributo.Gols, 0, new Minutagem(2400, Brasileirao2024));

            // Act
            var metrica = gols.PorNoventaMinutos(AmostraSuficiente);

            // Assert
            var calculada = Assert.IsType<AtributoCalculado>(metrica);
            Assert.Equal(0m, calculada.Valor);
        }

        [Fact]
        public void Doze_gols_em_dois_mil_e_quatrocentos_minutos_dao_zero_virgula_quarenta_e_cinco_por_90()
        {
            // Arrange
            var gols = new EstatisticaAcumulavel(TipoDeAtributo.Gols, 12, new Minutagem(2400, Brasileirao2024));

            // Act
            var metrica = gols.PorNoventaMinutos(AmostraSuficiente);

            // Assert
            var calculada = Assert.IsType<AtributoCalculado>(metrica);
            Assert.Equal(0.45m, calculada.Valor);
        }

        [Fact]
        public void A_metrica_declara_a_amostra_de_onde_saiu_e_nao_o_minimo_da_politica()
        {
            // Arrange
            var gols = new EstatisticaAcumulavel(
                TipoDeAtributo.Gols, 12, new Minutagem(2400, Brasileirao2024));

            // Act
            var metrica = gols.PorNoventaMinutos(AmostraSuficiente);

            // Assert
            var calculada = Assert.IsType<AtributoCalculado>(metrica);
            Assert.Equal(new Minutagem(2400, Brasileirao2024), calculada.Amostra);
        }
    }
}

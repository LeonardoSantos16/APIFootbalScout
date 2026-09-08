using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Tests.Analise
{
    public class ValorDerivadoTests
    {
        private static readonly Recorte Brasileirao2024 = new(325, 63814, ContextoDeRecorte.Clube);

        [Fact]
        public void O_derivado_informado_declara_a_amostra_sobre_a_qual_a_fonte_o_atribuiu()
        {
            // Arrange
            var rating = new ValorDerivado(TipoDeAtributo.Rating, 7.42m, new Minutagem(2400, Brasileirao2024));

            // Act
            var resultado = rating.Resultado();

            // Assert
            var calculado = Assert.IsType<AtributoCalculado>(resultado);
            Assert.Equal(7.42m, calculado.Valor);
            Assert.Equal(new Minutagem(2400, Brasileirao2024), calculado.Amostra);
        }

        [Fact]
        public void O_derivado_que_a_fonte_nao_atribuiu_e_recusa_declarada_e_nao_zero()
        {
            // Arrange — a fonte aplica o proprio corte de amostra omitindo o campo.
            var rating = new ValorDerivado(TipoDeAtributo.Rating, null, new Minutagem(200, Brasileirao2024));

            // Act
            var resultado = rating.Resultado();

            // Assert
            var recusa = Assert.IsType<AtributoRecusado>(resultado);
            Assert.Equal(MotivoDaRecusa.FonteNaoAtribuiu, recusa.Motivo);
        }

        [Fact]
        public void A_recusa_da_fonte_e_distinta_da_recusa_por_amostra_insuficiente()
        {
            // Arrange
            var rating = new ValorDerivado(TipoDeAtributo.Rating, null, new Minutagem(2400, Brasileirao2024));

            // Act
            var recusa = Assert.IsType<AtributoRecusado>(rating.Resultado());

            // Assert
            Assert.NotEqual(MotivoDaRecusa.AmostraInsuficiente, recusa.Motivo);
        }
    }
}

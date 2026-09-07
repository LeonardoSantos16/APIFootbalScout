using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.Specifications;
using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Tests.Analise
{
    public class AmostraSuficienteSpecificationTests
    {
        private static readonly Recorte Brasileirao2024 = new(325, 63814, ContextoDeRecorte.Clube);

        [Theory]
        [InlineData(0)]
        [InlineData(89)]
        [InlineData(449)]
        public void Amostra_aquem_do_minimo_nao_sustenta_o_calculo(int minutos)
        {
            // Arrange
            var amostraSuficiente = new AmostraSuficienteSpecification(new AmostraMinima(450));

            // Act
            var sustenta = amostraSuficiente.IsSatisfiedBy(new Minutagem(minutos, Brasileirao2024));

            // Assert
            Assert.False(sustenta);
        }

        [Fact]
        public void Amostra_no_minimo_sustenta_o_calculo()
        {
            // Arrange
            var amostraSuficiente = new AmostraSuficienteSpecification(new AmostraMinima(450));

            // Act
            var sustenta = amostraSuficiente.IsSatisfiedBy(new Minutagem(450, Brasileirao2024));

            // Assert
            Assert.True(sustenta);
        }

        [Fact]
        public void Amostra_alem_do_minimo_sustenta_o_calculo()
        {
            // Arrange
            var amostraSuficiente = new AmostraSuficienteSpecification(new AmostraMinima(450));

            // Act
            var sustenta = amostraSuficiente.IsSatisfiedBy(new Minutagem(2400, Brasileirao2024));

            // Assert
            Assert.True(sustenta);
        }

        [Fact]
        public void O_minimo_e_politica_nao_numero_fixo_do_modelo()
        {
            // Arrange
            var minutagem = new Minutagem(300, Brasileirao2024);

            // Act & Assert
            Assert.True(new AmostraSuficienteSpecification(new AmostraMinima(180)).IsSatisfiedBy(minutagem));
            Assert.False(new AmostraSuficienteSpecification(new AmostraMinima(900)).IsSatisfiedBy(minutagem));
        }
    }
}

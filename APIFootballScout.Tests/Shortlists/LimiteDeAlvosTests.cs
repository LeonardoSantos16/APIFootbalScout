using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.ShortlistPersonalizada.ValueObject;

namespace APIFootballScout.Tests.Shortlists
{
    public class LimiteDeAlvosTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void Quantidade_aquem_do_limite_ainda_cabe(int quantidadeDeAlvos)
        {
            // Arrange
            var limite = new LimiteDeAlvos(3);

            // Act
            var cabe = limite.Comporta(quantidadeDeAlvos);

            // Assert
            Assert.True(cabe);
        }

        [Fact]
        public void Quantidade_no_limite_nao_cabe_mais()
        {
            // Arrange
            var limite = new LimiteDeAlvos(3);

            // Act
            var cabe = limite.Comporta(3);

            // Assert
            Assert.False(cabe);
        }

        [Fact]
        public void O_limite_e_politica_nao_numero_fixo_do_modelo()
        {
            // Act & Assert
            Assert.True(new LimiteDeAlvos(25).Comporta(5));
            Assert.False(new LimiteDeAlvos(5).Comporta(5));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Limite_nao_positivo_e_recusado(int valor)
        {
            // Act
            var erro = Assert.Throws<ValorInvalidoException>(() => new LimiteDeAlvos(valor));

            // Assert
            Assert.Equal("shortlist.limite_nao_positivo", erro.Codigo);
        }
    }
}

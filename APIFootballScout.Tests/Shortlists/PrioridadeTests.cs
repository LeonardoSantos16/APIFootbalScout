using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.ShortlistPersonalizada.ValueObject;

namespace APIFootballScout.Tests.Shortlists
{
    // R7.3 - a prioridade e uma ordem total, sem empates. A positividade e do tipo:
    // a unicidade dentro da lista e do agregado, que e quem enxerga o conjunto.
    public class PrioridadeTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(25)]
        public void A_prioridade_positiva_e_aceita(int valor)
        {
            // Act
            var prioridade = new Prioridade(valor);

            // Assert
            Assert.Equal(valor, prioridade.Valor);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-25)]
        public void A_prioridade_nao_positiva_e_recusada(int valor)
        {
            // A ordem comeca em 1: zero nao e "sem prioridade", e uma posicao que a
            // lista nao tem.

            // Act
            var erro = Assert.Throws<ValorInvalidoException>(() => new Prioridade(valor));

            // Assert
            Assert.Equal("prioridade.nao_positiva", erro.Codigo);
        }

        [Fact]
        public void Duas_prioridades_de_mesmo_valor_sao_iguais()
        {
            // Value object: e o valor que diz se duas posicoes sao a mesma. E disso que
            // o agregado depende para detectar empate.

            // Act & Assert
            Assert.Equal(new Prioridade(2), new Prioridade(2));
            Assert.NotEqual(new Prioridade(2), new Prioridade(3));
        }
    }
}

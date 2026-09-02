using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;
using APIFootballScout.Domain.ShortlistPersonalizada.Specifications;
using APIFootballScout.Domain.ShortlistPersonalizada.ValueObject;

namespace APIFootballScout.Tests.Shortlists
{
    // R7.1 - a lista tem numero maximo de alvos. A existencia do limite e invariante;
    // seu valor e politica, e e aqui que ela mora: a specification carrega o limite,
    // o agregado so a consulta. Trocar o teto nao toca o modelo.
    public class ShortlistComVagaSpecificationTests
    {
        private static Shortlist ShortlistCom(int quantidadeDeAlvos)
            => Shortlist.Restaurar(
                id: Guid.NewGuid(),
                olheiroId: Guid.NewGuid(),
                nome: "Laterais esquerdos 2026",
                alvos: Enumerable.Range(1, quantidadeDeAlvos).Select(posicao =>
                    new Alvo(
                        JogadorId: 1000 + posicao,
                        Prioridade: new Prioridade(posicao),
                        CustoEstimado: new Dinheiro(5_000_000_00, "EUR"))));

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void Lista_aquem_do_limite_tem_vaga(int quantidadeDeAlvos)
        {
            // Arrange
            var comVaga = new ShortlistComVagaSpecification(limiteDeAlvos: 3);

            // Act
            var temVaga = comVaga.IsSatisfiedBy(ShortlistCom(quantidadeDeAlvos));

            // Assert
            Assert.True(temVaga);
        }

        [Fact]
        public void Lista_no_limite_nao_tem_vaga()
        {
            // O limite e o numero maximo de alvos, nao o numero a partir do qual se
            // recusa: uma lista com exatamente o teto ja esta cheia.

            // Arrange
            var comVaga = new ShortlistComVagaSpecification(limiteDeAlvos: 3);

            // Act
            var temVaga = comVaga.IsSatisfiedBy(ShortlistCom(3));

            // Assert
            Assert.False(temVaga);
        }

        [Fact]
        public void O_limite_e_politica_nao_numero_fixo_do_modelo()
        {
            // A mesma lista responde diferente sob politicas diferentes. Se este teste
            // passasse com o limite embutido no dominio, R7.1 estaria mal modelada.

            // Arrange
            var shortlist = ShortlistCom(5);

            // Act & Assert
            Assert.True(new ShortlistComVagaSpecification(limiteDeAlvos: 25).IsSatisfiedBy(shortlist));
            Assert.False(new ShortlistComVagaSpecification(limiteDeAlvos: 5).IsSatisfiedBy(shortlist));
        }
    }
}

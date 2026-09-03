using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Tests.SharedKernel
{
    public class DinheiroTests
    {
        private static Dinheiro Euros(long milhoes) => new(milhoes * 1_000_000_00, "EUR");

        private static Dinheiro Libras(long milhoes) => new(milhoes * 1_000_000_00, "GBP");

        [Fact]
        public void Variacao_entre_quantias_da_mesma_moeda_e_calculada()
        {
            var variacao = Euros(60).VariacaoPercentualAbsolutaEmRelacaoA(Euros(50));

            Assert.Equal(20m, variacao);
        }

        [Fact]
        public void Variacao_e_absoluta_quando_a_quantia_cai()
        {
            var variacao = Euros(40).VariacaoPercentualAbsolutaEmRelacaoA(Euros(50));

            Assert.Equal(20m, variacao);
        }

        [Fact]
        public void Quantias_em_moedas_distintas_nao_se_comparam()
        {
            var erro = Assert.Throws<ValorInvalidoException>(
                () => Libras(60).VariacaoPercentualAbsolutaEmRelacaoA(Euros(50)));

            Assert.Equal("dinheiro.moedas_distintas", erro.Codigo);
        }

        [Fact]
        public void A_recusa_independe_do_lado_em_que_a_moeda_estranha_esta()
        {
            Assert.Throws<ValorInvalidoException>(
                () => Euros(60).VariacaoPercentualAbsolutaEmRelacaoA(Libras(50)));
        }
        // R7.6 - a soma do custo da shortlist exige moeda unica. A regra e do value
        // object, nao da lista: e aqui que a recusa de operar entre moedas vive (R2.4).

        [Fact]
        public void Quantias_da_mesma_moeda_se_somam()
        {
            // Act
            var total = Euros(50).Somar(Euros(30));

            // Assert
            Assert.Equal(Euros(80), total);
        }

        [Fact]
        public void A_soma_preserva_a_moeda()
        {
            // Act
            var total = Libras(2).Somar(Libras(3));

            // Assert
            Assert.Equal("GBP", total.Moeda);
        }

        [Fact]
        public void Quantias_em_moedas_distintas_nao_se_somam()
        {
            // Mesma invariante da comparacao: nao ha conversao no escopo, entao somar
            // euro com libra nao tem resultado - nem zero, nem um dos dois lados.

            // Act
            var erro = Assert.Throws<ValorInvalidoException>(() => Euros(50).Somar(Libras(30)));

            // Assert
            Assert.Equal("dinheiro.moedas_distintas", erro.Codigo);
        }

        [Fact]
        public void A_recusa_da_soma_independe_do_lado_da_moeda_estranha()
        {
            // Act & Assert
            Assert.Throws<ValorInvalidoException>(() => Libras(30).Somar(Euros(50)));
        }

        [Fact]
        public void Somar_nao_altera_as_parcelas()
        {
            // Value object e imutavel: a soma produz uma quantia nova.

            // Arrange
            var parcela = Euros(50);

            // Act
            parcela.Somar(Euros(30));

            // Assert
            Assert.Equal(Euros(50), parcela);
        }
    }
}

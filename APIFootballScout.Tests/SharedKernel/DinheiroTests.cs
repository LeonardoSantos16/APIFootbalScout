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
    }
}

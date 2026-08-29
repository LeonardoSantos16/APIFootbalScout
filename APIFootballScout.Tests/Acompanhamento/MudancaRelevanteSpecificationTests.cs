using APIFootballScout.Domain.Acompanhamento.Specifications;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Tests.Acompanhamento
{
    public class MudancaRelevanteSpecificationTests
    {
        private static readonly Recorte Recorte = new(325, 63814, ContextoDeRecorte.Clube);

        private static MudancaRelevanteSpecification Especificacao() =>
            new(valorDeMercado: new LimiarPercentual(10), minutagem: new LimiarAbsoluto(180));

        private static Dinheiro Euros(long milhoes) => new(milhoes * 1_000_000_00, "EUR");

        private static Minutagem Minutos(int minutos) => new(minutos, Recorte);

        [Fact]
        public void Valor_de_mercado_que_varia_acima_do_limiar_e_relevante()
        {
            var relevante = Especificacao().IsSatisfiedBy(
                new MudancaDeValorDeMercado(Anterior: Euros(50), Atual: Euros(60)));

            Assert.True(relevante);
        }

        [Fact]
        public void Valor_de_mercado_que_cai_acima_do_limiar_e_relevante()
        {
            var relevante = Especificacao().IsSatisfiedBy(
                new MudancaDeValorDeMercado(Anterior: Euros(50), Atual: Euros(40)));

            Assert.True(relevante);
        }

        [Theory]
        [InlineData(50, 55)] 
        [InlineData(50, 45)] 
        [InlineData(50, 52)] 
        [InlineData(50, 50)]
        public void Valor_de_mercado_que_nao_ultrapassa_o_limiar_nao_e_relevante(long anterior, long atual)
        {
            var relevante = Especificacao().IsSatisfiedBy(
                new MudancaDeValorDeMercado(Euros(anterior), Euros(atual)));

            Assert.False(relevante);
        }

        [Fact]
        public void Minutagem_que_varia_acima_do_limiar_e_relevante()
        {
            var relevante = Especificacao().IsSatisfiedBy(
                new MudancaDeMinutagem(Anterior: Minutos(400), Atual: Minutos(700)));

            Assert.True(relevante);
        }

        [Fact]
        public void Minutagem_que_cai_acima_do_limiar_e_relevante()
        {
            var relevante = Especificacao().IsSatisfiedBy(
                new MudancaDeMinutagem(Anterior: Minutos(700), Atual: Minutos(400)));

            Assert.True(relevante);
        }

        [Theory]
        [InlineData(400, 580)] 
        [InlineData(400, 220)] 
        [InlineData(400, 450)] 
        [InlineData(400, 400)] 
        public void Minutagem_que_nao_ultrapassa_o_limiar_nao_e_relevante(int anterior, int atual)
        {
            var relevante = Especificacao().IsSatisfiedBy(
                new MudancaDeMinutagem(Minutos(anterior), Minutos(atual)));

            Assert.False(relevante);
        }

        [Fact]
        public void Mudanca_de_clube_e_relevante()
        {
            var relevante = Especificacao().IsSatisfiedBy(
                new MudancaDeClube(Anterior: "Santos", Atual: "Al-Hilal"));

            Assert.True(relevante);
        }

        [Fact]
        public void Clube_igual_dos_dois_lados_nao_e_relevante()
        {
            var relevante = Especificacao().IsSatisfiedBy(
                new MudancaDeClube(Anterior: "Santos", Atual: "Santos"));

            Assert.False(relevante);
        }
    }
}

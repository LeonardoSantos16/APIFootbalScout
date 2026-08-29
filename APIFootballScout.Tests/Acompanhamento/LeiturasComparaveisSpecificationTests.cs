using APIFootballScout.Domain.Acompanhamento.Specifications;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Tests.Acompanhamento
{
    public class LeiturasComparaveisSpecificationTests
    {
        private static readonly Recorte Temporada2024 = new(325, 63814, ContextoDeRecorte.Clube);
        private static readonly Recorte Temporada2025 = new(325, 77012, ContextoDeRecorte.Clube);

        [Fact]
        public void Minutagem_atraves_da_virada_de_temporada_nao_e_comparavel()
        {
            var comparaveis = new LeiturasComparaveisSpecification().IsSatisfiedBy(
                new MudancaDeMinutagem(
                    Anterior: new Minutagem(2400, Temporada2024),
                    Atual: new Minutagem(300, Temporada2025)));

            Assert.False(comparaveis);
        }

        [Fact]
        public void Minutagem_do_mesmo_recorte_e_comparavel()
        {
            var comparaveis = new LeiturasComparaveisSpecification().IsSatisfiedBy(
                new MudancaDeMinutagem(
                    Anterior: new Minutagem(400, Temporada2024),
                    Atual: new Minutagem(700, Temporada2024)));

            Assert.True(comparaveis);
        }

        [Fact]
        public void Valor_de_mercado_atravessa_a_virada_de_temporada()
        {
            var comparaveis = new LeiturasComparaveisSpecification().IsSatisfiedBy(
                new MudancaDeValorDeMercado(
                    Anterior: new Dinheiro(50_000_000_00, "EUR"),
                    Atual: new Dinheiro(60_000_000_00, "EUR")));

            Assert.True(comparaveis);
        }

        [Fact]
        public void Mudanca_de_clube_atravessa_a_virada_de_temporada()
        {
            var comparaveis = new LeiturasComparaveisSpecification().IsSatisfiedBy(
                new MudancaDeClube(Anterior: "Santos", Atual: "Al-Hilal"));

            Assert.True(comparaveis);
        }

        [Theory]
        [InlineData(8, 63814, ContextoDeRecorte.Clube)]
        [InlineData(325, 63814, ContextoDeRecorte.Selecao)]
        public void Minutagem_de_outro_recorte_nao_e_comparavel(
            int competicaoId, int temporadaId, ContextoDeRecorte contexto)
        {
            var comparaveis = new LeiturasComparaveisSpecification().IsSatisfiedBy(
                new MudancaDeMinutagem(
                    Anterior: new Minutagem(400, Temporada2024),
                    Atual: new Minutagem(700, new Recorte(competicaoId, temporadaId, contexto))));

            Assert.False(comparaveis);
        }
    }
}

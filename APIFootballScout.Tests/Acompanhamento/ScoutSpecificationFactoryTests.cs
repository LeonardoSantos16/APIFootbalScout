using APIFootballScout.Application;
using APIFootballScout.Application.Configuration;
using APIFootballScout.Domain.Acompanhamento.Specifications;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Domain.SharedKernel;
using Microsoft.Extensions.Options;

namespace APIFootballScout.Tests.Acompanhamento
{
    public class ScoutSpecificationFactoryTests
    {
        private static readonly Recorte Recorte = new(325, 63814, ContextoDeRecorte.Clube);

        private static MudancaRelevanteSpecification MudancaRelevante(
            int limiarValorDeMercadoPercentual,
            int limiarMinutagemMinutos)
        {
            var config = new ScoutConfig
            {
                LimiarValorDeMercadoPercentual = limiarValorDeMercadoPercentual,
                LimiarMinutagemMinutos = limiarMinutagemMinutos
            };

            return new ScoutSpecificationFactory(Options.Create(config)).MudancaRelevante();
        }

        private static Dinheiro Euros(long milhoes) => new(milhoes * 1_000_000_00, "EUR");

        private static Minutagem Minutos(int minutos) => new(minutos, Recorte);

        [Fact]
        public void Cada_tipo_de_mudanca_e_julgado_pelo_seu_proprio_limiar()
        {
            var especificacao = MudancaRelevante(
                limiarValorDeMercadoPercentual: 10,
                limiarMinutagemMinutos: 180);

            Assert.True(especificacao.IsSatisfiedBy(
                new MudancaDeValorDeMercado(Anterior: Euros(50), Atual: Euros(60))));

            Assert.False(especificacao.IsSatisfiedBy(
                new MudancaDeMinutagem(Anterior: Minutos(400), Atual: Minutos(420))));
        }
    }
}

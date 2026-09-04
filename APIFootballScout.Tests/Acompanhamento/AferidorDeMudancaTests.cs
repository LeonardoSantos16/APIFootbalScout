using APIFootballScout.Domain.Acompanhamento.Services;
using APIFootballScout.Domain.Acompanhamento.Specifications;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Tests.Acompanhamento
{
    public class AferidorDeMudancaTests
    {
        private static readonly Recorte Temporada2024 = new(325, 63814, ContextoDeRecorte.Clube);
        private static readonly Recorte Temporada2025 = new(325, 77012, ContextoDeRecorte.Clube);

        private static AferidorDeMudanca Aferidor() =>
            new(new MudancaRelevanteSpecification(
                    valorDeMercado: new LimiarPercentual(10),
                    minutagem: new LimiarAbsoluto(180)),
                new LeiturasComparaveisSpecification());

        private static Dinheiro Euros(long milhoes) => new(milhoes * 1_000_000_00, "EUR");

        [Fact]
        public void Minutagem_atraves_da_virada_sai_indisponivel_por_temporada_virada()
        {
            var afericao = Aferidor().Aferir(
                new MudancaDeMinutagem(
                    Anterior: new Minutagem(2400, Temporada2024),
                    Atual: new Minutagem(300, Temporada2025)));

            Assert.Equal(new Indisponivel(MotivoDeIndisponibilidade.TemporadaVirada), afericao);
        }

        [Fact]
        public void Leitura_incomparavel_sai_indisponivel_mesmo_quando_a_variacao_e_pequena()
        {
            var afericao = Aferidor().Aferir(
                new MudancaDeMinutagem(
                    Anterior: new Minutagem(400, Temporada2024),
                    Atual: new Minutagem(420, Temporada2025)));

            Assert.IsType<Indisponivel>(afericao);
        }

        [Fact]
        public void Valor_de_mercado_atual_ausente_sai_indisponivel_por_moeda_inesperada()
        {
            var afericao = Aferidor().AferirValorDeMercado(anterior: Euros(50), atual: null);

            Assert.Equal(new Indisponivel(MotivoDeIndisponibilidade.MoedaInesperada), afericao);
        }

        [Fact]
        public void Variacao_aquem_do_limiar_sai_sem_mudanca_relevante()
        {
            var afericao = Aferidor().Aferir(
                new MudancaDeValorDeMercado(Anterior: Euros(50), Atual: Euros(52)));

            Assert.IsType<SemMudancaRelevante>(afericao);
        }

        [Fact]
        public void Clube_inalterado_sai_sem_mudanca_relevante()
        {
            var afericao = Aferidor().Aferir(new MudancaDeClube(Anterior: "Santos", Atual: "Santos"));

            Assert.IsType<SemMudancaRelevante>(afericao);
        }

        [Fact]
        public void Mudanca_relevante_sai_como_a_propria_mudanca()
        {
            var mudanca = new MudancaDeValorDeMercado(Anterior: Euros(50), Atual: Euros(60));

            var afericao = Aferidor().Aferir(mudanca);

            Assert.Same(mudanca, afericao);
        }

        [Fact]
        public void Valor_de_mercado_presente_e_aferido_normalmente()
        {
            var afericao = Aferidor().AferirValorDeMercado(anterior: Euros(50), atual: Euros(60));

            Assert.Equal(
                new MudancaDeValorDeMercado(Anterior: Euros(50), Atual: Euros(60)),
                afericao);
        }
    }
}

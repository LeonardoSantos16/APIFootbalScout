using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Infrastructure.SofascoreExternalAdapter.Acl;
using APIFootballScout.Infrastructure.SofascoreExternalAdapter.player;

namespace APIFootballScout.Tests.Analise
{
    public class TraducaoDeEstatisticasTests
    {
        private static readonly Recorte Brasileirao2024 = new(325, 63814, ContextoDeRecorte.Clube);

        [Fact]
        public void Contagens_da_fonte_viram_estatisticas_acumulaveis()
        {
            // Arrange
            var retornoDaFonte = TemporadaNaFonte(gols: 12, assistencias: 7, passesDecisivos: 41,
                desarmes: 18, interceptacoes: 23, minutosJogados: 2400);

            // Act
            var conjunto = SofascoreTradutor.TraduzirParaConjuntoDeEstatisticas(retornoDaFonte, Brasileirao2024);

            // Assert
            var minutagem = new Minutagem(2400, Brasileirao2024);
            Assert.Equal(
                new[]
                {
                    new EstatisticaAcumulavel(TipoDeAtributo.Gols, 12, minutagem),
                    new EstatisticaAcumulavel(TipoDeAtributo.Assistencias, 7, minutagem),
                    new EstatisticaAcumulavel(TipoDeAtributo.PassesDecisivos, 41, minutagem),
                    new EstatisticaAcumulavel(TipoDeAtributo.Desarmes, 18, minutagem),
                    new EstatisticaAcumulavel(TipoDeAtributo.Interceptacoes, 23, minutagem)
                },
                conjunto.Acumulaveis);
        }

        [Fact]
        public void Rating_e_precisao_de_passe_viram_valor_derivado_com_a_minutagem_do_conjunto()
        {
            // Arrange
            var retornoDaFonte = TemporadaNaFonte(rating: 7.42, precisaoDePasse: 84.3, minutosJogados: 2400);

            // Act
            var conjunto = SofascoreTradutor.TraduzirParaConjuntoDeEstatisticas(retornoDaFonte, Brasileirao2024);

            // Assert
            var minutagem = new Minutagem(2400, Brasileirao2024);
            Assert.Equal(
                new[]
                {
                    new ValorDerivado(TipoDeAtributo.Rating, 7.42m, minutagem),
                    new ValorDerivado(TipoDeAtributo.PrecisaoDePasse, 84.3m, minutagem)
                },
                conjunto.Derivados);
        }

        [Fact]
        public void Derivado_omitido_pela_fonte_nao_vira_zero_na_traducao()
        {
            // Arrange — a fonte omite o campo quando nao atribui o valor, e omissao
            // nao e o mesmo que valor zero.
            var retornoDaFonte = TemporadaNaFonte(rating: null, precisaoDePasse: null, minutosJogados: 200);

            // Act
            var conjunto = SofascoreTradutor.TraduzirParaConjuntoDeEstatisticas(retornoDaFonte, Brasileirao2024);

            // Assert
            Assert.All(conjunto.Derivados, derivado => Assert.Null(derivado.Valor));
        }

        [Fact]
        public void Derivado_zerado_pela_fonte_e_distinto_de_derivado_omitido()
        {
            // Arrange
            var retornoDaFonte = TemporadaNaFonte(rating: 0, precisaoDePasse: 0, minutosJogados: 200);

            // Act
            var conjunto = SofascoreTradutor.TraduzirParaConjuntoDeEstatisticas(retornoDaFonte, Brasileirao2024);

            // Assert
            Assert.All(conjunto.Derivados, derivado => Assert.Equal(0m, derivado.Valor));
        }

        [Fact]
        public void Nenhum_valor_derivado_entra_no_conjunto_dos_acumulaveis()
        {
            // Arrange — rating e precisão de passe altos, gols zerados: se um deles
            // vazasse para os acumuláveis, a contagem denunciaria.
            var retornoDaFonte = TemporadaNaFonte(gols: 0, rating: 9.9, precisaoDePasse: 99.9);

            // Act
            var conjunto = SofascoreTradutor.TraduzirParaConjuntoDeEstatisticas(retornoDaFonte, Brasileirao2024);

            // Assert
            Assert.All(conjunto.Acumulaveis, estatistica => Assert.Equal(0, estatistica.Contagem));
        }

        private static SofaSeasonStatsResponse TemporadaNaFonte(
            int gols = 0,
            int assistencias = 0,
            int passesDecisivos = 0,
            int desarmes = 0,
            int interceptacoes = 0,
            int minutosJogados = 0,
            double? rating = 0,
            double? precisaoDePasse = 0)
            => new(
                new SofaStatistics(
                    Rating: rating,
                    Appearances: 30,
                    MinutesPlayed: minutosJogados,
                    Goals: gols,
                    Assists: assistencias,
                    BigChancesCreated: 9,
                    KeyPasses: passesDecisivos,
                    TotalPasses: 900,
                    AccuratePassesPercentage: precisaoDePasse,
                    SuccessfulDribbles: 33,
                    Tackles: desarmes,
                    Interceptions: interceptacoes,
                    TotalDuelsWon: 120,
                    GroundDuelsWon: 80,
                    AerialDuelsWon: 40,
                    YellowCards: 4,
                    RedCards: 0,
                    BallRecovery: 55,
                    Id: 1),
                new SofaTeamSummary(1, "Santos", "SAN"));
    }
}

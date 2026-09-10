using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Infrastructure.SofascoreExternalAdapter.Acl;
using APIFootballScout.Infrastructure.SofascoreExternalAdapter.player;

namespace APIFootballScout.Tests.Analise
{
    public class TraducaoDePosicaoTests
    {
        private static readonly Recorte Brasileirao2024 = new(325, 63814, ContextoDeRecorte.Clube);

        [Theory]
        [InlineData("G", Posicao.Goleiro)]
        [InlineData("D", Posicao.Defesa)]
        [InlineData("M", Posicao.MeioCampo)]
        [InlineData("F", Posicao.Ataque)]
        public void A_macroposicao_da_fonte_vira_posicao_do_dominio(string daFonte, Posicao esperada)
        {
            // Act
            var perfil = SofascoreTradutor.TraduzirParaPerfilDoJogador(
                JogadorNaFonte(daFonte), TemporadaNaFonte(), Brasileirao2024, LidoEm);

            // Assert
            Assert.Equal(esperada, perfil.Posicao);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("AM")]
        [InlineData("qualquer coisa nova")]
        public void Posicao_que_a_fonte_nao_declara_ou_o_dominio_nao_reconhece_fica_desconhecida(string? daFonte)
        {
            // Arrange — uma macroposicao nova na fonte nao pode derrubar a leitura do

            // Act
            var perfil = SofascoreTradutor.TraduzirParaPerfilDoJogador(
                JogadorNaFonte(daFonte!), TemporadaNaFonte(), Brasileirao2024, LidoEm);

            // Assert
            Assert.Null(perfil.Posicao);
        }

        private static readonly DateTime LidoEm = new(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc);

        private static SofaPlayerDetailsResponse JogadorNaFonte(string posicao)
            => new(
                new SofaPlayerDetail(
                    Id: 13812,
                    Name: "Neymar",
                    Slug: "neymar",
                    Team: new SofaTeamDetail(1, "Santos", "SAN", new SofaTournament(325, "Brasileirao"), false, 10),
                    Position: posicao,
                    PositionsDetailed: [],
                    JerseyNumber: "10",
                    Height: 175,
                    DateOfBirth: new DateTime(1992, 2, 5, 0, 0, 0, DateTimeKind.Utc),
                    PreferredFoot: "Right",
                    UserCount: 100,
                    ProposedMarketValue: 50_000_000_00,
                    ProposedMarketValueRaw: null));

        private static SofaSeasonStatsResponse TemporadaNaFonte()
            => new(
                new SofaStatistics(
                    Rating: 7.42,
                    Appearances: 30,
                    MinutesPlayed: 2400,
                    Goals: 12,
                    Assists: 7,
                    BigChancesCreated: 9,
                    KeyPasses: 41,
                    TotalPasses: 900,
                    AccuratePassesPercentage: 84.3,
                    SuccessfulDribbles: 33,
                    Tackles: 18,
                    Interceptions: 23,
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

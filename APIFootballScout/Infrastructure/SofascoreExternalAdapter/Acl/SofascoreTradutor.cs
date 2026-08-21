using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Infrastructure.SofascoreExternalAdapter.player;

namespace APIFootballScout.Infrastructure.SofascoreExternalAdapter.Acl
{
    internal static class SofascoreTradutor
    {
        private const string MoedaEsperada = "EUR";

        /// <summary>
        /// R9.6 — a fonte não identifica o recorte no retorno. O recorte pedido
        /// é carimbado aqui, de volta no dado traduzido.
        /// </summary>
        public static PerfilDoJogador TraduzirParaPerfilDoJogador(
            SofaPlayerDetailsResponse sofaPlayer,
            SofaSeasonStatsResponse statsPlayer,
            Recorte recorte,
            DateTime lidoEm)
        {
            return new PerfilDoJogador(
                JogadorId: sofaPlayer.Player.Id,
                Nome: sofaPlayer.Player.Name,
                Posicao: sofaPlayer.Player.Position,
                Clube: sofaPlayer.Player.Team?.Name,
                ValorDeMercado: new Dinheiro(sofaPlayer.Player.ProposedMarketValue, MoedaEsperada),
                MinutosJogados: statsPlayer.Statistics.MinutesPlayed,
                Recorte: recorte,
                LidoEm: lidoEm
            );
        }
    }
}

using APIFootballScout.Domain.CatalagoDeJogador;
using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Infrastructure.SofascoreExternalAdapter.player;
using Refit;

namespace APIFootballScout.Infrastructure.SofascoreExternalAdapter.Acl
{
    public static class SofascoreTradutor
    {
        public static PerfilDoJogador TraduzirParaPerfilDoJogador(SofaPlayerDetailsResponse sofaPlayer, int competicaoId, int temporadaId, SofaSeasonStatsResponse statsPlayer)
        {
            var jogador = new PerfilDoJogador(
                JogadorId: sofaPlayer.Player.Id,
                Nome: sofaPlayer.Player.Name,
                Posicao: sofaPlayer.Player.Position,
                Clube: sofaPlayer.Player.Team.Name,
                ReceitaEstimada: new Dinheiro(sofaPlayer.Player.ProposedMarketValue, "EUR"),
                MinutosJogados: statsPlayer.Statistics.MinutesPlayed,
                Recorte: new Recorte(competicaoId, temporadaId, (ContextoDeRecorte)1)
            );
            return jogador;
        }
    }
}

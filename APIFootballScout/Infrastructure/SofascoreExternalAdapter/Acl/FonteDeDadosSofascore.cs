using APIFootballScout.Domain.CatalagoDeJogador;
using APIFootballScout.Infrastructure.External;
using APIFootballScout.Infrastructure.SofascoreExternalAdapter.player;

namespace APIFootballScout.Infrastructure.SofascoreExternalAdapter.Acl
{
    public class FonteDeDadosSofascore : ICatalogoDeJogador
    {
        private readonly ISofascoreClient _sofascoreApiClient;
        public FonteDeDadosSofascore(ISofascoreClient sofascoreApiClient)
        {
            _sofascoreApiClient = sofascoreApiClient;
        }

        public async Task<PerfilDoJogador> ObterPerfilDoJogador(int jogadorId, int competicaoId, int temporadaId, CancellationToken cancellationToken = default)
        {
            var playerProfile = await _sofascoreApiClient.GetSofascorePlayerDetailsAsync(jogadorId, cancellationToken);
            var statsPlayer = await _sofascoreApiClient.GetSofascorePlayerStatisticsSeasonAsync(jogadorId, temporadaId.ToString(), competicaoId.ToString(), cancellationToken);
            if (playerProfile?.Content == null || statsPlayer?.Content == null)
            {
                throw new Exception("Erro ao obter perfil do jogador ou estatísticas do jogador.");
            };

            return SofascoreTradutor.TraduzirParaPerfilDoJogador(playerProfile.Content, competicaoId, temporadaId, statsPlayer.Content);    
        }
    }
}

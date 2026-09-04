using System.Net;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Infrastructure.External;

namespace APIFootballScout.Infrastructure.SofascoreExternalAdapter.Acl
{
    public class FonteDeDadosSofascore : ICatalogoDeJogador
    {
        private readonly ISofascoreClient _sofascoreApiClient;

        public FonteDeDadosSofascore(ISofascoreClient sofascoreApiClient)
        {
            _sofascoreApiClient = sofascoreApiClient;
        }

        public async Task<PerfilDoJogador?> ObterPerfilDoJogador(int jogadorId, Recorte recorte, CancellationToken cancellationToken = default)
        {
            var lidoEm = DateTime.UtcNow;

            var playerProfile = await _sofascoreApiClient.GetSofascorePlayerDetailsAsync(jogadorId, cancellationToken);
            var statsPlayer = await _sofascoreApiClient.GetSofascorePlayerStatisticsSeasonAsync(
                jogadorId,
                recorte.CompeticaoId.ToString(),
                recorte.TemporadaId.ToString(),
                cancellationToken);

            if (playerProfile?.StatusCode == HttpStatusCode.NotFound
                || statsPlayer?.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (playerProfile?.Content == null || statsPlayer?.Content == null)
            {
                throw new FonteExternaIndisponivelException(
                    "sofascore.perfil_do_jogador_indisponivel",
                    "Could not retrieve the player profile or season statistics from Sofascore.");
            }

            return SofascoreTradutor.TraduzirParaPerfilDoJogador(
                playerProfile.Content, statsPlayer.Content, recorte, lidoEm);
        }
    }
}

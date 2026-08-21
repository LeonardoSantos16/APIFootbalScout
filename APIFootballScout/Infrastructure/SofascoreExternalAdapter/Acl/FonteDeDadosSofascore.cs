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

        public async Task<PerfilDoJogador> ObterPerfilDoJogador(int jogadorId, Recorte recorte, CancellationToken cancellationToken = default)
        {
            var lidoEm = DateTime.UtcNow;

            var playerProfile = await _sofascoreApiClient.GetSofascorePlayerDetailsAsync(jogadorId, cancellationToken);
            var statsPlayer = await _sofascoreApiClient.GetSofascorePlayerStatisticsSeasonAsync(
                jogadorId,
                recorte.CompeticaoId.ToString(),
                recorte.TemporadaId.ToString(),
                cancellationToken);

            if (playerProfile?.Content == null || statsPlayer?.Content == null)
            {
                throw new Exception("Erro ao obter perfil do jogador ou estatísticas do jogador.");
            }

            return SofascoreTradutor.TraduzirParaPerfilDoJogador(
                playerProfile.Content, statsPlayer.Content, recorte, lidoEm);
        }
    }
}

using APIFootballScout.Infrastructure.SofascoreExternalAdapter.player;

namespace APIFootballScout.Infrastructure.SofascoreExternalAdapter
{
    public interface ISofascorePlayerReader
    {
        Task<SofaSearchResponse> SearchPlayersAsync(string name);
        Task<PlayerFullProfileDto> GetPlayerProfileAsync(int playerId, CancellationToken cancellationToken = default);
        Task<SofaPlayerDetailsResponse> GetPlayerDetailsAsync(int playerId, CancellationToken cancellationToken = default);
        string GetPlayerImageAsync(int playerId);
        Task<SofaSeasonStatsResponse> GetPlayerStatisticsSeasonAsync(int playerId, string tournamentId, string seasonId, CancellationToken cancellationToken, string? type = "overall");
        Task<SofaTransferHistoryResponse> GetPlayerTransferHistoryAsync(int playerId);
        Task<SofaPlayerStatisticsSeasonsResponse> GetPlayerHistoryStatsAsync(int playerId);
    }
}

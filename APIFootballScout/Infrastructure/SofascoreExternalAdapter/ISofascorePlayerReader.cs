using APIFootballScout.Infrastructure.SofascoreExternalAdapter.player;

namespace APIFootballScout.Infrastructure.SofascoreExternalAdapter
{
    public interface ISofascorePlayerReader
    {
        Task<SofaSearchResponse> SearchPlayersAsync(string name);
        Task<PlayerFullProfileDto> GetPlayerProfileAsync(int playerId);
        Task<SofaPlayerDetailsResponse> GetPlayerDetailsAsync(int playerId);
        string GetPlayerImageAsync(int playerId);
        Task<SofaSeasonStatsResponse> GetPlayerStatisticsSeasonAsync(int playerId, string tournamentId, string seasonId, string? type = "overall");
        Task<SofaTransferHistoryResponse> GetPlayerTransferHistoryAsync(int playerId);
        Task<SofaPlayerStatisticsSeasonsResponse> GetPlayerHistoryStatsAsync(int playerId);
    }
}

using APIFootballScout.Models.DTOs.Sofascore.player;
using APIFootballScout.Models.DTOs.Sofascore.Tournament;
using Refit;

namespace APIFootballScout.Services.External
{
   public interface ISofascoreClient
    {
        [Get("/search")]
        Task<IApiResponse<SofaSearchResponse>> SearchPlayerAsync([Query] string q, [Query] string? type = "player-team-persons", [Query] int? page = 0);

        [Get("/players/detail")]
        Task<IApiResponse<SofaPlayerDetailsResponse>> GetPlayerDetailsAsync([Query] int playerId);

        [Get("/players/get-image")]
        Task<IApiResponse<HttpContent>> GetPlayerImageAsync([Query] string id);

        [Get("/players/get-statistics-seasons")]
        Task<IApiResponse<SofaSeasonStatsResponse>> GetPlayerStatisticsSeasonAsync([Query] int playerId, [Query] string tournamentId, [Query] string seasonId, [Query] string? type = "overall");

        [Get("/players/get-national-team-statistics")]
        Task<IApiResponse<SofaNationalTeamStatsResponse>> GetPlayerNationalTeamAsync([Query] string id);

        [Get("/players/get-transfer-history")]
        Task<IApiResponse<SofaTransferHistoryResponse>> GetPlayerTransferHistoryAsync([Query] int playerId);

        [Get("/tournaments/get-standings")]
        Task<IApiResponse<SofaStandingsResponse>> GetTournamentStandingsAsync([Query] string tournamentId, [Query] string seasonId, [Query] string? type = "total");

        [Get("/tournaments/detail")]
        Task<IApiResponse<SofaTournamentDetailResponse>> GetTournamentDetailsAsync([Query] string tournamentId);

        [Get("/tournaments/get-logo")]
        Task<IApiResponse<HttpContent>> GetTournamentLogoAsync([Query] string tournamentId);

        [Get("/tournaments/get-top-players")]
        Task<IApiResponse<SofaTopPlayersResponse>> GetTopPlayersAsync([Query] string tournamentId, [Query] string seasonId);

        [Get("/players/get-all-statistics")]
        Task<IApiResponse<SofaPlayerStatisticsSeasonsResponse>> GetPlayerHistoryStatsAsync([Query] int playerId);
    }
}

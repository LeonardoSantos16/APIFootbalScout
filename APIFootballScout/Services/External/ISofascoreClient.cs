using APIFootballScout.Models.DTOs.Sofascore.player;
using APIFootballScout.Models.DTOs.Sofascore.Tournament;
using Refit;

namespace APIFootballScout.Services.External
{
   public interface ISofascoreClient
    {
        [Get("/players/search")]
        Task<IApiResponse<SofaSearchResponse>> SearchPlayerAsync([Query] string q, [Query] string? type = "all", [Query] int? page = 0);

        [Get("/players/details")]
        Task<IApiResponse<SofaPlayerDetailsResponse>> GetPlayerDetailsAsync([Query] string id);

        [Get("/players/image")]
        Task<IApiResponse<HttpContent>> GetPlayerImageAsync([Query] string id);

        [Get("/players/statistics")]
        Task<IApiResponse<SofaSeasonStatsResponse>> GetPlayerStatisticsAsync([Query] string id, [Query] string tournamentId, [Query] string seasonId, [Query] string? type = "overall");

        [Get("/players/national-team")]
        Task<IApiResponse<SofaNationalTeamStatsResponse>> GetPlayerNationalTeamAsync([Query] string id);

        [Get("/players/transfer-history")]
        Task<IApiResponse<SofaTransferHistoryResponse>> GetPlayerTransferHistoryAsync([Query] string id);

        [Get("/tournaments/standings")]
        Task<IApiResponse<SofaStandingsResponse>> GetTournamentStandingsAsync([Query] string tournamentId, [Query] string seasonId, [Query] string? type = "total");

        [Get("/tournaments/detail")]
        Task<IApiResponse<SofaTournamentDetailResponse>> GetTournamentDetailsAsync([Query] string tournamentId);

        [Get("/tournaments/logo")]
        Task<IApiResponse<HttpContent>> GetTournamentLogoAsync([Query] string tournamentId);

        [Get("/tournaments/top-players")]
        Task<IApiResponse<SofaTopPlayersResponse>> GetTopPlayersAsync([Query] string tournamentId, [Query] string seasonId);
    }
}

using APIFootballScout.Infrastructure.SofascoreExternalAdapter.player;
using APIFootballScout.Infrastructure.SofascoreExternalAdapter.Tournament;
using Refit;

namespace APIFootballScout.Infrastructure.External
{
   public interface ISofascoreClient
    {
        [Get("/search")]
        Task<IApiResponse<SofaSearchResponse>> SofascoreSearchPlayerAsync([Query] string q, [Query] string? type = "player-team-persons", [Query] int? page = 0);

        [Get("/players/detail")]
        Task<IApiResponse<SofaPlayerDetailsResponse>> GetSofascorePlayerDetailsAsync([Query] int playerId);

        [Get("/players/get-image")]
        Task<IApiResponse<HttpContent>> GetSofascorePlayerImageAsync([Query] int id);

        [Get("/players/get-statistics-seasons")]
        Task<IApiResponse<SofaSeasonStatsResponse>> GetSofascorePlayerStatisticsSeasonAsync([Query] int playerId, [Query] string tournamentId, [Query] string seasonId, [Query] string? type = "overall");

        [Get("/players/get-national-team-statistics")]
        Task<IApiResponse<SofaNationalTeamStatsResponse>> GetSofascorePlayerNationalTeamAsync([Query] int playerId);

        [Get("/players/get-transfer-history")]
        Task<IApiResponse<SofaTransferHistoryResponse>> GetSofascorePlayerTransferHistoryAsync([Query] int playerId);

        [Get("/tournaments/get-standings")]
        Task<IApiResponse<SofaStandingsResponse>> GetSofascoreTournamentStandingsAsync([Query] int tournamentId, [Query] int seasonId, [Query] string? type = "total");

        [Get("/tournaments/detail")]
        Task<IApiResponse<SofaTournamentDetailResponse>> GetSofascoreTournamentDetailsAsync([Query] int tournamentId);

        [Get("/tournaments/get-logo")]
        Task<IApiResponse<HttpContent>> GetSofascoreTournamentLogoAsync([Query] int tournamentId);

        [Get("/tournaments/get-top-players")]
        Task<IApiResponse<SofaTopPlayersResponse>> GetSofascoreTopPlayersAsync([Query] int tournamentId, [Query] int seasonId);

        [Get("/players/get-all-statistics")]
        Task<IApiResponse<SofaPlayerStatisticsSeasonsResponse>> GetSofascorePlayerHistoryStatsAsync([Query] int playerId);
        [Get("/tournaments/get-seasons")]
        Task<IApiResponse<SofaAllSeasonResponse>> GetSofascoreTournamentSeason([Query] int tournamentId);
    }
}

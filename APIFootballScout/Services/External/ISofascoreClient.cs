using Refit;

namespace APIFootballScout.Services.External
{
   public interface ISofascoreClient
{
    [Get("/players/search")]
    Task<IApiResponse<string>> SearchPlayerAsync([Query] string q, [Query] string? type = "all", [Query] int? page = 0);

    [Get("/players/details")]
    Task<IApiResponse<string>> GetPlayerDetailsAsync([Query] string id);

    [Get("/players/image")]
    Task<IApiResponse<string>> GetPlayerImageAsync([Query] string id);

    [Get("/players/statistics")]
    Task<IApiResponse<string>> GetPlayerStatisticsAsync([Query] string id, [Query] string tournamentId, [Query] string seasonId, [Query] string? type = "overall");

    [Get("/players/national-team")]
    Task<IApiResponse<string>> GetPlayerNationalTeamAsync([Query] string id);

    [Get("/players/transfer-history")]
    Task<IApiResponse<string>> GetPlayerTransferHistoryAsync([Query] string id);

    [Get("/tournaments/standings")]
    Task<IApiResponse<string>> GetTournamentStandingsAsync([Query] string tournamentId, [Query] string seasonId, [Query] string? type = "total");

    [Get("/tournaments/detail")]
    Task<IApiResponse<string>> GetTournamentDetailsAsync([Query] string tournamentId);

    [Get("/tournaments/logo")]
    Task<IApiResponse<string>> GetTournamentLogoAsync([Query] string tournamentId);

    [Get("/tournaments/top-players")]
    Task<IApiResponse<string>> GetTopPlayersAsync([Query] string tournamentId, [Query] string seasonId);
}
}

using APIFootballScout.Infrastructure.SofascoreExternalAdapter.Tournament;

namespace APIFootballScout.Infrastructure.SofascoreExternalAdapter
{
    public interface ISofascoreTournamentReader
    {
        Task<SofaTournamentFullDTO> GetTournamentDetailAsync(int tournamentId, int? seasonId);
        Task<SofaTournamentDetailResponse?> GetDetailsAsync(int tournamentId);
        Task<SofaStandingsResponse?> GetStandingsAsync(int tournamentId, int seasonId);
        Task<SofaTopPlayersResponse?> GetTopPlayersAsync(int tournamentId, int seasonId);
        Task<SofaAllSeasonResponse?> GetSeasonsAsync(int tournamentId);
        string? GetTournamentLogoBase64Async(int tournamentId);
    }
}

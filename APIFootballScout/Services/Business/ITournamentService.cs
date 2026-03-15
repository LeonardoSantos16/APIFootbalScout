using APIFootballScout.Models.DTOs.Sofascore.Tournament;

namespace APIFootballScout.Services.Business
{
    public interface ITournamentService
    {
        Task<SofaTournamentFullDTO> GetTournamentDetailAsync(int tournamentId, int? seasonId);
    }
}

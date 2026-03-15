using APIFootballScout.Models.DTOs.Sofascore.player;

namespace APIFootballScout.Services.Business
{
    public interface IPlayerService
    {
        Task<SofaSearchResponse> SearchPlayersAsync(string name);
        Task<PlayerFullProfileDto> GetPlayerProfileAsync(int playerId);
    }
}

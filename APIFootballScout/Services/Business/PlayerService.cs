using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using APIFootballScout.Services.External;
using APIFootballScout.Models.DTOs;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;
using APIFootballScout.Models.DTOs.Sofascore.player;
using APIFootballScout.Models.Domain;
using APIFootballScout.Context;

namespace APIFootballScout.Services.Business
{
    public class PlayerService
    (ISofascoreClient sofascoreClient, AppDbContext context, IDistributedCache cache, IConfiguration config) : IPlayerService
    {
        private readonly string _apiKey = config["SofaScore:ApiKey"] ?? string.Empty;

        public async Task<SofaSearchResponse> SearchPlayersAsync(string name)
        {
            var response = await sofascoreClient.SearchPlayerAsync(name);

            return response.Content ?? new SofaSearchResponse(new List<SofaSearchResult>());
        }

        public async Task<PlayerFullProfileDto> GetPlayerProfileAsync(int playerId)
        {
            var cacheKey = $"player_profile_{playerId}";
            var cachedData = await cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<PlayerFullProfileDto>(cachedData)!;
            }
            
            var detailsTask = sofascoreClient.GetPlayerDetailsAsync(playerId);
            var transferTask = sofascoreClient.GetPlayerTransferHistoryAsync(playerId);
            var statsTask = sofascoreClient.GetPlayerHistoryStatsAsync(playerId);
            var nationalStatsTask = sofascoreClient.GetPlayerNationalTeamAsync(playerId);
            await Task.WhenAll(detailsTask, transferTask, statsTask, nationalStatsTask);

            var details = detailsTask.Result.Content?.Player;
            
            if (details != null)
            {
                var existsPlayerInDB = await context.Players.AnyAsync(player => player.ExternalId == playerId);
                if (!existsPlayerInDB)
                {
                    context.Players.Add(new Player { ExternalId = playerId, Name= details.Name, TeamName=details.Team.Name, Position= details.Position, CreatedAt= DateTime.UtcNow});
                    await context.SaveChangesAsync();
                }
            }

            var profile = new PlayerFullProfileDto
            {
                Details = details,
                Stats = statsTask.Result.Content?.Seasons,
                HistoryTransfer = transferTask.Result.Content?.Transfers,
                NationalTeamStats = nationalStatsTask.Result.Content?.Statistics,
                PlayerImage = $"https://api.sofascore.app/api/v1/player/{playerId}/image"
            };

            var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12) };

            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(profile),cacheOptions);

            return profile;
        }
    }

}

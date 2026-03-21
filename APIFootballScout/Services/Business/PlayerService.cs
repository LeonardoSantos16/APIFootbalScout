using APIFootballScout.Configuration;
using APIFootballScout.Context;
using APIFootballScout.Extensions;
using APIFootballScout.Models.Domain;
using APIFootballScout.Models.DTOs;
using APIFootballScout.Models.DTOs.Sofascore.player;
using APIFootballScout.Models.DTOs.Sofascore.Tournament;
using APIFootballScout.Services.External;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Refit;
using System.Text.Json;

namespace APIFootballScout.Services.Business
{
    public class PlayerService
    (ISofascoreClient sofascoreClient, AppDbContext context, IDistributedCache cache, IConfiguration config, IOptions<ScoutConfig> scoutOptions) : IPlayerService
    {
        private readonly string _apiKey = config["SofaScore:ApiKey"] ?? string.Empty;

        public async Task<SofaSearchResponse> SearchPlayersAsync(string name)
        {
            var response = await sofascoreClient.SofascoreSearchPlayerAsync(name);

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
            
            var detailsTask = sofascoreClient.GetSofascorePlayerDetailsAsync(playerId);
            var transferTask = sofascoreClient.GetSofascorePlayerTransferHistoryAsync(playerId);
            var statsTask = sofascoreClient.GetSofascorePlayerHistoryStatsAsync(playerId);
            var nationalStatsTask = sofascoreClient.GetSofascorePlayerNationalTeamAsync(playerId);
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

        public async Task<SofaPlayerDetailsResponse> GetPlayerDetailsAsync(int playerId)
        {
            var cacheKey = $"playerId_{playerId}_details";
            bool isPremium = scoutOptions.Value.IsPremiumPlayer(playerId);
            TimeSpan? expiration = isPremium ? TimeSpan.FromDays(7) : null;
            return await cache.GetOrFetchAsync(cacheKey, () => sofascoreClient.GetSofascorePlayerDetailsAsync(playerId), expiration);
        }

        public string GetPlayerImageAsync(int playerId)
        {
            return $"https://api.sofascore.app/api/v1/player/{playerId}/image";
        }

        public async Task<SofaSeasonStatsResponse> GetPlayerStatisticsSeasonAsync(int playerId, string tournamentId, string seasonId, [Query] string? type = "overall")
        {
            var cacheKey = $"playerId_{playerId}_statistics_season";
            bool isPremium = scoutOptions.Value.IsPremiumPlayer(playerId);
            TimeSpan? expiration = isPremium ? TimeSpan.FromDays(7) : null;
            return await cache.GetOrFetchAsync(cacheKey, () => sofascoreClient.GetSofascorePlayerStatisticsSeasonAsync(playerId, tournamentId, seasonId), expiration);
        }

        public async Task<SofaTransferHistoryResponse> GetPlayerTransferHistoryAsync(int playerId)
        {
            var cacheKey = $"playerId_{playerId}_transfer_history";
            bool isPremium = scoutOptions.Value.IsPremiumPlayer(playerId);
            TimeSpan? expiration = isPremium ? TimeSpan.FromDays(20) : null;
            return await cache.GetOrFetchAsync(cacheKey, () => sofascoreClient.GetSofascorePlayerTransferHistoryAsync(playerId), expiration);
        }

        public async Task<SofaPlayerStatisticsSeasonsResponse> GetPlayerHistoryStatsAsync(int playerId)
        {
            var cacheKey = $"playerId_{playerId}_history_stats";
            bool isPremium = scoutOptions.Value.IsPremiumPlayer(playerId);
            TimeSpan? expiration = isPremium ? TimeSpan.FromDays(1) : null;
            return await cache.GetOrFetchAsync(cacheKey, () => sofascoreClient.GetSofascorePlayerHistoryStatsAsync(playerId), expiration);
        }
    }

}

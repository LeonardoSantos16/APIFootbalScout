using APIFootballScout.Models.DTOs.Sofascore.Tournament;
using APIFootballScout.Services.External;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace APIFootballScout.Services.Business
{
    public class TournamentService(ISofascoreClient sofascoreClient, IDistributedCache cache) : ITournamentService
    {
        public async Task<SofaTournamentFullDTO> GetTournamentDetailAsync(int tournamentId, int? seasonId)
        {
            var cacheKey = $"tournament_{tournamentId}_season_{seasonId?.ToString() ?? "latest"}";

            var cachedData = await cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<SofaTournamentFullDTO>(cachedData)!;
            }

            int selectedSeasonId;

            if (seasonId == null)
            {
                var seasonsResponse = await sofascoreClient.GetTournamentSeason(tournamentId);

                if (!seasonsResponse.IsSuccessStatusCode || seasonsResponse.Content?.Seasons == null || !seasonsResponse.Content.Seasons.Any())
                {
                    throw new Exception("Não foi possível encontrar temporadas para este torneio.");
                }

                selectedSeasonId = seasonsResponse.Content.Seasons.First().Id;
            }
            else
            {
                selectedSeasonId = seasonId.Value;
            }

            var detailsTask = sofascoreClient.GetTournamentDetailsAsync(tournamentId);
            var topPlayerTask = sofascoreClient.GetTopPlayersAsync(tournamentId, selectedSeasonId);
            var standingTask = sofascoreClient.GetTournamentStandingsAsync(tournamentId, selectedSeasonId);

            await Task.WhenAll(detailsTask, topPlayerTask, standingTask);

            var tournament = new SofaTournamentFullDTO
            {
                Details = detailsTask.Result.Content?.Tournament,
                Stading = standingTask.Result.Content?.Standings,
                TopPlayers = topPlayerTask.Result.Content?.Lists
            };

            var cachedOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12) };
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(tournament), cachedOptions);

            return tournament;
        }
    }
}

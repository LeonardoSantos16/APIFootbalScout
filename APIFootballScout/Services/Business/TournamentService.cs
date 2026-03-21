using APIFootballScout.Configuration;
using APIFootballScout.Extensions;
using APIFootballScout.Models.DTOs.Sofascore.Tournament;
using APIFootballScout.Services.External;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace APIFootballScout.Services.Business
{
    public class TournamentService(ISofascoreClient sofascoreClient, IDistributedCache cache, IOptions<ScoutConfig> scoutOptions) : ITournamentService
    {
        public async Task<SofaTournamentDetailResponse?> GetDetailsAsync(int tournamentId)
        {
            var cacheKey = $"tournament_{tournamentId}_details";
            bool isPremium = scoutOptions.Value.IsPremiumTournament(tournamentId);
            TimeSpan? expiration = isPremium ? TimeSpan.FromDays(2) : null;
            return await cache.GetOrFetchAsync(cacheKey, () => sofascoreClient.GetSofascoreTournamentDetailsAsync(tournamentId), expiration);
        }

        public async Task<SofaAllSeasonResponse?> GetSeasonsAsync(int tournamentId)
        {
            var cacheKey = $"tournament_{tournamentId}_seasons";
            bool isPremium = scoutOptions.Value.IsPremiumTournament(tournamentId);
            TimeSpan? expiration = isPremium ? TimeSpan.FromDays(7) : TimeSpan.FromHours(2);

            return await cache.GetOrFetchAsync(
                cacheKey,
                () => sofascoreClient.GetSofascoreTournamentSeason(tournamentId),
                expiration
                );
        }

        public async Task<SofaStandingsResponse?> GetStandingsAsync(int tournamentId, int seasonId)
        {
            var cacheKey = $"standings_tournament_{tournamentId}_season_{seasonId}";
            bool isPremium = scoutOptions.Value.IsPremiumTournament(tournamentId);
            TimeSpan? expiration = isPremium ? TimeSpan.FromDays(5) : null;
            return await cache.GetOrFetchAsync(cacheKey, () => sofascoreClient.GetSofascoreTournamentStandingsAsync(tournamentId, seasonId, null), expiration);
        }

        public async Task<SofaTopPlayersResponse?> GetTopPlayersAsync(int tournamentId, int seasonId)
        {
            var cacheKey = $"topPlayer_tournament_{tournamentId}_season+{seasonId}";
            bool isPremium = scoutOptions.Value.IsPremiumTournament(tournamentId);
            TimeSpan? expiration = isPremium ? TimeSpan.FromDays(2) : null;
            return await cache.GetOrFetchAsync(cacheKey, () => sofascoreClient.GetSofascoreTopPlayersAsync(tournamentId, seasonId), expiration);
        }

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
                var seasonsResponse = await sofascoreClient.GetSofascoreTournamentSeason(tournamentId);

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

            var detailsTask = sofascoreClient.GetSofascoreTournamentDetailsAsync(tournamentId);
            var topPlayerTask = sofascoreClient.GetSofascoreTopPlayersAsync(tournamentId, selectedSeasonId);
            var standingTask = sofascoreClient.GetSofascoreTournamentStandingsAsync(tournamentId, selectedSeasonId);

            await Task.WhenAll(detailsTask, topPlayerTask, standingTask);

            var tournament = new SofaTournamentFullDTO
            {
                Details = detailsTask.Result.Content?.Tournament,
                Stading = standingTask.Result.Content?.Standings,
                TopPlayers = topPlayerTask.Result.Content?.Lists,
                Image = $"https://api.sofascore.app/api/v1/unique-tournament/{tournamentId}/image"
            };

            var cachedOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12) };
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(tournament), cachedOptions);

            return tournament;
        }

        public string? GetTournamentLogoBase64Async(int tournamentId)
        {
            return $"https://api.sofascore.app/api/v1/unique-tournament/{tournamentId}/image";
        }

    }
}

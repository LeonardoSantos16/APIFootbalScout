using System.Text.Json.Serialization;

namespace APIFootballScout.Infrastructure.SofascoreExternalAdapter.player
{
    public record SofaPlayerStatisticsSeasonsResponse(
        [property: JsonPropertyName("seasons")] List<SofaSeasonStatisticItem> Seasons
    );

    public record SofaSeasonStatisticItem(
        [property: JsonPropertyName("year")] string Year,
        [property: JsonPropertyName("team")] SofaTeam Team,
        [property: JsonPropertyName("uniqueTournament")] SofaTournamentSummary UniqueTournament,
        [property: JsonPropertyName("statistics")] SofaHistoricalStats Statistics,
        [property: JsonPropertyName("season")] SofaSeasonInfo Season
    );

    public record SofaHistoricalStats(
        [property: JsonPropertyName("rating")] double Rating,
        [property: JsonPropertyName("appearances")] int Appearances,
        [property: JsonPropertyName("goals")] int Goals,
        [property: JsonPropertyName("assists")] int Assists,
        [property: JsonPropertyName("minutesPlayed")] int MinutesPlayed,
        [property: JsonPropertyName("expectedGoals")] double? ExpectedGoals,
        [property: JsonPropertyName("expectedAssists")] double? ExpectedAssists,
        [property: JsonPropertyName("accuratePassesPercentage")] double AccuratePassesPercentage,
        [property: JsonPropertyName("keyPasses")] int KeyPasses,
        [property: JsonPropertyName("tackles")] int Tackles,
        [property: JsonPropertyName("interceptions")] int Interceptions,
        [property: JsonPropertyName("yellowCards")] int YellowCards,
        [property: JsonPropertyName("redCards")] int RedCards
    );

    public record SofaTournamentSummary(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("competitionType")] string CompetitionType,
        [property: JsonPropertyName("category")] SofaCategorySummary Category
    );

    public record SofaCategorySummary(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("flag")] string? Flag
    );

    public record SofaSeasonInfo(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name
    );

}

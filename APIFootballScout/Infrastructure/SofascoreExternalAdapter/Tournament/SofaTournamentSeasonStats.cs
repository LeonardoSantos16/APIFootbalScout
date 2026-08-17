using System.Text.Json.Serialization;

namespace APIFootballScout.Infrastructure.SofascoreExternalAdapter.Tournament
{
    public record SofaDetailedSeasonStatsResponse(
        [property: JsonPropertyName("results")] List<SofaDetailedPlayerStat> Results
    );

    public record SofaDetailedPlayerStat(
        [property: JsonPropertyName("player")] SofaPlayerSummary Player,
        [property: JsonPropertyName("team")] SofaTeam Team,
        [property: JsonPropertyName("goals")] int Goals,
        [property: JsonPropertyName("expectedGoals")] double ExpectedGoals,
        [property: JsonPropertyName("assists")] int Assists,
        [property: JsonPropertyName("successfulDribbles")] int SuccessfulDribbles,
        [property: JsonPropertyName("tackles")] int Tackles,
        [property: JsonPropertyName("accuratePassesPercentage")] double AccuratePassesPercentage,
        [property: JsonPropertyName("rating")] double Rating
    );

    public record SofaTeam(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name
    );

}

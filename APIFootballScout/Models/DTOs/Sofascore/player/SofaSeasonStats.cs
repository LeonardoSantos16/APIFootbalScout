using System.Text.Json.Serialization;

namespace APIFootballScout.Models.DTOs.Sofascore.player
{
    public record SofaSeasonStatsResponse(
        [property: JsonPropertyName("statistics")] SofaStatistics Statistics,
        [property: JsonPropertyName("team")] SofaTeamSummary Team
    );

    public record SofaStatistics(
        [property: JsonPropertyName("rating")] double Rating,
        [property: JsonPropertyName("appearances")] int Appearances,
        [property: JsonPropertyName("minutesPlayed")] int MinutesPlayed,
        [property: JsonPropertyName("goals")] int Goals,
        [property: JsonPropertyName("assists")] int Assists,
        [property: JsonPropertyName("bigChancesCreated")] int BigChancesCreated,
        [property: JsonPropertyName("keyPasses")] int KeyPasses,
        [property: JsonPropertyName("totalPasses")] int TotalPasses,
        [property: JsonPropertyName("accuratePassesPercentage")] double AccuratePassesPercentage,
        [property: JsonPropertyName("successfulDribbles")] int SuccessfulDribbles,
        [property: JsonPropertyName("tackles")] int Tackles,
        [property: JsonPropertyName("interceptions")] int Interceptions,
        [property: JsonPropertyName("totalDuelsWon")] int TotalDuelsWon,
        [property: JsonPropertyName("groundDuelsWon")] int GroundDuelsWon,
        [property: JsonPropertyName("aerialDuelsWon")] int AerialDuelsWon,
        [property: JsonPropertyName("yellowCards")] int YellowCards,
        [property: JsonPropertyName("redCards")] int RedCards,
        [property: JsonPropertyName("ballRecovery")] int BallRecovery,
        [property: JsonPropertyName("id")] int Id
    );

    public record SofaTeamSummary(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("nameCode")] string NameCode
    );
}

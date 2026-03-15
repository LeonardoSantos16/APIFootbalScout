using System.Text.Json.Serialization;

namespace APIFootballScout.Models.DTOs.Sofascore.player
{
    public record SofaNationalTeamStatsResponse(
        [property: JsonPropertyName("statistics")] List<SofaNationalTeamStats> Statistics
    );

    public record SofaNationalTeamStats(
        [property: JsonPropertyName("team")] SofaNationalTeam Team,
        [property: JsonPropertyName("appearances")] int Appearances,
        [property: JsonPropertyName("goals")] int Goals,
        [property: JsonPropertyName("debutTimestamp")] long DebutTimestamp
    );

    public record SofaNationalTeam(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("nameCode")] string NameCode,
        [property: JsonPropertyName("ranking")] int? Ranking,
        [property: JsonPropertyName("national")] bool IsNational
    );
}

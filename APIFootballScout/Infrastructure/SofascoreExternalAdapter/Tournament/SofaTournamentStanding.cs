using APIFootballScout.Infrastructure.SofascoreExternalAdapter.player;
using System.Text.Json.Serialization;

namespace APIFootballScout.Infrastructure.SofascoreExternalAdapter.Tournament
{
    public record SofaStandingsResponse(
        [property: JsonPropertyName("standings")] List<SofaStandingGroup> Standings
    );

    public record SofaStandingGroup(
        [property: JsonPropertyName("rows")] List<SofaStandingRow> Rows,
        [property: JsonPropertyName("type")] string Type
    );

    public record SofaStandingRow(
        [property: JsonPropertyName("position")] int Position,
        [property: JsonPropertyName("team")] SofaTeamSummary Team,
        [property: JsonPropertyName("matches")] int Matches,
        [property: JsonPropertyName("wins")] int Wins,
        [property: JsonPropertyName("draws")] int Draws,
        [property: JsonPropertyName("losses")] int Losses,
        [property: JsonPropertyName("scoresFor")] int GoalsScored,
        [property: JsonPropertyName("scoresAgainst")] int GoalsConceded,
        [property: JsonPropertyName("points")] int Points,
        [property: JsonPropertyName("scoreDiffFormatted")] string GoalDifference,
        [property: JsonPropertyName("promotion")] SofaPromotion? Promotion
    );

    public record SofaPromotion(
        [property: JsonPropertyName("text")] string Name
    );
}

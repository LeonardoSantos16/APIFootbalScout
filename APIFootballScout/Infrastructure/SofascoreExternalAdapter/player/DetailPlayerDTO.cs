using System.Text.Json.Serialization;

namespace APIFootballScout.Infrastructure.SofascoreExternalAdapter.player
{
    public record SofaPlayerDetailsResponse(
        [property: JsonPropertyName("player")] SofaPlayerDetail Player
    );

    public record SofaPlayerDetail(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("slug")] string Slug,
        [property: JsonPropertyName("team")] SofaTeamDetail Team,
        [property: JsonPropertyName("position")] string Position,
        [property: JsonPropertyName("positionsDetailed")] string[] PositionsDetailed,
        [property: JsonPropertyName("jerseyNumber")] string? JerseyNumber,
        [property: JsonPropertyName("height")] int Height,
        [property: JsonPropertyName("dateOfBirth")] DateTime DateOfBirth,
        [property: JsonPropertyName("preferredFoot")] string? PreferredFoot,
        [property: JsonPropertyName("userCount")] int UserCount,
        [property: JsonPropertyName("proposedMarketValue")] long ProposedMarketValue,
        [property: JsonPropertyName("proposedMarketValueRaw")] SofaMarketValue? ProposedMarketValueRaw
    );

    public record SofaTeamDetail(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("nameCode")] string NameCode,
        [property: JsonPropertyName("tournament")] SofaTournament Tournament,
        [property: JsonPropertyName("national")] bool National,
        [property: JsonPropertyName("userCount")] int UserCount
    );

    public record SofaTournament(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name
    );

    public record SofaMarketValue(
        [property: JsonPropertyName("value")] long Value,
        [property: JsonPropertyName("currency")] string Currency
    );
}

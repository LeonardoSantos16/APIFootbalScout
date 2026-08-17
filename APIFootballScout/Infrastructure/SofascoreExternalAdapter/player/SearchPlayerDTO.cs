using System.Text.Json.Serialization;

namespace APIFootballScout.Infrastructure.SofascoreExternalAdapter.player
{
    public record SofaSearchResponse(
        [property: JsonPropertyName("results")] List<SofaSearchResult> Results
    );

    public record SofaSearchResult(
        [property: JsonPropertyName("entity")] SofaEntity Entity,
        [property: JsonPropertyName("score")] float Score,
        [property: JsonPropertyName("type")] string Type
    );

    public record SofaEntity(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("shortName")] string ShortName,
        [property: JsonPropertyName("team")] SofaTeam? Team,
        [property: JsonPropertyName("country")] SofaCountry? Country,
        [property: JsonPropertyName("position")] string? Position,
        [property: JsonPropertyName("jerseyNumber")] string? JerseyNumber,
        [property: JsonPropertyName("sofascoreId")] string? SofascoreId
    );

    public record SofaTeam(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("gender")] string Gender
    );

    public record SofaCountry(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("alpha2")] string Alpha2
    );
}


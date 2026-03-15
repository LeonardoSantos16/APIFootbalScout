using APIFootballScout.Models.DTOs.Sofascore.player;
using System.Text.Json.Serialization;

namespace APIFootballScout.Models.DTOs.Sofascore.Tournament
{
    public record SofaTournamentDetailResponse(
        [property: JsonPropertyName("uniqueTournament")] SofaUniqueTournament Tournament
    );

    public record SofaUniqueTournament(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("category")] SofaCategory Category,
        [property: JsonPropertyName("tier")] int Tier,
        [property: JsonPropertyName("userCount")] int UserCount,
        [property: JsonPropertyName("titleHolder")] SofaTeamSummary? TitleHolder,
        [property: JsonPropertyName("mostTitlesTeams")] List<SofaTeamSummary> MostTitlesTeams,
        [property: JsonPropertyName("startDateTimestamp")] long? StartDateTimestamp,
        [property: JsonPropertyName("endDateTimestamp")] long? EndDateTimestamp
    );

    public record SofaCategory(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("country")] SofaCountrySummary Country,
        [property: JsonPropertyName("flag")] string? Flag
    );

    public record SofaCountrySummary(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("alpha2")] string Alpha2
    );

}

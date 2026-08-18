using System.Text.Json.Serialization;

namespace APIFootballScout.Infrastructure.SofascoreExternalAdapter.Tournament
{
    public record SofaAllSeasonResponse(
        [property: JsonPropertyName("seasons")] List<SofaTournamentSeason> Seasons
    );

    public record SofaTournamentSeason(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("year")] string Year,
        [property: JsonPropertyName("editor")] bool Editor
    );

}

using APIFootballScout.Models.DTOs.Sofascore.player;
using System.Text.Json.Serialization;

namespace APIFootballScout.Models.DTOs.Sofascore.Tournament
{
    public record SofaTopPlayersResponse(
        [property: JsonPropertyName("topPlayers")] SofaTopPlayers Lists
    );

    public record SofaTopPlayers(
        [property: JsonPropertyName("rating")] List<SofaTopPlayerItem> Rating
    );

    public record SofaTopPlayerItem(
        [property: JsonPropertyName("player")] SofaPlayerSummary Player,
        [property: JsonPropertyName("team")] SofaTeamSummary Team,
        [property: JsonPropertyName("statistics")] SofaTopPlayerStats Statistics,
        [property: JsonPropertyName("playedEnough")] bool PlayedEnough
    );

    public record SofaPlayerSummary(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("slug")] string Slug,
        [property: JsonPropertyName("position")] string Position
    );

    public record SofaTopPlayerStats(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("rating")] double Rating,
        [property: JsonPropertyName("appearances")] int Appearances
    );
}

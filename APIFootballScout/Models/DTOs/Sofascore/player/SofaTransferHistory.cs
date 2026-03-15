using System.Text.Json.Serialization;

namespace APIFootballScout.Models.DTOs.Sofascore.player
{
    public record SofaTransferHistoryResponse(
        [property: JsonPropertyName("transferHistory")] List<SofaTransfer> Transfers
    );

    public record SofaTransfer(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("fromTeamName")] string FromTeamName,
        [property: JsonPropertyName("toTeamName")] string ToTeamName,
        [property: JsonPropertyName("transferFrom")] SofaTransferTeam TransferFrom,
        [property: JsonPropertyName("transferTo")] SofaTransferTeam TransferTo,
        [property: JsonPropertyName("transferFeeDescription")] string? TransferFeeDescription,
        [property: JsonPropertyName("transferDateTimestamp")] long TransferDateTimestamp,
        [property: JsonPropertyName("transferFeeRaw")] SofaTransferFee? TransferFeeRaw
    );

    public record SofaTransferTeam(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("shortName")] string ShortName
    );

    public record SofaTransferFee(
        [property: JsonPropertyName("value")] long Value,
        [property: JsonPropertyName("currency")] string Currency
    );
}

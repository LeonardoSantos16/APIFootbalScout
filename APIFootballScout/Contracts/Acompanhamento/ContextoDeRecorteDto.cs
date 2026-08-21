using System.Text.Json.Serialization;

namespace APIFootballScout.Contracts.Acompanhamento
{
    [JsonConverter(typeof(JsonStringEnumConverter<ContextoDeRecorteDto>))]
    public enum ContextoDeRecorteDto
    {
        Clube = 1,
        Selecao = 2
    }
}

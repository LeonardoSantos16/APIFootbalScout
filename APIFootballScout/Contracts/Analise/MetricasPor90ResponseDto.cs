using System.Text.Json.Serialization;
using APIFootballScout.Contracts.Acompanhamento;

namespace APIFootballScout.Contracts.Analise
{
    [JsonConverter(typeof(JsonStringEnumConverter<ResultadoDoCalculoDto>))]
    public enum ResultadoDoCalculoDto
    {
        Calculada = 1,
        Recusada = 2
    }

    [JsonConverter(typeof(JsonStringEnumConverter<MotivoDaRecusaDto>))]
    public enum MotivoDaRecusaDto
    {
        AmostraInsuficiente = 1
    }

    [JsonConverter(typeof(JsonStringEnumConverter<TipoDeAtributoDto>))]
    public enum TipoDeAtributoDto
    {
        Gols = 1,
        Assistencias = 2,
        PassesDecisivos = 3,
        Desarmes = 4,
        Interceptacoes = 5,
        Rating = 6,
        PrecisaoDePasse = 7
    }

    public sealed record RecorteDto(int CompeticaoId, int TemporadaId, ContextoDeRecorteDto Contexto);

    public sealed record MetricaPor90Dto(
        TipoDeAtributoDto Tipo,
        ResultadoDoCalculoDto Resultado,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? Valor = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? AmostraEmMinutos = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] MotivoDaRecusaDto? Motivo = null);

    public sealed record ValorDerivadoDto(TipoDeAtributoDto Tipo, decimal Valor);

    public sealed record MetricasPor90ResponseDto(
        int JogadorId,
        RecorteDto Recorte,
        IReadOnlyCollection<MetricaPor90Dto> Metricas,
        IReadOnlyCollection<ValorDerivadoDto> Derivados);
}

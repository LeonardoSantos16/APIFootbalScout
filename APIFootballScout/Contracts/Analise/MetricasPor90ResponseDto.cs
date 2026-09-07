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

    [JsonConverter(typeof(JsonStringEnumConverter<TipoDeEstatisticaDto>))]
    public enum TipoDeEstatisticaDto
    {
        Gols = 1,
        Assistencias = 2,
        PassesDecisivos = 3,
        Desarmes = 4,
        Interceptacoes = 5
    }

    [JsonConverter(typeof(JsonStringEnumConverter<TipoDeValorDerivadoDto>))]
    public enum TipoDeValorDerivadoDto
    {
        Rating = 1,
        PrecisaoDePasse = 2
    }

    public sealed record RecorteDto(int CompeticaoId, int TemporadaId, ContextoDeRecorteDto Contexto);

    public sealed record MetricaPor90Dto(
        TipoDeEstatisticaDto Tipo,
        ResultadoDoCalculoDto Resultado,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? Valor = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? AmostraEmMinutos = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] MotivoDaRecusaDto? Motivo = null);

    public sealed record ValorDerivadoDto(TipoDeValorDerivadoDto Tipo, decimal Valor);

    public sealed record MetricasPor90ResponseDto(
        int JogadorId,
        RecorteDto Recorte,
        IReadOnlyCollection<MetricaPor90Dto> Metricas,
        IReadOnlyCollection<ValorDerivadoDto> Derivados);
}

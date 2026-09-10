using System.Text.Json.Serialization;

namespace APIFootballScout.Contracts.Analise
{
    [JsonConverter(typeof(JsonStringEnumConverter<PosicaoDto>))]
    public enum PosicaoDto
    {
        Goleiro = 1,
        Defesa = 2,
        MeioCampo = 3,
        Ataque = 4
    }

    [JsonConverter(typeof(JsonStringEnumConverter<ResultadoDaComparacaoDto>))]
    public enum ResultadoDaComparacaoDto
    {
        Realizada = 1,
        Recusada = 2
    }

    [JsonConverter(typeof(JsonStringEnumConverter<MotivoDaRecusaDaComparacaoDto>))]
    public enum MotivoDaRecusaDaComparacaoDto
    {
        PosicoesIncompativeis = 1,
        PosicaoDesconhecida = 2,
        RecorteDivergente = 3,
        AmostraInsuficiente = 4
    }

    public sealed record JogadorNaComparacaoDto(
        string Nome,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] PosicaoDto? Posicao = null);

    public sealed record ValorComparadoDto(
        ResultadoDoCalculoDto Resultado,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? Valor = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? AmostraEmMinutos = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] MotivoDaRecusaDto? Motivo = null);

    public sealed record AtributoComparadoDto(
        TipoDeAtributoDto Tipo,
        IReadOnlyDictionary<int, ValorComparadoDto> Valores);

    public sealed record ComparacaoDiretaResponseDto(
        IReadOnlyDictionary<int, JogadorNaComparacaoDto> Jogadores,
        ResultadoDaComparacaoDto Resultado,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] RecorteDto? Recorte = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] MotivoDaRecusaDaComparacaoDto? Motivo = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyCollection<AtributoComparadoDto>? Atributos = null);
}

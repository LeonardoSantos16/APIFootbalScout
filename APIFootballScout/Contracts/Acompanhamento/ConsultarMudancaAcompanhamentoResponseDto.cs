using System.Text.Json.Serialization;

namespace APIFootballScout.Contracts.Acompanhamento
{
    [JsonConverter(typeof(JsonStringEnumConverter<ResultadoDaAfericaoDto>))]
    public enum ResultadoDaAfericaoDto
    {
        ComMudanca = 1,
        SemMudancaRelevante = 2,
        Indisponivel = 3
    }

    [JsonConverter(typeof(JsonStringEnumConverter<MotivoDeIndisponibilidadeDto>))]
    public enum MotivoDeIndisponibilidadeDto
    {
        MoedaInesperada = 1,
        TemporadaVirada = 2
    }

    public sealed record DinheiroDto(long QuantiaEmCentavos, string Moeda);

    public sealed record JanelaDaComparacaoDto(DateTime De, DateTime Ate, double DuracaoEmDias);

    public sealed record AfericaoDeClubeDto(
        ResultadoDaAfericaoDto Resultado,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Anterior = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Atual = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] MotivoDeIndisponibilidadeDto? Motivo = null);

    public sealed record AfericaoDeValorDeMercadoDto(
        ResultadoDaAfericaoDto Resultado,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DinheiroDto? Anterior = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DinheiroDto? Atual = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? VariacaoPercentualAbsoluta = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] MotivoDeIndisponibilidadeDto? Motivo = null);

    public sealed record AfericaoDeMinutagemDto(
        ResultadoDaAfericaoDto Resultado,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? Anterior = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? Atual = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? VariacaoAbsoluta = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] MotivoDeIndisponibilidadeDto? Motivo = null);

    public sealed record ConsultarMudancaAcompanhamentoResponseDto(
        Guid DossieId,
        int JogadorId,
        JanelaDaComparacaoDto Janela,
        AfericaoDeClubeDto Clube,
        AfericaoDeValorDeMercadoDto ValorDeMercado,
        AfericaoDeMinutagemDto Minutagem);
}

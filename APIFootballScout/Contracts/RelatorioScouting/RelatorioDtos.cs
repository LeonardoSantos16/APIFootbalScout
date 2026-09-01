using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIFootballScout.Contracts.RelatorioScouting
{
    [JsonConverter(typeof(JsonStringEnumConverter<ParecerDto>))]
    public enum ParecerDto
    {
        Contratar = 1,
        Monitorar = 2,
        Reavaliar = 3,
        Descartar = 4
    }

    [JsonConverter(typeof(JsonStringEnumConverter<StatusRelatorioDto>))]
    public enum StatusRelatorioDto
    {
        Rascunho = 1,
        Finalizado = 2
    }

    public sealed record AbrirRascunhoRelatorioRequestDto
    {
        [Range(1, int.MaxValue)]
        public int JogadorId { get; init; }

        [Required]
        public string Texto { get; init; } = string.Empty;

        [Required]
        public DateTimeOffset ObservadoEm { get; init; }
    }

    public sealed record EditarRascunhoRelatorioRequestDto
    {
        public string? Texto { get; init; }
        public decimal? Nota { get; init; }
        public IReadOnlyList<string>? PontosPositivos { get; init; }
        public IReadOnlyList<string>? PontosNegativos { get; init; }
        public ParecerDto? Parecer { get; init; }
    }

    public sealed record RelatorioResponseDto(
        Guid RelatorioId,
        int JogadorId,
        StatusRelatorioDto Status,
        string Texto,
        DateTimeOffset ObservadoEm,
        DateTimeOffset EscritoEm,
        IReadOnlyList<string> PontosPositivos,
        IReadOnlyList<string> PontosNegativos,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? Nota = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] ParecerDto? Parecer = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DateTimeOffset? FinalizadoEm = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Guid? CorrigeRelatorioId = null);
}

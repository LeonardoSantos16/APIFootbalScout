using APIFootballScout.Contracts.Acompanhamento;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIFootballScout.Contracts.ShortlistPersonalizada
{
    public sealed record CriarShortlistRequestDto
    {
        [Required]
        public string Nome { get; init; } = string.Empty;
    }

    public sealed record AdicionarAlvoRequestDto
    {
        [Range(1, int.MaxValue)]
        public int JogadorId { get; init; }

        [Range(1, int.MaxValue)]
        public int Prioridade { get; init; }

        [Required]
        public DinheiroDto CustoEstimado { get; init; } = default!;
    }

    public sealed record RepriorizarAlvoRequestDto
    {
        [Range(1, int.MaxValue)]
        public int Prioridade { get; init; }
    }

    public sealed record AlvoResponseDto(
        int JogadorId,
        int Prioridade,
        DinheiroDto CustoEstimado);

    public sealed record ShortlistResponseDto(
        Guid ShortlistId,
        string Nome,
        int LimiteDeAlvos,
        IReadOnlyList<AlvoResponseDto> Alvos,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DinheiroDto? CustoTotal = null);
}

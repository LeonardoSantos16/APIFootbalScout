using System.ComponentModel.DataAnnotations;

namespace APIFootballScout.Contracts.Acompanhamento
{
    public sealed record AbrirAcompanhamentoRequestDto
    {
        [Required]
        public Guid OlheiroId { get; init; }

        [Range(1, int.MaxValue)]
        public int JogadorId { get; init; }

        [Range(1, int.MaxValue)]
        public int CompeticaoId { get; init; }

        [Range(1, int.MaxValue)]
        public int TemporadaId { get; init; }

        [Required]
        [EnumDataType(typeof(ContextoDeRecorteDto))]
        public ContextoDeRecorteDto Contexto { get; init; }
    }
}
